using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// 현재 게임의 전체 흐름과 씬 전환을 관리하는 싱글톤 매니저.
///
/// 1. 현재 게임 상태(GameState) 관리
/// 2. 씬 전환 처리
/// 3. 로딩 UI 표시/해제
/// 4. 현재 씬 위치명 브로드캐스트
/// 5. 새 씬 진입 시 DataManager의 전역 설정값 다시 적용
///
/// 참고:
/// - 플레이어 데이터 자체는 DataManager가 관리한다.
/// - 실제 사운드 재생은 AudioManager가 담당한다.
/// - GameManager는 "게임 흐름과 씬 이동" 중심의 매니저이다.

/// GDD 기준 씬 구조:
/// Lobby → PetTown → PetRoom / Island
/// Loading : 씬 전환 중 상태

public class GameManager : MonoBehaviour
{
    /// 전역 접근용 싱글톤 인스턴스.
    /// 다른 클래스에서 GameManager.Instance 로 접근한다.
    public static GameManager Instance { get; private set; }

    // =========================================================
    // 1. 현재 게임 상태

    private GameState _currentState;
    public GameState CurrentState => _currentState;

    // =========================================================
    // 2. 매니저 참조

    [Header("Managers")]

    /// 현재 씬 또는 공용 오브젝트에서 참조할 AudioManager.
    /// 실제 BGM / SFX 재생은 이 매니저가 담당한다.
    [SerializeField] private AudioManager _audioManager;

    /// 외부 읽기 전용 프로퍼티.
    public AudioManager AudioManager => _audioManager;


    // =========================================================
    // 3. 씬 설정

    [Header("Scene Configuration")]
#if UNITY_EDITOR

    /// 에디터에서 씬 에셋을 보기 쉽게 관리하기 위한 리스트.
    /// 빌드에서는 직접 사용하지 않고 참고용으로만 둔다.
    [SerializeField] private List<SceneAsset> _sceneAssets = new List<SceneAsset>();
#endif

    /// 실제 씬 전환에 사용할 씬 이름 목록.
    /// 인덱스와 GameState를 서로 대응시켜 사용한다.
    /// 
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
    // 4. UI 관련 설정

    [Header("UI")]

    /// 씬 전환 중 표시할 로딩 패널.
    /// null이면 로딩 UI 없이 동작한다

    [SerializeField] private GameObject _loadingPanel;

    // =========================================================
    // 5. 이벤트

    /// 게임 상태가 바뀌었을 때 호출되는 이벤트.
    /// 예: UI가 현재 상태에 따라 표시를 바꿀 때 사용 가능.
    public event Action<GameState> OnGameStateChanged;

    /// 현재 위치명이 바뀌었을 때 호출되는 이벤트.
    /// 예: 우측 상단 현재 위치 텍스트 갱신. 
    /// 필요한지는>?
    public event Action<string> OnLocationChanged;

    // =========================================================
    // 6. 내부 상태값

    /// 현재 씬 전환 중인지 여부.
    /// 중복 LoadScene 호출 방지용.
    private bool _isTransitioning;

    // =========================================================
    // 7. Unity 생명주기

    private void Awake()
    {

        // 이미 다른 GameManager가 존재하면 현재 오브젝트는 제거한다.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 이미 다른 GameManager가 존재하면 현재 오브젝트는 제거한다.
        Instance = this;
        // 씬이 바뀌어도 유지되도록 설정한다.
        DontDestroyOnLoad(gameObject);

        // 씬 로드 완료 이벤트 구독.	
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // 시작 직후 현재 위치명도 한 번 브로드캐스트한다.
        BroadcastCurrentLocation();
    }

    private void Update()
    {
        // 현재 상태가 존재하면 입력 처리와 업데이트를 상태 객체에 위임한다.
        if (_currentState != null)
        {
            _currentState.HandleInput();
            _currentState.Update();
        }
    }

    /// 새 씬이 로드된 직후 호출된다.
    ///
    /// 1. 씬 전환 플래그 해제
    /// 2. 활성 씬 이름을 기준으로 현재 상태 동기화
    /// 3. 현재 위치명 브로드캐스트
    /// 4. DataManager에 저장된 사운드 설정 재적용
    /// 5. 로딩 UI 숨기기

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _isTransitioning = false;

        BroadcastCurrentLocation();
        ApplyGlobalSettingsToScene();
        HideLoading();
    }

    private void OnDestroy()
    {
        // 현재 오브젝트가 실제 싱글톤 인스턴스일 때만 이벤트 해제.
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // =========================================================
    // 8. 게임 상태 관련 메서드

    /// 현재 게임 상태를 변경한다.
    /// 상태가 실제로 바뀌었을 때만 이벤트를 호출한다.	
    public void ChangeState(GameState newState)
    {
        if (_currentState != null)
        {
            _currentState.OnExit();
        }

        _currentState = newState;

        if (_currentState != null)
        {
            _currentState.OnEnter();
        }

        OnGameStateChanged?.Invoke(_currentState);
    }

    /// 현재 상태가 특정 타입인지 확인한다.
    public bool IsCurrentState<T>() where T : GameState
    {
        return _currentState is T;
    }


    // =========================================================
    // 9. 씬 전환 관련 메서드

    /// GameState 값을 기준으로 씬 전환을 수행한다.
    /// 내부적으로 해당 상태에 대응하는 씬 인덱스를 구해 전환한다.
    public void TransitionToScene(int sceneIndex)
    {
        // 이미 씬 전환 중이면 중복 호출을 막는다.
        if (_isTransitioning)
            return;

        // 인덱스가 유효 범위를 벗어나면 에러 로그 출력 후 종료한다.
        if (sceneIndex < 0 || sceneIndex >= _sceneNames.Count)
        {
            Debug.LogError($"Invalid scene index: {sceneIndex}");
            return;
        }

        _isTransitioning = true;
        ShowLoading();

        string targetSceneName = _sceneNames[sceneIndex];

        // 기존 BGM 오브젝트가 남아 있다면 정리한다.
        // 씬마다 새 BGM을 재생하는 구조일 때 중복 재생 방지용이다.
        /*if (_audioManager != null && _audioManager.BGMInstance != null)
        {
            Destroy(_audioManager.BGMInstance.gameObject);
        }
        */

        SceneManager.LoadScene(targetSceneName);
    }

    /// Lobby 씬으로 이동한다.
    public void GoToLobby()
    {
        TransitionToScene(0);
    }

    /// 메인 허브인 PetTown 씬으로 이동한다.
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
    // 10. 공통 UI / 오디오 적용

    /// 현재 활성 씬 이름을 UI용 위치 문자열로 바꿔 이벤트로 전달한다.
    private void BroadcastCurrentLocation()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        OnLocationChanged?.Invoke(activeSceneName);
    }

    /// DataManager에 저장된 전역 설정값을 현재 씬 객체들이 다시 적용할 수 있도록 브로드캐스트한다.
    ///
    /// 주의:
    /// DataManager의 event는 외부 클래스에서 직접 Invoke 할 수 없으므로
    /// DataManager 내부의 BroadcastAudioSettings() 메서드를 호출해야 한다.
    private void ApplyGlobalSettingsToScene()
    {
        if (DataManager.Data != null)
        {
            DataManager.Data.BroadcastAudioSettings();
        }
    }


    // =========================================================
    // 13. 로딩 UI 제어

    private void ShowLoading()
    {
        if (_loadingPanel != null)
        {
            _loadingPanel.SetActive(true);
        }
    }

    private void HideLoading()
    {
        if (_loadingPanel != null)
        {
            _loadingPanel.SetActive(false);
        }
    }
}