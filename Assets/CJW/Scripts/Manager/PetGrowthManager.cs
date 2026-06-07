using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 현재 펫 상태 저장, HUD 갱신, 먹이/물 증가, 레벨업, 크기 변경
public class PetGrowthManager : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text petNameText;
    [SerializeField] private Slider foodSlider;
    [SerializeField] private Slider waterSlider;

    [SerializeField] private TMP_Text foodCountText;
    [SerializeField] private TMP_Text waterCountText;

    [SerializeField] private ToastMessage toastMessage;

    [Header("Placed Pet")]
    [SerializeField] private Transform placedPetTransform;

    private int currentPetId;
    private int currentLevel;
    private string currentPetName;

    private int currentFood;
    private int currentFoodMax;

    private int currentWater;
    private int currentWaterMax;


    public void SetCurrentPet(PetRoomResponse response, Transform petTransform)
    {
        currentPetId = response.data.pet.petId;
        currentLevel = response.data.pet.level;
        currentPetName = response.data.pet.petName;

        currentFood = response.data.pet.food.current;
        currentFoodMax = response.data.pet.food.max;

        currentWater = response.data.pet.water.current;
        currentWaterMax = response.data.pet.water.max;

        placedPetTransform = petTransform;

        ApplyPetScale();
        RefreshHUD();
    }


    public int GetCurrentPetId()
    {
        return currentPetId;
    }
    public void UseItemOnCurrentPet(int itemTypeId, System.Action onSuccess)
    {
        if (currentPetId <= 0)
        {
            Debug.LogWarning("[PetGrowthManager] 현재 선택된 펫 없음");
            return;
        }

        NetworkManager.Instance.RequestUseItem(
            currentPetId,
            itemTypeId,
            response =>
            {
                ApplyUseItemData(response);
                onSuccess?.Invoke();
            },
            error =>
            {
                Debug.LogError("[PetGrowthManager] 아이템 사용 실패: " + error);
                toastMessage?.ShowToast(error);
            }
        );
    }

    public void ApplyUseItemData(UseItemResponse response)
    {
        int previousLevel = currentLevel;

        currentLevel = response.data.pet.level;

        currentFood = response.data.pet.food.current;
        currentFoodMax = response.data.pet.food.max;

        currentWater = response.data.pet.water.current;
        currentWaterMax = response.data.pet.water.max;

        if (currentLevel > previousLevel)
        {
            ApplyPetScale();

            if (currentLevel == 2)
            {
                toastMessage?.ShowToast("Level up!");
                AudioManager.SFXInstance?.PlayOneShot(4);
            }
            else if (currentLevel == 3)
            {
                toastMessage?.ShowToast("Your pet has finished growing.");
                AudioManager.SFXInstance?.PlayOneShot(5);
            }
        }
        else
        {
            AudioManager.SFXInstance?.PlayOneShot(3);
        }
        RefreshHUD();
    }

    private void ApplyPetScale()
    {
        if (placedPetTransform == null)
        {
            Debug.LogWarning("[PetGrowthManager] placedPetTransform 없음");
            return;
        }

        float scale = GetScaleByLevel(currentLevel);
        placedPetTransform.localScale = new Vector3(scale, scale, 1f);

        Debug.Log($"[PetGrowthManager] 펫 크기 변경 / level:{currentLevel}, scale:{scale}");
    }

    private void RefreshHUD()
    {
        if (levelText != null)
            levelText.text = "Lv. " + currentLevel;

        if (petNameText != null)
            petNameText.text = currentPetName;

        if (foodSlider != null)
        {
            foodSlider.maxValue = currentFoodMax;
            foodSlider.value = currentFood;
        }

        if (waterSlider != null)
        {
            waterSlider.maxValue = currentWaterMax;
            waterSlider.value = currentWater;
        }

        if (foodCountText != null)
        {
            foodCountText.text = $"{currentFood} / {currentFoodMax}";
        }

        if (waterCountText != null)
        {
            waterCountText.text = $"{currentWater} / {currentWaterMax}";
        }
    
    }

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