using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Singleton manager that stores shared player data and global settings.
/// It persists across scenes and provides data to UI, gameplay, and audio systems.
/// </summary>
/// 
public class DataManager : MonoBehaviour
{

    /// <summary>
    /// Data structure for one owned pet slot.
    /// Stores the pet ID, type, and current level.
    /// </summary>
    [Serializable]
    public struct OwnedPetSlot
    {
        public int petId;
        /// 1 = Hare, 2 = Fox, 3 = Derr, 4 = Boar
        public int petTypeId;
        public int level;
    }

    /// <summary>
    /// Data structure for one owned item slot.
    /// Stores the item ID, type, and quantity.
    /// </summary>
    [Serializable]
    public struct OwnedItemSlot
    {
        public int itemId;
        // 1 = pumkin, 2 = banana, 3 = apple, 4 = carrot, 5 = water
        public int itemTypeId;
        public int count;
    }

    // Global singleton instance of DataManager.
    public static DataManager Data { get; private set; }


    [Header("Current User Session")]

    /// <summary>
    /// Current logged-in user information.
    /// Default userId is -1 when no user is logged in.
    /// </summary>
    [SerializeField] private int _userId = -1;

    [SerializeField] private string _loginId = string.Empty;

    [SerializeField] private string _nickname = "Player";

    [SerializeField] private string _petShopName = "My PetShop";

    public int UserId => _userId;
    public string LoginId => _loginId;
    public string Nickname => _nickname;
    public string PetShopName => _petShopName;

    
    [Header("Owned Pets")]

    /// <summary>
    /// Owned pet slots used by pet inventory and pet spawning systems.
    /// Maximum of 4 pets can be stored.
    /// </summary>

    [SerializeField] private OwnedPetSlot[] _ownedPetSlots = new OwnedPetSlot[4];

    public OwnedPetSlot[] OwnedPetSlots => _ownedPetSlots;

    /// <summary>
    /// Owned item slots used by inventory UI and pet growth systems.
    /// Maximum of 12 item slots can be stored.
    /// </summary>

    [Header("Owned Items")]
    [SerializeField] private OwnedItemSlot[] _ownedItemSlots = new OwnedItemSlot[12];

    public OwnedItemSlot[] OwnedItemSlots => _ownedItemSlots;

    [Header("Audio Settings")]

    /// BGM volume
    [SerializeField][Range(0, 100)] private int _bgmVolumeLevel = 80;

    /// SFX volume
    [SerializeField][Range(0, 100)] private int _sfxVolumeLevel = 80;


    private float _bgmVolume => _bgmVolumeLevel / 100f;

    private float _sfxVolume => _sfxVolumeLevel / 100f;



    [Header("Mail State")]

    /// <summary>
    /// Indicates whether the player has any unread mail.
    /// Used for mail notification UI.
    /// </summary>
    [SerializeField] private bool _hasUnreadMail = false;

    public bool HasUnreadMail => _hasUnreadMail;


    public static event Action<float> OnBgmVolumeChanged;

    public static event Action<float> OnSfxVolumeChanged;
    /// <summary>
    /// Invoked when the unread mail state changes.
    /// Mail alert UI listens to this event.
    /// </summary>
    public static event Action<bool> OnUnreadMailStateChanged;

  

    private void Awake()
    {
   
        if (Data != null && Data != this)
        {
            Destroy(gameObject);
            return;
        }

        Data = this;
        DontDestroyOnLoad(gameObject);
        
        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    /// <summary>
    /// Called whenever a new scene is loaded.
    /// Rebroadcasts saved profile, audio, and mail states to scene objects.
    /// </summary>
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BroadcastAudioSettings();
        BroadcastMailState();
        BroadcastProfileState();
    }

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

    /// <summary>
    /// Updates the current user session data after login or auth/status response.
    /// Also notifies profile UI listeners.
    /// </summary>
    
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

    /// <summary>
    /// Stores owned pet data received from the server.
    /// The data is converted into 4 fixed pet slots for client-side use.
    /// </summary>
    
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

    /// <summary>
    /// Returns the pet ID stored in the specified pet slot.
    /// Used when selecting or placing a pet.
    /// </summary>
    
    public int GetOwnedPetId(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _ownedPetSlots.Length)
        {
            Debug.LogWarning("[DataManager] 잘못된 펫 슬롯 인덱스: " + slotIndex);
            return -1;
        }

        return _ownedPetSlots[slotIndex].petId;
    }

    public int GetOwnedPetTypeId(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _ownedPetSlots.Length)
        {
            Debug.LogWarning("[DataManager] 잘못된 펫 슬롯 인덱스: " + slotIndex);
            return -1;
        }

        return _ownedPetSlots[slotIndex].petTypeId;
    }

    /// <summary>
    /// Adds a newly acquired pet to the first empty pet slot.
    /// Used after pet acquisition succeeds on the server.
    /// </summary>
    
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

    /// <summary>
    /// Stores inventory item data received from the inventory API.
    /// Used by the item inventory UI.
    /// </summary>

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

    /// <summary>
    /// Stores item data received from the pet room API.
    /// Used when entering the pet room and feeding pets.
    /// </summary>

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

    /// <summary>
    /// Adds or updates an owned item after item acquisition.
    /// If the item type already exists, its count is updated.
    /// </summary>

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

    public void BroadcastAudioSettings()
    {
        OnBgmVolumeChanged?.Invoke(_bgmVolume);
        OnSfxVolumeChanged?.Invoke(_sfxVolume);
    }

    public float GetBgmVolume() => _bgmVolume;
    public float GetSfxVolume() => _sfxVolume;
    public int GetBgmVolumeLevel() => _bgmVolumeLevel;
    public int GetSfxVolumeLevel() => _sfxVolumeLevel;

    /// <summary>
    /// Updates unread mail state and notifies mail UI listeners.
    /// </summary>
    
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