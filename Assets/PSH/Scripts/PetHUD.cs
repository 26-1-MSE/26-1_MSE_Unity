using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the current pet's name, level, and food/water status on the HUD.
/// </summary>
public class PetHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text petNameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Slider foodSlider;
    [SerializeField] private Slider waterSlider;
    [SerializeField] private TMP_Text foodCountText;
    [SerializeField] private TMP_Text waterCountText;

    // Updates all HUD elements with the given pet's status
    public void SetHUD(string petName, int level, int foodCurrent, int foodMax, int waterCurrent, int waterMax)
    {
        petNameText.text = petName;
        levelText.text = "Lv. " + level;
        foodSlider.maxValue = foodMax;
        foodSlider.value = foodCurrent;
        waterSlider.maxValue = waterMax;
        waterSlider.value = waterCurrent;
        foodCountText.text = foodCurrent + " / " + foodMax;
        waterCountText.text = waterCurrent + " / " + waterMax;
    }
}