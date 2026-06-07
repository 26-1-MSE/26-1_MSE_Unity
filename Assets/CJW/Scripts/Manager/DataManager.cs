using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// 게임 전체에서 공통으로 유지되어야 하는 플레이어 데이터와 설정값을 관리하는 싱글톤 매니저

public class DataManager : MonoBehaviour
{

    // 0. 데이터 구조체
    [Serializable]
    public struct OwnedPetSlot
    {
        ///펫을 식별하는 고유 ID
        public int petId;

        /// 1 = 토끼, 2 = 여우, 3 = 사슴, 4 = 멧돼지
        public int petTypeId;

        public int level;
    }

    [Serializable]
    public struct OwnedItemSlot
    {
        public int itemId;

        // 1 = 호박, 2 = 바나나, 3 = 사과, 4 = 당근, 5 = 물
        public int itemTypeId;

        public int count;
    }

    // -------------------------------------------------------------
    // 1. 싱글톤
    public static DataManager Data { get; private set; }

    // -------------------------------------------------------------
    // 2. 플레이어 프로필 데이터

    [Header("Current User Session")]

    /// 로그인되지 않은 상태 -1
    [SerializeField] private int _userId = -1;

    [SerializeField] private string _loginId = string.Empty;

    /// 유저 닉네임
    [SerializeField] private string _nickname = "Player";

    /// 펫샵 이름
    [SerializeField] private string _petShopName = "My PetShop";

    /// 외부에서는 읽기 전용으로 접근하도록 프로퍼티 제공
    public int UserId => _userId;
    public string LoginId => _loginId;
    public string Nickname => _nickname;
    public string PetShopName => _petShopName;

    // -------------------------------------------------------------
    // 3. 보유 펫 데이터

    [Header("Owned Pets")]

    /// 인덱스 0~3 -> UI에서 1~4번 슬롯으로 사용한다.

    [SerializeField] private OwnedPetSlot[] _ownedPetSlots = new OwnedPetSlot[4];

 

    /// 외부에서는 읽기 전용으로 접근하도록 프로퍼티 제공
    public OwnedPetSlot[] OwnedPetSlots => _ownedPetSlots;

    // -------------------------------------------------------------
    // 4. 보유 아이템 데이터

    [Header("Owned Items")]
    [SerializeField] private OwnedItemSlot[] _ownedItemSlots = new OwnedItemSlot[12];

    public OwnedItemSlot[] OwnedItemSlots => _ownedItemSlots;

    // -------------------------------------------------------------
    // 5. 사운드 설정

    [Header("Audio Settings")]

    /// BGM 볼륨
    [SerializeField][Range(0, 100)] private int _bgmVolumeLevel = 80;

    /// 효과음 볼륨
    [SerializeField][Range(0, 100)] private int _sfxVolumeLevel = 80;

    /// 실제 AudioSource.volume에 넣기 위한 0.0~1.0 값
    private float _bgmVolume => _bgmVolumeLevel / 100f;

    /// 실제 AudioSource.volume에 넣기 위한 0.0~1.0 값
    private float _sfxVolume => _sfxVolumeLevel / 100f;

    // -------------------------------------------------------------
    // 6. 메일 상태

    [Header("Mail State")]

    /// <summary>
    /// 읽지 않은 메일이 하나라도 있는지 여부.
    [SerializeField] private bool _hasUnreadMail = false;

    /// 외부 읽기 전용 프로퍼티.
    public bool HasUnreadMail => _hasUnreadMail;

    // -------------------------------------------------------------
    // 7. 이벤트

    public static event Action<float> OnBgmVolumeChanged;

    public static event Action<float> OnSfxVolumeChanged;

    /// 읽지 않은 메일 여부가 바뀌었을 때 호출되는 이벤트
    public static event Action<bool> OnUnreadMailStateChanged;

    // -------------------------------------------------------------
    // 8. Unity 생명주기

