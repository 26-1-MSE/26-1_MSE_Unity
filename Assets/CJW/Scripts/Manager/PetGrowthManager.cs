using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 펫룸에서 현재 선택된 펫의 성장 상태를 관리하는 클래스
// 주요 기능:
// 1. 서버에서 받은 펫 상세 데이터를 저장
// 2. HUD UI 갱신
// 3. 아이템 사용 요청
// 4. 아이템 사용 결과에 따른 먹이/물 수치 갱신
// 5. 레벨업 시 펫 크기 변경 및 토스트 메시지 출력

public class PetGrowthManager : MonoBehaviour
{
    // HUD 연결
    [Header("HUD")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text petNameText;
    [SerializeField] private Slider foodSlider;
    [SerializeField] private Slider waterSlider;

    [SerializeField] private TMP_Text foodCountText;
    [SerializeField] private TMP_Text waterCountText;

    [SerializeField] private ToastMessage toastMessage;

    // 펫룸에 배치된 실제 펫 오브젝트의 Transform
    [Header("Placed Pet")]
    [SerializeField] private Transform placedPetTransform;

    //선택된 펫의 데이터
    private int currentPetId;
    private int currentLevel;
    private string currentPetName;

    private int currentFood;
    private int currentFoodMax;

    private int currentWater;
    private int currentWaterMax;

    // 아이템 사용 요청 중복 방지 플래그
    private bool isUsingItem = false;

    private float useItemRequestStartTime;

    // 펫룸에 펫을 배치한 직후 호출되는 초기화 메서드
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

    // 현재 선택된 펫의 ID를 반환한다.
    public int GetCurrentPetId()
    {
        return currentPetId;
    }

    // 현재 선택된 펫에게 아이템을 사용하는 메서드
    public void UseItemOnCurrentPet(int itemTypeId, System.Action onSuccess)
    {
        Debug.Log($"[PetGrowthManager] UseItemOnCurrentPet 호출 / time:{Time.time:F3}, petId:{currentPetId}, itemTypeId:{itemTypeId}");

        // 선택된 펫이 없을 경우
        if (currentPetId <= 0)
        {
            Debug.LogWarning("[PetGrowthManager] 현재 선택된 펫 없음");
            return;
        }

        // 이미 아이템 사용 요청 중이면 중복 요청 방지
        if (isUsingItem)
        {
            Debug.LogWarning($"[PetGrowthManager] 아이템 사용 요청 처리 중이라 차단 / time:{Time.time:F3}");
            return;
        }

        isUsingItem = true;
        useItemRequestStartTime = Time.time;

        Debug.Log($"[PetGrowthManager] 서버 RequestUseItem 시작 / time:{useItemRequestStartTime:F3}");

        // 서버에 아이템 사용 요청
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

    // 현재 아이템 사용 요청이 진행 중인지 반환한다.
    public bool IsUsingItem()
    {
        return isUsingItem;
    }

    // 아이템 사용 서버 응답을 현재 펫 상태에 반영하는 메서드
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

        // 이전 레벨보다 현재 레벨이 높으면 레벨업 처리
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

        // 변경된 수치를 HUD에 반영
        RefreshHUD();
        Debug.Log($"[PetGrowthManager] ApplyUseItemData 완료 / time:{Time.time:F3}");
    }

    // 현재 레벨에 맞춰 펫 오브젝트 크기를 변경하는 메서드
    private void ApplyPetScale()
    {
        Debug.Log($"[PetGrowthManager] ApplyPetScale 호출 / time:{Time.time:F3}");

        if (placedPetTransform == null)
        {
            Debug.LogWarning("[PetGrowthManager] placedPetTransform 없음");
            return;
        }

        // 레벨별 스케일 값 가져와서 펫 오브젝트 크기 변경
        float scale = GetScaleByLevel(currentLevel);
        placedPetTransform.localScale = new Vector3(scale, scale, 1f);

        Debug.Log($"[PetGrowthManager] 펫 크기 변경 완료 / level:{currentLevel}, scale:{scale}, actualScale:{placedPetTransform.localScale}, time:{Time.time:F3}");
    }

    // HUD에 표시되는 레벨, 이름, 먹이/물 슬라이더와 텍스트를 갱신한다.
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

        // 최대 레벨이면 MAX / MAX로 표시
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

// 펫 레벨에 따른 크기 값을 반환
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