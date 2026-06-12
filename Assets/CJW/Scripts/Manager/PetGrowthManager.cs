using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages pet growth progression in the Pet Room.
/// Handles pet status updates, item usage, HUD refresh,
/// level-up processing, and pet scaling.
/// </summary>

public class PetGrowthManager : MonoBehaviour
{
    // HUD 연결
    [Header("HUD")]
    // Displays the current status of the selected pet.
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text petNameText;

    // Displays the current food/water progress of the pet.
    [SerializeField] private Slider foodSlider;
    [SerializeField] private Slider waterSlider;

    [SerializeField] private TMP_Text foodCountText;
    [SerializeField] private TMP_Text waterCountText;

    [SerializeField] private ToastMessage toastMessage;

    [Header("Placed Pet")]
    [SerializeField] private Transform placedPetTransform;

    //data of the selected pet
    private int currentPetId;
    private int currentLevel;
    private string currentPetName;

    private int currentFood;
    private int currentFoodMax;

    private int currentWater;
    private int currentWaterMax;

    // Prevents duplicate item usage requests while waiting for a server response
    private bool isUsingItem = false;

    private float useItemRequestStartTime;
    /// <summary>
    /// Initializes the currently selected pet using
    /// data received from the Pet Room API.
    /// Also updates pet scale and HUD information.
    /// </summary>
    
    public void SetCurrentPet(PetRoomResponse response, Transform petTransform)
    {
        Debug.Log($"[PetGrowthManager] SetCurrentPet 시작 / time:{Time.time:F3}");

        currentPetId = response.data.pet.petId;
        currentLevel = response.data.pet.level;
        currentPetName = response.data.pet.petName;

        currentFood = response.data.pet.food.current;
        currentFoodMax = response.data.pet.food.max;

        currentWater = response.data.pet.water.current;
        currentWaterMax = response.data.pet.water.max;

        placedPetTransform = petTransform;

        Debug.Log($"[PetGrowthManager] 초기 펫 데이터 / petId:{currentPetId}, level:{currentLevel}, food:{currentFood}/{currentFoodMax}, water:{currentWater}/{currentWaterMax}");

        ApplyPetScale();
        RefreshHUD();

        Debug.Log($"[PetGrowthManager] SetCurrentPet 완료 / time:{Time.time:F3}");
    }

    // Returns the ID of the currently selected pet.
    public int GetCurrentPetId()
    {
        return currentPetId;
    }

    /// <summary>
    /// Sends an item usage request for the selected pet.
    /// Prevents duplicate requests and updates pet status
    /// when the server response is received.
    /// </summary>
    
    public void UseItemOnCurrentPet(int itemTypeId, System.Action onSuccess)
    {
        Debug.Log($"[PetGrowthManager] UseItemOnCurrentPet 호출 / time:{Time.time:F3}, petId:{currentPetId}, itemTypeId:{itemTypeId}");

        
        if (currentPetId <= 0)
        {
            Debug.LogWarning("[PetGrowthManager] 현재 선택된 펫 없음");
            return;
        }

        if (isUsingItem)
        {
            Debug.LogWarning($"[PetGrowthManager] 아이템 사용 요청 처리 중이라 차단 / time:{Time.time:F3}");
            return;
        }

        isUsingItem = true;
        useItemRequestStartTime = Time.time;

        Debug.Log($"[PetGrowthManager] 서버 RequestUseItem 시작 / time:{useItemRequestStartTime:F3}");

        NetworkManager.Instance.RequestUseItem(
            currentPetId,
            itemTypeId,
            response =>
            {
                //서버 응답 기반으로 펫 상태 갱신 -> 성공 콜백 -> 아이템 사용가능 상태 전환

                float responseTime = Time.time;
                Debug.Log($"[PetGrowthManager] 서버 RequestUseItem 응답 도착 / time:{responseTime:F3}, elapsed:{responseTime - useItemRequestStartTime:F3}s");

                ApplyUseItemData(response);

                Debug.Log($"[PetGrowthManager] onSuccess 콜백 실행 전 / time:{Time.time:F3}");
                onSuccess?.Invoke();
                Debug.Log($"[PetGrowthManager] onSuccess 콜백 실행 후 / time:{Time.time:F3}");

                isUsingItem = false;

                Debug.Log($"[PetGrowthManager] 아이템 사용 처리 완료 / totalElapsed:{Time.time - useItemRequestStartTime:F3}s");

            },
            error =>
            {
                Debug.LogError($"[PetGrowthManager] 아이템 사용 실패 / time:{Time.time:F3}, elapsed:{Time.time - useItemRequestStartTime:F3}s, error:{error}");
                isUsingItem = false;
            }
        );
    }

