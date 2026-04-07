using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 전체에서 공통으로 유지되어야 하는 플레이어 데이터와 설정값을 관리하는 싱글톤 매니저
/// 
/// 이 클래스가 담당하는 것
/// 1. 로그인/회원가입 이후 유지해야 하는 플레이어 정보
///    - userId
///    - loginId
///    - nickname
///    - petshopName

/// 2. 옵션 설정 
///    - BGM 볼륨
///    - SFX 볼륨
///
/// 3. 메일 알림 상태 보관
///    - 읽지 않은 쪽지가 있는지 여부
///
/// 참고:
/// - 이 클래스는 "DB 전체"를 들고 있는 클래스가 아니다.
/// - DB에서 자주 참조되는 현재 유저의 핵심 정보만 캐싱한다.
/// - Pet 목록, Item 목록, Mail 전체 목록은 각각 별도 Manager에서 관리하는 것이 더 적절하다.

public class DataManager : MonoBehaviour
{

    /// 전역 접근용 싱글톤 인스턴스
    public static DataManager Data { get; private set; }

    // ─────────────────────────────────────────────
    // 1. 플레이어 프로필 데이터

    [Header("Current User Session")]

    /// DB의 User.userId 에 해당하는 값.
    /// 현재 로그인한 유저를 식별하는 기본 키이다.
    /// 로그인되지 않은 상태는 -1로 둔다.
    [SerializeField] private int _userId = -1;

    /// DB의 User.id 에 해당하는 값.
    /// 사용자가 로그인할 때 사용하는 문자열 ID이다.
    [SerializeField] private string _loginId = string.Empty;

    /// DB의 User.nickname 에 해당하는 값.
    /// 게임 화면에서 표시되는 유저 닉네임이다.
    [SerializeField] private string _nickname = "Player";

    ///DB의 User.shopName에 해당하는 값
    /// 펫샵 이름으로, 간판 UI 등에 표시할 수 있다.
    [SerializeField] private string _petShopName = "My PetShop";
    /// 외부에서는 읽기 전용으로 접근하도록 프로퍼티 제공
    public int UserId => _userId;
    public string LoginId => _loginId;
    public string Nickname => _nickname;
    public string PetShopName => _petShopName;

    // ─────────────────────────────────────────────
    // 2. 사운드 설정 데이터 

    [Header("Audio Settings")]


    /// BGM 볼륨(0~100)
    [SerializeField][Range(0, 100)] private int _bgmVolumeLevel = 80;

    /// 효과음 볼륨(0~100)
    [SerializeField][Range(0, 100)] private int _sfxVolumeLevel = 80;

    /// 실제 AudioSource.volume에 넣기 위한 0.0~1.0 값
    private float _bgmVolume => _bgmVolumeLevel / 100f;

    /// 실제 AudioSource.volume에 넣기 위한 0.0~1.0 값
    private float _sfxVolume => _sfxVolumeLevel / 100f;

    // ─────────────────────────────────────────────
    // 3. 쪽지 / 접속 상태 데이터

    [Header("Mail State")]

    /// <summary>
    /// 읽지 않은 메일이 하나라도 있는지 여부.
    /// DB의 Mail.isRead 값을 서버가 계산한 뒤,
    /// 클라이언트에서는 요약 상태만 들고 있는 형태로 사용 가능하다.
    [SerializeField] private bool _hasUnreadMail = false;

    /// <summary>
    /// 외부 읽기 전용 프로퍼티.
    public bool HasUnreadMail => _hasUnreadMail;

    // ─────────────────────────────────────────────
    // 4. 이벤트

    /// BGM 볼륨이 바뀌었을 때 AudioManager들이 구독해서 사용
    public static event Action<float> OnBgmVolumeChanged;

    /// SFX 볼륨이 바뀌었을 때 AudioManager들이 구독해서 사용
    public static event Action<float> OnSfxVolumeChanged;

    /// 일지 않은 메일 여부가 바뀌었을 때 호출되는 이벤트
    /// 메일 아이콘 알림 UI 등에 사용할 수 있다.
    public static event Action<bool> OnUnreadMailStateChanged;

    // ─────────────────────────────────────────────
    // 5. 유니티 생명주기

    private void Awake()
    {
        // 이미 다른 DataManager가 존재하면 현재 오브젝트는 제거
        if (Data != null && Data != this)
        {
            Destroy(gameObject);
            return;
        }

        // 싱글톤 인스턴스 등록
        Data = this;

        // 씬이 바뀌어도 살아남도록 설정
        DontDestroyOnLoad(gameObject);

        // 새 씬이 로드될 때마다 현재 설정값을 다시 브로드캐스트하기 위해 구독
        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    /// 새 씬이 로드될 때 호출됨
    /// 
    /// 이유:
    /// 씬마다 AudioManager 또는 UI가 새로 생성될 수 있으므로
    /// 현재 저장 중인 볼륨값/쪽지 상태를 다시 알려줄 필요가 있음
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BroadcastAudioSettings();
        BroadcastMailState();
        BroadcastProfileState();
    }

    /// <summary>
    /// 인스펙터에서 값이 바뀔 때 범위를 강제로 맞춰줌
    /// 
    /// 참고:
    /// 에디터에서 테스트할 때 잘못된 값이 들어가는 것을 방지함
    /// </summary>
    private void OnValidate()
    {
        _bgmVolumeLevel = Mathf.Clamp(_bgmVolumeLevel, 0, 100);
        _sfxVolumeLevel = Mathf.Clamp(_sfxVolumeLevel, 0, 100);

        // 플레이 중일 때만 즉시 반영
        if (Application.isPlaying)
        {
            BroadcastAudioSettings();
            BroadcastMailState();
            BroadcastProfileState();
        }
    }