    private void Awake()
    {
   
        if (Data != null && Data != this)
        {
            Destroy(gameObject);
            return;
        }

        // 싱글톤 인스턴스 등록
        Data = this;

        DontDestroyOnLoad(gameObject);

        // 새 씬이 로드될 때마다 현재 설정값을 다시 브로드캐스트하기 위해 구독
        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    /// 새 씬이 로드될 때 호출됨
    /// 현재 저장 중인 볼륨값/쪽지 상태를 다시 알려줌
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BroadcastAudioSettings();
        BroadcastMailState();
        BroadcastProfileState();
    }

    /// 인스펙터에서 값이 바뀔 때 범위를 강제로 맞춰줌
    private void OnValidate()
    {
        _bgmVolumeLevel = Mathf.Clamp(_bgmVolumeLevel, 0, 100);
        _sfxVolumeLevel = Mathf.Clamp(_sfxVolumeLevel, 0, 100);

        if (Application.isPlaying)
        {
            BroadcastAudioSettings();
            BroadcastMailState();
            BroadcastProfileState();
        }
    }

    private void OnDestroy()
    {
        if (Data == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            OnBgmVolumeChanged = null;
            OnSfxVolumeChanged = null;
            OnProfileChanged = null;
            OnUnreadMailStateChanged = null;
        }
    }

    // -------------------------------------------------------------
    // 9. 프로필 메서드

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

        // 프로필 관련 UI 갱신
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

    // -------------------------------------------------------------
    // 10. 보유 펫 메서드

    /// 로그인 응답으로 받은 보유 펫 목록을 4개의 펫 슬롯에 저장한다
    /// - 상세 정보는 펫룸 진입 시 NetworkManager.RequestPetData(petId)를 통해 서버에서 다시 조회한다
    public void SetOwnedPets(OwnedPetData[] ownedPets)
    {
        _ownedPetSlots = new OwnedPetSlot[4];

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
            _ownedPetSlots[i].level = ownedPets[i].level;
        }