    // Returns whether an item usage request is currently being processed
    public bool IsUsingItem()
    {
        return isUsingItem;
    }

    /// <summary>
    /// Applies the latest pet status returned from the server after an item is used.
    /// Handles level-up effects, scaling, sound effects, and HUD updates.
    /// </summary>
    public void ApplyUseItemData(UseItemResponse response)
    {
        Debug.Log($"[PetGrowthManager] ApplyUseItemData 시작 / time:{Time.time:F3}");

        int previousLevel = currentLevel;

        currentLevel = response.data.pet.level;

        currentFood = response.data.pet.food.current;
        currentFoodMax = response.data.pet.food.max;

        currentWater = response.data.pet.water.current;
        currentWaterMax = response.data.pet.water.max;

        Debug.Log($"[PetGrowthManager] 서버 응답 데이터 반영 / prevLevel:{previousLevel}, newLevel:{currentLevel}, food:{currentFood}/{currentFoodMax}, water:{currentWater}/{currentWaterMax}");

        if (currentLevel > previousLevel)
        {
            Debug.Log($"[PetGrowthManager] 레벨업 감지 / {previousLevel} -> {currentLevel}, time:{Time.time:F3}");

            ApplyPetScale();

            if (currentLevel == 2)
            {
                Debug.Log($"[PetGrowthManager] Lv2 레벨업 연출 시작 / time:{Time.time:F3}");
                toastMessage?.ShowToast("Level up!");
                AudioManager.SFXInstance?.PlayOneShot(4);
            }
            else if (currentLevel == 3)
            {
                Debug.Log($"[PetGrowthManager] Lv3 성장 완료 연출 시작 / time:{Time.time:F3}");
                toastMessage?.ShowToast("Your pet has finished growing.");
                AudioManager.SFXInstance?.PlayOneShot(5);
            }
        }
        else
        {
            Debug.Log($"[PetGrowthManager] 일반 아이템 사용 처리 / level:{currentLevel}, time:{Time.time:F3}");
            AudioManager.SFXInstance?.PlayOneShot(3);
        }

        RefreshHUD();
        Debug.Log($"[PetGrowthManager] ApplyUseItemData 완료 / time:{Time.time:F3}");
    }

    /// Adjusts the pet's visual scale according to its level.
    private void ApplyPetScale()
    {
        Debug.Log($"[PetGrowthManager] ApplyPetScale 호출 / time:{Time.time:F3}");

        if (placedPetTransform == null)
        {
            Debug.LogWarning("[PetGrowthManager] placedPetTransform 없음");
            return;
        }

       
        float scale = GetScaleByLevel(currentLevel);
        placedPetTransform.localScale = new Vector3(scale, scale, 1f);

        Debug.Log($"[PetGrowthManager] 펫 크기 변경 완료 / level:{currentLevel}, scale:{scale}, actualScale:{placedPetTransform.localScale}, time:{Time.time:F3}");
    }

    /// Refreshes all pet-related HUD elements
    private void RefreshHUD()
    {
        Debug.Log($"[PetGrowthManager] RefreshHUD 호출 / time:{Time.time:F3}");

        if (levelText != null)
            levelText.text = "Lv. " + currentLevel;

        if (petNameText != null)
            petNameText.text = currentPetName;

        bool isMaxLevel = currentLevel >= 3;

        if (foodSlider != null)
        {
            foodSlider.maxValue = currentFoodMax;
            foodSlider.value = isMaxLevel ? currentFoodMax : currentFood;
        }

        if (waterSlider != null)
        {
            waterSlider.maxValue = currentWaterMax;
            waterSlider.value = isMaxLevel ? currentWaterMax : currentWater;
        }

        // Leve3 -> MAX / MAX
        if (foodCountText != null)
        {
            foodCountText.text = isMaxLevel
                ? "MAX / MAX"
                : $"{currentFood} / {currentFoodMax}";
        }

        if (waterCountText != null)
        {
            waterCountText.text = isMaxLevel
                ? "MAX / MAX"
                : $"{currentWater} / {currentWaterMax}";
        }

        Debug.Log($"[PetGrowthManager] RefreshHUD 완료 / levelText:{levelText?.text}, foodText:{foodCountText?.text}, waterText:{waterCountText?.text}, time:{Time.time:F3}");


}

    // Returns the visual scale value associated with a pet level.
    private float GetScaleByLevel(int level)
    {
        switch (level)
        {
            case 1: return 8f;
            case 2: return 10f;
            case 3: return 13f;
            default: return 8f;
        }
    }
}