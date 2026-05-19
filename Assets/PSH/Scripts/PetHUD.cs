using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PetHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text petNameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Slider foodSlider;
    [SerializeField] private Slider waterSlider;

    public void SetHUD(string petName, int level, int foodCurrent, int foodMax, int waterCurrent, int waterMax)
    {
        petNameText.text = petName;
        levelText.text = "Lv. " + level;
        foodSlider.maxValue = foodMax;
        foodSlider.value = foodCurrent;
        waterSlider.maxValue = waterMax;
        waterSlider.value = waterCurrent;
    }
}