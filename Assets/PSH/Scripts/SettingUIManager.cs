using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the settings panel UI, syncing BGM/SFX sliders with DataManager.
/// 설정 패널 UI 관리, BGM/SFX 슬라이더와 DataManager 동기화
/// </summary>

public class SettingUIManager : MonoBehaviour
{
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;


    private PublicUIManager publicUI;

    private void Start()
    {
        publicUI = GetComponent<PublicUIManager>();

        bgmSlider.minValue = 0;
        bgmSlider.maxValue = 100;
        sfxSlider.minValue = 0;
        sfxSlider.maxValue = 100;

        bgmSlider.onValueChanged.AddListener(OnBgmChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxChanged);
    }

    // Loads saved volume values into sliders and opens the panel
    public void OpenSetting()
    {
        bgmSlider.value = DataManager.Data.GetBgmVolumeLevel();
        sfxSlider.value = DataManager.Data.GetSfxVolumeLevel();
        publicUI.OpenPanel(settingPanel);
    }

    public void CloseSetting()
    {
        publicUI.ClosePanel();
    }

    private void OnBgmChanged(float value)
    {
       // Debug.Log("BGM 슬라이더 값: " + value);
        DataManager.Data.SetBgmVolume((int)value);
    }
    private void OnSfxChanged(float value)
    {
      //  Debug.Log("SFX 슬라이더 값: " + value);
        DataManager.Data.SetSfxVolume((int)value);
     //   Debug.Log("SFX 실제 볼륨: " + DataManager.Data.GetSfxVolume());
    }

}