        Debug.Log("[DataManager] 보유 펫 슬롯 저장 완료");
    }

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

    /// 특정 슬롯의 petTypeId를 반환한다
    public int GetOwnedPetTypeId(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _ownedPetSlots.Length)
        {
            Debug.LogWarning("[DataManager] 잘못된 펫 슬롯 인덱스: " + slotIndex);
            return -1;
        }

        return _ownedPetSlots[slotIndex].petTypeId;
    }

    // S3에서 펫 추가했을 시를 위함
    public void AddOwnedPet(int petId, int petTypeId)
    {
        for (int i = 0; i < _ownedPetSlots.Length; i++)
        {
            if (_ownedPetSlots[i].petId == 0)
            {
                _ownedPetSlots[i].petId = petId;
                _ownedPetSlots[i].petTypeId = petTypeId;

                Debug.Log($"[DataManager] 보유 펫 추가 저장 완료 / slot: {i}, petId: {petId}, petTypeId: {petTypeId}");
                return;
            }
        }

        Debug.LogWarning("[DataManager] 보유 펫 슬롯이 가득 찼습니다.");
    }

    // -------------------------------------------------------------
    // 11. 보유 아이템 메서드

    public void SetOwnedItems(InventoryItemData[] items)
    {
        _ownedItemSlots = new OwnedItemSlot[12];

        Debug.Log("[DataManager] InventoryItemData SetOwnedItems 호출됨");

        if (items == null || items.Length == 0)
            return;

        for (int i = 0; i < items.Length && i < _ownedItemSlots.Length; i++)
        {
            _ownedItemSlots[i] = new OwnedItemSlot
            {
                itemId = items[i].itemId,
                itemTypeId = items[i].itemTypeId,
                count = items[i].count
            };
            Debug.Log($"[DataManager] itemId:{items[i].itemId}, typeId:{items[i].itemTypeId}, count:{items[i].count}");
        }
    }

    public void SetOwnedItems(PetItemData[] items)
    {
        _ownedItemSlots = new OwnedItemSlot[12];

        if (items == null || items.Length == 0)
            return;

        for (int i = 0; i < items.Length && i < _ownedItemSlots.Length; i++)
        {
            _ownedItemSlots[i] = new OwnedItemSlot
            {
                itemId = items[i].itemId,
                itemTypeId = items[i].itemTypeId,
                count = items[i].count
            };
        }
    }

    public void AddOwnedItem(int itemId, int itemTypeId, int count)
    {
        for (int i = 0; i < _ownedItemSlots.Length; i++)
        {
            if (_ownedItemSlots[i].itemTypeId == itemTypeId)
            {
                _ownedItemSlots[i].count = count;
                Debug.Log($"[DataManager] 아이템 수량 갱신 / itemTypeId:{itemTypeId}, count:{count}");
                return;
            }
        }

        for (int i = 0; i < _ownedItemSlots.Length; i++)
        {
            if (_ownedItemSlots[i].itemId == 0)
            {
                _ownedItemSlots[i].itemId = itemId;
                _ownedItemSlots[i].itemTypeId = itemTypeId;
                _ownedItemSlots[i].count = count;

                Debug.Log($"[DataManager] 새 아이템 추가 / itemTypeId:{itemTypeId}, count:{count}");
                return;
            }
        }

        Debug.LogWarning("[DataManager] 아이템 슬롯이 가득 찼습니다.");
    }

    // -------------------------------------------------------------
    // 12. 볼륨 메서드

    public void SetBgmVolume(int volumeLevel)
    {
        int newLevel = Mathf.Clamp(volumeLevel, 0, 100);

        if (_bgmVolumeLevel == newLevel)
            return;

        _bgmVolumeLevel = newLevel;
        OnBgmVolumeChanged?.Invoke(_bgmVolume);
    }


    public void SetSfxVolume(int volumeLevel)
    {
        int newLevel = Mathf.Clamp(volumeLevel, 0, 100);

        if (_sfxVolumeLevel == newLevel)
            return;

        _sfxVolumeLevel = newLevel;
        OnSfxVolumeChanged?.Invoke(_sfxVolume);
    }

    /// 현재 저장된 볼륨값을 한 번에 다시 브로드캐스트
    public void BroadcastAudioSettings()
    {
        OnBgmVolumeChanged?.Invoke(_bgmVolume);
        OnSfxVolumeChanged?.Invoke(_sfxVolume);
    }

    public float GetBgmVolume() => _bgmVolume;
    public float GetSfxVolume() => _sfxVolume;
    public int GetBgmVolumeLevel() => _bgmVolumeLevel;
    public int GetSfxVolumeLevel() => _sfxVolumeLevel;

    // -------------------------------------------------------------
    // 13. 쪽지 메서드

    /// 새 쪽지 여부 설정
    public void SetUnreadMailState(bool hasUnreadMail)
    {
        if (_hasUnreadMail == hasUnreadMail)
            return;

        _hasUnreadMail = hasUnreadMail;
        OnUnreadMailStateChanged?.Invoke(_hasUnreadMail);
    }

    public void BroadcastMailState()
    {
        OnUnreadMailStateChanged?.Invoke(_hasUnreadMail);
    }

    public void MarkAllMailAsRead()
    {
        SetUnreadMailState(false);
    }

    // -------------------------------------------------------------
    // 14. 초기화 메서드

    public void InitializeDefaultData()
    {
        _userId = -1;
        _loginId = string.Empty;
        _nickname = "Player";
        _petShopName = "My PetShop";
        _ownedPetSlots = new OwnedPetSlot[4];
        _ownedItemSlots = new OwnedItemSlot[12];

        _bgmVolumeLevel = 80;
        _sfxVolumeLevel = 80;

        _hasUnreadMail = false;

        BroadcastProfileState();
        BroadcastAudioSettings();
        BroadcastMailState();
    }
}