using UnityEngine;

/// <summary>
/// 클립별 개별 볼륨 비율을 포함한 오디오 데이터 클래스
/// </summary>
[System.Serializable]
public class AudioClipData
{
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volumeScale = 1f; // 마스터 볼륨 대비 상대적 비율
}

/// <summary>
/// BGM/SFX 분리, 싱글톤, DontDestroyOnLoad, DataManager 볼륨 연동,
/// 클립별 개별 볼륨 비율을 지원하는 오디오 매니저
/// </summary>
public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// 씬 전환 후에도 유지되는 BGM 인스턴스
    /// </summary>
    public static AudioManager BGMInstance { get; private set; }

    [Header("Destroy Settings")]
    [SerializeField] private bool _dontDestroyOnLoad = false;

    [Header("Audio Components")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClipData[] _audioClipsData; // 🎵 AudioClip[] → AudioClipData[]

    [Header("Auto Play Settings")]
    [SerializeField] private bool _autoPlayOnStart = false;
    [SerializeField] private int _backgroundMusicClipId = 0;
    [SerializeField] private bool _loopBackgroundMusic = true;

    [Header("Audio Type")]
    [SerializeField] private bool _isBgmSource = false; // true: BGM, false: SFX

    /// <summary>
    /// 인스턴스 초기화 및 DontDestroyOnLoad 설정
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
        else
        {
            Debug.Log("AudioManager created without DontDestroyOnLoad");
        }
    }

    /// <summary>
    /// 오디오 컴포넌트 초기화, 볼륨 이벤트 구독, 자동 재생
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
    /// 이벤트 구독 해제 및 BGMInstance 초기화
    /// </summary>
    void OnDestroy()
    {
        if (_isBgmSource)
            DataManager.OnBgmVolumeChanged -= OnVolumeChanged;
        else
            DataManager.OnSfxVolumeChanged -= OnVolumeChanged;

        if (BGMInstance == this)
            BGMInstance = null;
    }

    /// <summary>
    /// 에디터에서 값 변경 시 실시간 반영
    /// </summary>
    private void OnValidate()
    {
        // 🎵 AudioClipData 기준으로 범위 제한
        if (_audioClipsData != null && _audioClipsData.Length > 0)
            _backgroundMusicClipId = Mathf.Clamp(_backgroundMusicClipId, 0, _audioClipsData.Length - 1);

        if (Application.isPlaying && IsAudioSourceReady())
        {
            ApplyCurrentVolume(_backgroundMusicClipId); // 🎵 clipId 전달

            if (_autoPlayOnStart && _isBgmSource && !_audioSource.isPlaying)
                PlayBackgroundMusic();
            else if (!_autoPlayOnStart && _audioSource.isPlaying)
                StopAudio();
        }
    }

    /// <summary>
    /// DataManager 볼륨 변경 이벤트 핸들러
    /// </summary>
    private void OnVolumeChanged(float newVolume)
    {
        ApplyCurrentVolume();
    }

    /// <summary>
    /// 지정된 BGM 클립을 루프 설정으로 재생
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

        _audioSource.clip = _audioClipsData[_backgroundMusicClipId].clip; // 🎵
        _audioSource.loop = _loopBackgroundMusic;
        ApplyCurrentVolume(_backgroundMusicClipId); // 🎵
        _audioSource.Play();

        Debug.Log($"Background music started: {_audioClipsData[_backgroundMusicClipId].clip.name}");
    }

    /// <summary>
    /// 특정 클립을 1회 재생 (루프 없음)
    /// </summary>
    /// <param name="clipId">재생할 클립 인덱스</param>
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

        _audioSource.clip = _audioClipsData[clipId].clip; // 🎵
        _audioSource.loop = false;
        ApplyCurrentVolume(clipId); // 🎵
        _audioSource.Play();
    }

    /// <summary>
    /// AudioSource의 clip을 바꾸지 않고 추가로 소리 재생.
    /// 이전 소리를 중단하지 않고 동시에 여러 소리 재생 가능
    /// </summary>
    /// <param name="clipId">재생할 클립 인덱스</param>
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

        // 🎵 마스터 볼륨 × 클립 개별 볼륨 비율
        float volumeScale = GetCurrentVolume() * _audioClipsData[clipId].volumeScale;
        _audioSource.PlayOneShot(_audioClipsData[clipId].clip, volumeScale);
    }

    /// <summary>
    /// 현재 재생 중인 오디오 정지
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
    /// AudioSource 재생 준비 여부 확인
    /// </summary>
    private bool IsAudioSourceReady()
    {
        return _audioSource != null &&
               _audioSource.enabled &&
               _audioSource.gameObject.activeInHierarchy;
    }

    /// <summary>
    /// 클립 ID 유효성 검사
    /// </summary>
    private bool IsValidClipId(int clipId)
    {
        // AudioClipData 기준으로 유효성 검사
        return _audioClipsData != null &&
               clipId >= 0 &&
               clipId < _audioClipsData.Length &&
               _audioClipsData[clipId] != null &&
               _audioClipsData[clipId].clip != null;
    }

    /// <summary>
    /// DataManager에서 오디오 타입에 맞는 마스터 볼륨 반환
    /// </summary>
    private float GetCurrentVolume()
    {
        if (DataManager.Data)
            return _isBgmSource ? DataManager.Data.GetBgmVolume() : DataManager.Data.GetSfxVolume();

        return 1f;
    }

    /// <summary>
    /// 마스터 볼륨 × 클립 개별 볼륨 비율을 AudioSource에 적용
    /// </summary>
    /// <param name="clipId">개별 볼륨 비율을 가져올 클립 인덱스 (-1이면 비율 1.0 적용)</param>
    private void ApplyCurrentVolume(int clipId = -1) // 🎵 clipId 파라미터 추가
    {
        if (!IsAudioSourceReady()) return;

        float master = GetCurrentVolume();
        float scale = (clipId >= 0 && IsValidClipId(clipId))
            ? _audioClipsData[clipId].volumeScale
            : 1f;

        _audioSource.volume = master * scale; // 🎵 마스터 × 클립 비율
    }
}