    private void OnDestroy()
    {
        // 현재 인스턴스가 파괴될 때만 이벤트/구독 정리
        if (Data == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            OnBgmVolumeChanged = null;
            OnSfxVolumeChanged = null;
            OnProfileChanged = null;
            OnUnreadMailStateChanged = null;
        }
    }

    // ─────────────────────────────────────────────
    // 6. 플레이어 프로필 관련 메서드
    // ─────────────────────────────────────────────

    /// <summary>
    /// 회원가입/로그인 이후 플레이어 정보를 한 번에 설정
    /// 
    /// userId      : DB 기본 키
    /// loginId     : 로그인용 문자열 ID
    /// nickname    : 인게임 닉네임
    /// petShopName : 펫샵 이름

    public void SetUserSession(int userId, string loginId, string nickname, string petShopName)
    {
        _userId = userId;
        _loginId = string.IsNullOrWhiteSpace(loginId) ? string.Empty : loginId.Trim();
        _nickname = string.IsNullOrWhiteSpace(nickname) ? "Player" : nickname.Trim();
        _petShopName = string.IsNullOrWhiteSpace(petShopName) ? "My PetShop" : petShopName.Trim();

        // 프로필 관련 UI들이 갱신될 수 있도록 이벤트 호출.
        BroadcastProfileState();
    }


    /// 현재 프로필 상태를 구독 중인 UI에게 다시 알림
    /// 
    /// 사용 예:
    /// - PetTown 간판 UI
    /// - 플레이어 닉네임 UI
    public static event Action OnProfileChanged;

    public void BroadcastProfileState()
    {
        OnProfileChanged?.Invoke();
    }

    /// 로그아웃 시 현재 유저 세션 정보를 초기화한다.
    public void ClearUserSession()
    {
        _userId = -1;
        _loginId = string.Empty;
        _nickname = "Player";
        _petShopName = "My PetShop";
        _hasUnreadMail = false;

        BroadcastProfileState();
        BroadcastMailState();
    }

    // ─────────────────────────────────────────────
    // 7. 볼륨 관련 메서드

    /// BGM 볼륨 설정
    /// 0~100 범위로 자동 보정 후 이벤트 발행
    public void SetBgmVolume(int volumeLevel)
    {
        int newLevel = Mathf.Clamp(volumeLevel, 0, 100);

        // 값이 같으면 불필요한 이벤트 호출 방지
        if (_bgmVolumeLevel == newLevel)
            return;

        _bgmVolumeLevel = newLevel;
        OnBgmVolumeChanged?.Invoke(_bgmVolume);
    }


    /// SFX 볼륨 설정
    /// 0~100 범위로 자동 보정 후 이벤트 발행
    public void SetSfxVolume(int volumeLevel)
    {
        int newLevel = Mathf.Clamp(volumeLevel, 0, 100);

        if (_sfxVolumeLevel == newLevel)
            return;

        _sfxVolumeLevel = newLevel;
        OnSfxVolumeChanged?.Invoke(_sfxVolume);
    }

    /// 현재 저장된 볼륨값을 한 번에 다시 브로드캐스트
    /// 
    /// 사용 이유:
    /// 씬이 바뀌고 새 AudioManager가 생성되었을 때
    /// 현재 값을 다시 적용하기 위해 사용
    /// </summary>
    public void BroadcastAudioSettings()
    {
        OnBgmVolumeChanged?.Invoke(_bgmVolume);
        OnSfxVolumeChanged?.Invoke(_sfxVolume);
    }


    /// UI 슬라이더 초기값 설정 등에 사용할 getter
    public float GetBgmVolume() => _bgmVolume;
    public float GetSfxVolume() => _sfxVolume;
    public int GetBgmVolumeLevel() => _bgmVolumeLevel;
    public int GetSfxVolumeLevel() => _sfxVolumeLevel;

    // ─────────────────────────────────────────────
    // 8. 쪽지 관련 메서드

    /// 새 쪽지 여부 설정
    /// 
    /// true  -> 메뉴 쪽지 아이콘에 알림 표시 가능
    /// false -> 알림 제거
    public void SetUnreadMailState(bool hasUnreadMail)
    {
        if (_hasUnreadMail == hasUnreadMail)
            return;

        _hasUnreadMail = hasUnreadMail;
        OnUnreadMailStateChanged?.Invoke(_hasUnreadMail);
    }

    /// 현재 새 쪽지 여부를 구독 중인 UI에게 다시 알림
    /// 씬 진입 직후 메뉴 UI 갱신용
    public void BroadcastMailState()
    {
        OnUnreadMailStateChanged?.Invoke(_hasUnreadMail);
    }

    /// 쪽지함을 열었을 때 호출 가능
    /// 읽지 않은 쪽지 알림을 꺼줌
    public void MarkAllMailAsRead()
    {
        SetUnreadMailState(false);
    }

    // =========================================================
    // 9. 초기화 관련 메서드

    /// <summary>
    /// 테스트용 기본 상태로 초기화한다.
    /// 로그인 전 상태를 가정한 기본값들로 되돌린다.
    public void InitializeDefaultData()
    {
        _userId = -1;
        _loginId = string.Empty;
        _nickname = "Player";
        _petShopName = "My PetShop";

        _bgmVolumeLevel = 80;
        _sfxVolumeLevel = 80;

        _hasUnreadMail = false;

        BroadcastProfileState();
        BroadcastAudioSettings();
        BroadcastMailState();
    }
}