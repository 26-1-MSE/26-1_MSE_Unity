using UnityEngine;
using UnityEngine.UI;

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
        Debug.Log("BGM 슬라이더 값: " + value);
        DataManager.Data.SetBgmVolume((int)value);
    }
    private void OnSfxChanged(float value)
    {
        Debug.Log("SFX 슬라이더 값: " + value);
        DataManager.Data.SetSfxVolume((int)value);
        Debug.Log("SFX 실제 볼륨: " + DataManager.Data.GetSfxVolume());
    }

}