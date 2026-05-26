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

    [Header("Placed Pet")]
    [SerializeField] private Transform placedPetTransform;

    private PetRoomResponse currentPetData;

    private int currentPetId;
    private int currentPetTypeId;
    private int currentLevel;
    private string currentPetName;

    private int currentFood;
    private int currentFoodMax;

    private int currentWater;
    private int currentWaterMax;

    private bool isPetStatusDirty;

    public void SetCurrentPet(PetRoomResponse response, Transform petTransform)
    {
        currentPetData = response;

        currentPetId = response.data.pet.petId;
        currentPetTypeId = response.data.pet.petTypeId;
        currentLevel = response.data.pet.level;
        currentPetName = response.data.pet.petName;

        currentFood = response.data.pet.food.current;
        currentFoodMax = response.data.pet.food.max;

        currentWater = response.data.pet.water.current;
        currentWaterMax = response.data.pet.water.max;

        placedPetTransform = petTransform;
        isPetStatusDirty = false;

        RefreshHUD();
    
    }

    public void FeedCurrentPet(int itemTypeId)
    {
        if (currentPetId <= 0)
            return;

        // itemTypeId 5 = 물
        if (itemTypeId == 5)
        {
            currentWater = Mathf.Min(currentWater + 1, currentWaterMax);
        }
        else
        {
            currentFood = Mathf.Min(currentFood + 1, currentFoodMax);
        }

        CheckLevelUp();

        isPetStatusDirty = true;
        RefreshHUD();
    }

    private void CheckLevelUp()
    {
        if (currentLevel >= 3)
            return;

        if (currentFood >= currentFoodMax && currentWater >= currentWaterMax)
        {
            currentLevel++;

            currentFood = 0;
            currentWater = 0;

            currentFoodMax = GetFoodMaxByLevel(currentLevel);
            currentWaterMax = GetWaterMaxByLevel(currentLevel);
        }
    }

    private int GetFoodMaxByLevel(int level)
    {
        switch (level)
        {
            case 1: return 5;
            case 2: return 10;
            case 3: return 15;
            default: return 5;
        }
    }

    private int GetWaterMaxByLevel(int level)
    {
        switch (level)
        {
            case 1: return 3;
            case 2: return 6;
            case 3: return 10;
            default: return 3;
        }
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
    }

    private float GetScaleByLevel(int level)
    {
        switch (level)
        {
            case 1: return 8f;
            case 2: return 10f;
            case 3: return 12f;
            default: return 8f;
        }
    }

    public bool IsPetStatusDirty()
    {
        return isPetStatusDirty;
    }
}