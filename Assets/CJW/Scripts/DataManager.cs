using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 플레이어 데이터 및 오디오 볼륨 설정을 관리하는 싱글톤 매니저
/// </summary>
public class DataManager : MonoBehaviour
{
    public static DataManager Data { get; private set; }

    [Header("Player Information")]
    private const int MAX_HEALTH = 8;
    public int MaxHealth => MAX_HEALTH;
    [SerializeField] private int _currentHealth = MAX_HEALTH;
    public int CurrentHealth => _currentHealth;

    [Header("Game Settings")]
    [SerializeField] [Range(0, 100)] private int _bgmVolumeLevel = 80;
    [SerializeField] [Range(0, 100)] private int _sfxVolumeLevel = 80;
    private float _bgmVolume => _bgmVolumeLevel / 100f;
    private float _sfxVolume => _sfxVolumeLevel / 100f;

    public static event Action<float> OnBgmVolumeChanged;
    public static event Action<float> OnSfxVolumeChanged;

    private void Awake()
    {
        if (!Data)
        {
            Data = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 전환 시 현재 볼륨을 새 씬의 AudioManager들에게 브로드캐스트
        OnBgmVolumeChanged?.Invoke(_bgmVolume);
        OnSfxVolumeChanged?.Invoke(_sfxVolume);
    }

    private void OnValidate()
    {
        int previousBgm = _bgmVolumeLevel;
        int previousSfx = _sfxVolumeLevel;

        _currentHealth = Mathf.Clamp(_currentHealth, 0, MAX_HEALTH);
        _bgmVolumeLevel = Mathf.Clamp(_bgmVolumeLevel, 0, 100);
        _sfxVolumeLevel = Mathf.Clamp(_sfxVolumeLevel, 0, 100);

        if (Application.isPlaying)
        {
            if (previousBgm != _bgmVolumeLevel)
                OnBgmVolumeChanged?.Invoke(_bgmVolume);

            if (previousSfx != _sfxVolumeLevel)
                OnSfxVolumeChanged?.Invoke(_sfxVolume);
        }
    }

    private void OnDestroy()
    {
        if (Data == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            OnBgmVolumeChanged = null;
            OnSfxVolumeChanged = null;
        }
    }

    // ── 체력 ──────────────────────────────────────────

    public void UseHealth()
    {
        if (_currentHealth > 0)
        {
            --_currentHealth;
            // TODO: 체력 UI 업데이트
        }
        else
        {
            // TODO: 게임 오버 처리
        }
    }

    public void InitializeHealth()
    {
        _currentHealth = MAX_HEALTH;
        // TODO: 체력 UI 업데이트
    }

    // ── 볼륨 ──────────────────────────────────────────

    public void SetBgmVolume(int volumeLevel)
    {
        int newLevel = Mathf.Clamp(volumeLevel, 0, 100);
        if (_bgmVolumeLevel == newLevel) return;

        _bgmVolumeLevel = newLevel;
        OnBgmVolumeChanged?.Invoke(_bgmVolume);
    }

    public void SetSfxVolume(int volumeLevel)
    {
        int newLevel = Mathf.Clamp(volumeLevel, 0, 100);
        if (_sfxVolumeLevel == newLevel) return;

        _sfxVolumeLevel = newLevel;
        OnSfxVolumeChanged?.Invoke(_sfxVolume);
    }

    public float GetBgmVolume() => _bgmVolume;
    public float GetSfxVolume() => _sfxVolume;
    public int GetBgmVolumeLevel() => _bgmVolumeLevel;
    public int GetSfxVolumeLevel() => _sfxVolumeLevel;
}