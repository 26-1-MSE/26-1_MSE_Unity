using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 전체 흐름과 씬 전환을 관리하는 싱글톤 매니저.
/// 
/// 주요 역할:
/// 1. 씬 전환 처리
/// 2. 로딩 UI 표시/숨김
/// 3. 현재 위치 변경 이벤트 전달
/// 4. 씬 로드 시 필요한 전역 설정 적용
/// </summary>

public class GameManager : MonoBehaviour
{
    /// 게임 전체에서 접근 가능한 GameManager 싱글톤 인스턴스.
    public static GameManager Instance { get; private set; }


    // =========================================================
    /// 현재 활성화된 씬의 인덱스
    public int CurrentSceneIndex => _sceneNames.IndexOf(SceneManager.GetActiveScene().name);

    // =========================================================
    // 2. 매니저 참조

    [Header("Managers")]
    [SerializeField] private AudioManager _audioManager;

    /// 외부에서 접근 가능한 AudioManager 참조
    public AudioManager AudioManager => _audioManager;


    // =========================================================
    // 3. 씬 설정

    [Header("Scene Configuration")]
#if UNITY_EDITOR

    /// 에디터에서 씬 에셋을 확인하기 위한 리스트
    /// 빌드에서는 직접 사용하지 않고 참고용으로 둔다
    [SerializeField] private List<SceneAsset> _sceneAssets = new List<SceneAsset>();
#endif

    /// 씬 전환에 사용할 씬 이름 목록.
    /// 0 = Lobby
    /// 1 = PetTown
    /// 2 = PetRoom
    /// 3 = Island

    [SerializeField]
    private List<string> _sceneNames = new List<string>()
    {
        "S0_Lobby",    // index 0
        "S1_PetTown",  // index 1
        "S2_PetRoom",  // index 2
        "S3_Island"    // index 3
    };

    // =========================================================
    // 4. UI 설정

    [Header("UI")]

    /// 씬 전환 중 표시할 로딩 패널
    /// null이면 로딩 UI를 사용하지 않는다

    [SerializeField] private GameObject _loadingPanel;

    // =========================================================
    // 5. 이벤트

    /// 현재 씬 위치가 변경될 때 호출되는 이벤트.
    /// 씬 이름을 문자열로 전달한다.
    public event Action<string> OnLocationChanged;

    // =========================================================
    // 6. 씬 전환 상태

    /// 현재 씬 전환 중인지 여부.
    /// 중복 LoadScene 호출을 방지하기 위해 사용한다.
    private bool _isTransitioning;

    // =========================================================
    // 7. Unity 생명주기

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 씬 로드 완료 이벤트 등록.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // 시작 시 현재 위치를 한 번 전달한다.
        BroadcastCurrentLocation();
    }

    /// 씬 로드가 완료될 때 호출된다.
    /// 
    /// 처리 내용:
    /// 1. 씬 전환 플래그 해제
    /// 2. 현재 위치 이벤트 전달
    /// 3. 전역 설정 적용
    /// 4. 로딩 UI 숨김
    /// 5. PetTown 진입 시 auth/status 요청 후 펫 스폰 갱신

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _isTransitioning = false;

        BroadcastCurrentLocation();
        ApplyGlobalSettingsToScene();
        HideLoading();

        if (scene.name == "S1_PetTown")
        {
            Debug.Log("[GameManager] PetTown 진입 : auth/status 요청");

            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.RequestAuthStatus(
                    response =>
                    {
                        Debug.Log("[GameManager] auth/status 성공");

                        PetSpawner petSpawner = FindFirstObjectByType<PetSpawner>();
                        if (petSpawner != null)
                        {
                            petSpawner.SpawnPets();
                        }
                    }
                );
            }
        }
    }

    private void OnDestroy()
    {
        // 현재 인스턴스가 본인일 때만 씬 로드 이벤트를 해제한다.
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // =========================================================
    // 9. 씬 전환 메서드

    /// 지정한 인덱스에 해당하는 씬으로 전환한다.
    /// 중복 전환을 방지하고, 로딩 UI를 표시한 뒤 씬을 로드한다.
    public void TransitionToScene(int sceneIndex)
    {
        if (_isTransitioning)return;
        if (sceneIndex < 0 || sceneIndex >= _sceneNames.Count)
        {
            Debug.LogError($"Invalid scene index: {sceneIndex}");
            return;
        }

        _isTransitioning = true;
        ShowLoading();
        string targetSceneName = _sceneNames[sceneIndex];


        SceneManager.LoadScene(targetSceneName);
    }

    /// Lobby 씬으로 이동한다.
    public void GoToLobby()
    {
        TransitionToScene(0);
    }

    /// PetTown 씬으로 이동한다.
    public void GoToPetTown()
    {
        TransitionToScene(1);
    }

    /// PetRoom 씬으로 이동한다.
    public void GoToPetRoom()
    {
        TransitionToScene(2);
    }

    /// Island 씬으로 이동한다.
    public void GoToIsland()
    {
        TransitionToScene(3);
    }

    // =========================================================
    // 10. 위치 이벤트 및 전역 설정

    /// 현재 활성화된 씬 이름을 OnLocationChanged 이벤트로 전달한다.
    private void BroadcastCurrentLocation()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        OnLocationChanged?.Invoke(activeSceneName);
    }

    /// DataManager에 저장된 전역 설정을 새 씬에 다시 적용한다.
    private void ApplyGlobalSettingsToScene()
    {
        if (DataManager.Data != null)
        {
            DataManager.Data.BroadcastAudioSettings();
        }
    }

    // =========================================================
    // 11. 로딩 UI 제어

    /// 로딩 패널을 표시한다.
    private void ShowLoading()
    {
        if (_loadingPanel != null)
        {
            _loadingPanel.SetActive(true);
        }
    }

    /// 로딩 패널을 숨긴다.
    private void HideLoading()
    {
        if (_loadingPanel != null)
        {
            _loadingPanel.SetActive(false);
        }
    }
}