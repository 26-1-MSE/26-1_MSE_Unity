using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 전체에서 공통으로 유지되어야 하는 플레이어 데이터와 설정값을 관리하는 싱글톤 매니저

public class DataManager : MonoBehaviour
{

    /// 로그인 후 서버에서 전달받은 보유 펫 정보를 저장하기 위한 슬롯 구조체.
    [Serializable]
    public struct OwnedPetSlot
    {
        /// 서버 DB에서 특정 펫을 식별하는 고유 ID
        public int petId;

        /// 펫 종류를 구분하는 ID
        /// 1 = 토끼, 2 = 여우, 3 = 사슴, 4 = 멧돼지
        public int petTypeId;
    }

    /// 전역 접근용 싱글톤 인스턴스
    public static DataManager Data { get; private set; }

    // ─────────────────────────────────────────────
    // 1-2. 플레이어 프로필 데이터

    [Header("Current User Session")]

    /// DB의 User.userId 에 해당하는 값.
    /// 로그인되지 않은 상태는 -1로 둔다.
    [SerializeField] private int _userId = -1;

    /// DB의 User.id 에 해당하는 값.
    /// 사용자가 로그인할 때 사용하는 문자열 ID
    [SerializeField] private string _loginId = string.Empty;

    /// DB의 User.nickname 에 해당하는 값.
    /// 게임 화면에서 표시되는 유저 닉네임
    [SerializeField] private string _nickname = "Player";

    ///DB의 User.shopName에 해당하는 값
    /// 펫샵 이름으로, 간판 UI 등에 표시
    [SerializeField] private string _petShopName = "My PetShop";

    /// 외부에서는 읽기 전용으로 접근하도록 프로퍼티 제공
    public int UserId => _userId;
    public string LoginId => _loginId;
    public string Nickname => _nickname;
    public string PetShopName => _petShopName;

    // ─────────────────────────────────────────────
    // 1-2. 보유 펫 데이터

    [Header("Owned Pets")]

    /// 로그인 응답으로 전달받은 보유 펫 정보를 슬롯 형태로 저장한다.
    /// 
    /// 슬롯은 총 4개이며, 인덱스 0~3은 UI에서 1~4번 슬롯으로 사용한다.

    /// petId:
    /// 서버 DB에서 특정 펫을 식별하는 고유 ID.
    /// 나중에 펫룸에서 펫 상세 조회 요청 시 서버로 전달한다.
    ///
    /// petTypeId:
    /// 펫 종류를 구분하는 ID.
    /// 예: 1 = 토끼, 2 = 여우, 3 = 사슴, 4 = 멧돼지
    [SerializeField] private OwnedPetSlot[] _ownedPetSlots = new OwnedPetSlot[4];

    /// 외부에서는 읽기 전용으로 접근하도록 프로퍼티 제공
    public OwnedPetSlot[] OwnedPetSlots => _ownedPetSlots;


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
        _ownedPetSlots = new OwnedPetSlot[4];


        BroadcastProfileState();
        BroadcastMailState();
    }

    // ─────────────────────────────────────────────
    // 7. 보유 펫 관련 메서드

    /// 로그인 응답으로 받은 보유 펫 목록을 4개의 펫 슬롯에 저장한다.
    ///
    /// 사용 이유:
    /// - 로그인 이후 PetTown에서 보유 펫을 스폰하기 위해 사용
    /// - PetRoom에서 슬롯을 눌렀을 때 해당 슬롯의 petId로 상세 조회 요청을 보내기 위해 사용
    ///
    /// 주의:
    /// - 상세 정보는 펫룸 진입 시 NetworkManager.RequestPetData(petId)를 통해 서버에서 다시 조회한다.
    public void SetOwnedPets(OwnedPetData[] ownedPets)
    {
        // 기존 슬롯 정보를 초기화한다.
        _ownedPetSlots = new OwnedPetSlot[4];

        // 서버에서 받은 보유 펫 목록이 없으면 빈 슬롯 상태로 유지한다.
        if (ownedPets == null)
        {
            Debug.Log("[DataManager] 보유 펫 없음");
            return;
        }

        // 최대 4마리까지만 슬롯에 저장한다.
        for (int i = 0; i < ownedPets.Length && i < 4; i++)
        {
            _ownedPetSlots[i].petId = ownedPets[i].petId;
            _ownedPetSlots[i].petTypeId = ownedPets[i].petTypeId;
        }

        Debug.Log("[DataManager] 보유 펫 슬롯 저장 완료");
    }

    /// 특정 슬롯의 petId를 반환한다.
    /// 
    /// slotIndex는 배열 기준으로 0~3 값을 사용한다.
    /// UI에서 1~4번 슬롯을 사용할 경우, 1번 슬롯은 index 0으로 변환해서 사용한다.
    public int GetOwnedPetId(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _ownedPetSlots.Length)
        {
            Debug.LogWarning("[DataManager] 잘못된 펫 슬롯 인덱스: " + slotIndex);
            return -1;
        }

        return _ownedPetSlots[slotIndex].petId;
    }

    /// 특정 슬롯의 petTypeId를 반환한다.
    public int GetOwnedPetTypeId(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _ownedPetSlots.Length)
        {
            Debug.LogWarning("[DataManager] 잘못된 펫 슬롯 인덱스: " + slotIndex);
            return -1;
        }

        return _ownedPetSlots[slotIndex].petTypeId;
    }

    // ─────────────────────────────────────────────
    // 8. 볼륨 관련 메서드

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
    // 9. 쪽지 관련 메서드

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
    // 10. 초기화 관련 메서드

    /// <summary>
    /// 테스트용 기본 상태로 초기화한다.
    /// 로그인 전 상태를 가정한 기본값들로 되돌린다.
    public void InitializeDefaultData()
    {
        _userId = -1;
        _loginId = string.Empty;
        _nickname = "Player";
        _petShopName = "My PetShop";
        _ownedPetSlots = new OwnedPetSlot[4];

        _bgmVolumeLevel = 80;
        _sfxVolumeLevel = 80;

        _hasUnreadMail = false;

        BroadcastProfileState();
        BroadcastAudioSettings();
        BroadcastMailState();
    }
}