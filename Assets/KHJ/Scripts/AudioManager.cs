using UnityEngine;

/// <summary>
/// Audio data container that pairs an AudioClip with a per-clip volume scale.
/// </summary>
[System.Serializable]
public class AudioClipData
{
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volumeScale = 1f; // volume relative to the master volume
}

/// <summary>
/// Singleton audio manager that separates BGM and SFX playback, survives scene transitions
/// via DontDestroyOnLoad, and syncs volume with DataManager events.
/// Supports per-clip volume scaling on top of the master volume.
/// </summary>
public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// Persistent BGM instance that survives scene transitions.
    /// </summary>
    public static AudioManager BGMInstance { get; private set; }
    public static AudioManager SFXInstance { get; private set; }

    [Header("Destroy Settings")]
    [SerializeField] private bool _dontDestroyOnLoad = false;

    [Header("Audio Components")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClipData[] _audioClipsData;

    [Header("Auto Play Settings")]
    [SerializeField] private bool _autoPlayOnStart = false;
    [SerializeField] private int _backgroundMusicClipId = 0;
    [SerializeField] private bool _loopBackgroundMusic = true;

    [Header("Audio Type")]
    [SerializeField] private bool _isBgmSource = false; // true: BGM, false: SFX

    /// <summary>
    /// Initializes the singleton instance and applies DontDestroyOnLoad if configured.
    /// </summary>
    void Awake()
    {
        if (_dontDestroyOnLoad)
        {
            if (BGMInstance)
            {
                Debug.Log("DontDestroyOnLoad AudioManager already exists. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            BGMInstance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("AudioManager set to DontDestroyOnLoad and assigned as BGMInstance");
        }
        else if (_isBgmSource == false)
        {
            SFXInstance = this;
        }
        else
        {
            Debug.Log("AudioManager created without DontDestroyOnLoad");
        }
    }

    /// <summary>
    /// Initializes the AudioSource, subscribes to volume change events, and starts auto-play if configured.
    /// </summary>
    void Start()
    {
        if (!_audioSource) _audioSource = GetComponent<AudioSource>();

        if (_isBgmSource)
            DataManager.OnBgmVolumeChanged += OnVolumeChanged;
        else
            DataManager.OnSfxVolumeChanged += OnVolumeChanged;

        ApplyCurrentVolume();

        if (_autoPlayOnStart && BGMInstance == this && _isBgmSource)
            PlayBackgroundMusic();
    }

    /// <summary>
    /// Unsubscribes from volume events and clears the static BGMInstance reference.
    /// </summary>
    void OnDestroy()
    {
        if (_isBgmSource)
            DataManager.OnBgmVolumeChanged -= OnVolumeChanged;
        else
            DataManager.OnSfxVolumeChanged -= OnVolumeChanged;

        if (BGMInstance == this)
            BGMInstance = null;
        if (SFXInstance == this)
            SFXInstance = null;
    }

    /// <summary>
    /// Applies inspector changes at runtime for immediate feedback in the editor.
    /// </summary>
    private void OnValidate()
    {
        if (_audioClipsData != null && _audioClipsData.Length > 0)
            _backgroundMusicClipId = Mathf.Clamp(_backgroundMusicClipId, 0, _audioClipsData.Length - 1);

        if (Application.isPlaying && IsAudioSourceReady())
        {
            ApplyCurrentVolume(_backgroundMusicClipId);

            if (_autoPlayOnStart && _isBgmSource && !_audioSource.isPlaying)
                PlayBackgroundMusic();
            else if (!_autoPlayOnStart && _audioSource.isPlaying)
                StopAudio();
        }
    }

    /// <summary>
    /// Handles volume change events dispatched by DataManager.
    /// </summary>
    private void OnVolumeChanged(float newVolume)
    {
        ApplyCurrentVolume();
    }

    /// <summary>
    /// Plays the configured background music clip with looping enabled.
    /// </summary>
    public void PlayBackgroundMusic()
    {
        if (!IsAudioSourceReady())
        {
            Debug.LogWarning("AudioSource is not ready for playback");
            return;
        }

        if (!IsValidClipId(_backgroundMusicClipId))
        {
            Debug.LogWarning($"Invalid background music clip ID: {_backgroundMusicClipId}");
            return;
        }

        _audioSource.clip = _audioClipsData[_backgroundMusicClipId].clip;
        _audioSource.loop = _loopBackgroundMusic;
        ApplyCurrentVolume(_backgroundMusicClipId);
        _audioSource.Play();

        Debug.Log($"Background music started: {_audioClipsData[_backgroundMusicClipId].clip.name}");
    }

    /// <summary>
    /// Plays the specified clip once without looping.
    /// </summary>
    /// <param name="clipId">Index of the clip to play.</param>
    public void PlayAudio(int clipId)
    {
        if (!IsAudioSourceReady())
        {
            Debug.LogWarning("AudioSource is not ready for playback");
            return;
        }

        if (!IsValidClipId(clipId))
        {
            Debug.LogWarning($"Invalid clip ID: {clipId}");
            return;
        }

        _audioSource.clip = _audioClipsData[clipId].clip;
        _audioSource.loop = false;
        ApplyCurrentVolume(clipId);
        _audioSource.Play();
    }

    /// <summary>
    /// Plays a one-shot sound without interrupting the current clip, allowing overlapping playback.
    /// </summary>
    /// <param name="clipId">Index of the clip to play.</param>
    public void PlayOneShot(int clipId)
    {
        if (!IsAudioSourceReady())
        {
            Debug.LogWarning("AudioSource is not ready for one-shot playback");
            return;
        }

        if (!IsValidClipId(clipId))
        {
            Debug.LogWarning($"Invalid clip ID: {clipId}");
            return;
        }

        // master volume multiplied by the per-clip volume scale
        float volumeScale = GetCurrentVolume() * _audioClipsData[clipId].volumeScale;
        _audioSource.PlayOneShot(_audioClipsData[clipId].clip, volumeScale);
    }

    /// <summary>
    /// Stops the currently playing audio.
    /// </summary>
    public void StopAudio()
    {
        if (IsAudioSourceReady())
        {
            _audioSource.Stop();
            Debug.Log("Audio stopped");
        }
    }

    /// <summary>
    /// Returns true if the AudioSource is ready for playback.
    /// </summary>
    private bool IsAudioSourceReady()
    {
        return _audioSource != null &&
               _audioSource.enabled &&
               _audioSource.gameObject.activeInHierarchy;
    }

    /// <summary>
    /// Returns true if the given clip ID is valid and has an assigned AudioClip.
    /// </summary>
    private bool IsValidClipId(int clipId)
    {
        return _audioClipsData != null &&
               clipId >= 0 &&
               clipId < _audioClipsData.Length &&
               _audioClipsData[clipId] != null &&
               _audioClipsData[clipId].clip != null;
    }

    /// <summary>
    /// Returns the master volume for this source type from DataManager.
    /// </summary>
    private float GetCurrentVolume()
    {
        if (DataManager.Data)
            return _isBgmSource ? DataManager.Data.GetBgmVolume() : DataManager.Data.GetSfxVolume();

        return 1f;
    }

    /// <summary>
    /// Applies the combined master and per-clip volume to the AudioSource.
    /// </summary>
    /// <param name="clipId">Clip index used to look up the per-clip volume scale; pass -1 to use a scale of 1.0.</param>
    private void ApplyCurrentVolume(int clipId = -1)
    {
        if (!IsAudioSourceReady()) return;

        float master = GetCurrentVolume();
        float scale = (clipId >= 0 && IsValidClipId(clipId))
            ? _audioClipsData[clipId].volumeScale
            : 1f;

        _audioSource.volume = master * scale;
    }
}
