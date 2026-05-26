using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// ���� ������ ��ü �帧�� �� ��ȯ�� �����ϴ� �̱��� �Ŵ���.
///
/// 1. ���� ���� ����(GameState) ����
/// 2. �� ��ȯ ó��
/// 3. �ε� UI ǥ��/����
/// 4. ���� �� ��ġ�� ��ε�ĳ��Ʈ
/// 5. �� �� ���� �� DataManager�� ���� ������ �ٽ� ����

public class GameManager : MonoBehaviour
{
    /// ���� ���ٿ� �̱��� �ν��Ͻ�.
    public static GameManager Instance { get; private set; }


    // =========================================================
    // 1. ���� ���� ����

    private GameState _currentState;
    public GameState CurrentState => _currentState;
    
    public int CurrentSceneIndex => _sceneNames.IndexOf(SceneManager.GetActiveScene().name);

    // =========================================================
    // 2. �Ŵ��� ����

    [Header("Managers")]

    [SerializeField] private AudioManager _audioManager;

    /// �ܺ� �б� ���� ������Ƽ.
    public AudioManager AudioManager => _audioManager;


    // =========================================================
    // 3. �� ����

    [Header("Scene Configuration")]
#if UNITY_EDITOR

    /// �����Ϳ��� �� ������ ���� ���� �����ϱ� ���� ����Ʈ.
    /// ���忡���� ���� ������� �ʰ� ��������θ� �д�.
    [SerializeField] private List<SceneAsset> _sceneAssets = new List<SceneAsset>();
#endif

    /// ���� �� ��ȯ�� ����� �� �̸� ���.
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
    // 4. UI ���� ����

    [Header("UI")]

    /// �� ��ȯ �� ǥ���� �ε� �г�.
    /// null�̸� �ε� UI ���� �����Ѵ�

    [SerializeField] private GameObject _loadingPanel;

    // =========================================================
    // 5. �̺�Ʈ

    /// ���� ���°� �ٲ���� �� ȣ��Ǵ� �̺�Ʈ.           
    public event Action<GameState> OnGameStateChanged;
    public event Action<string> OnLocationChanged;

    // =========================================================
    // 6. ���� ���°�

    /// ���� �� ��ȯ ������ ����.
    /// �ߺ� LoadScene ȣ�� ������.
    private bool _isTransitioning;
    private string _previousSceneName = string.Empty;

    // =========================================================
    // 7. Unity �����ֱ�

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // �� �ε� �Ϸ� �̺�Ʈ ����.	
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // ���� ���� ���� ��ġ�� �� �� ��ε�ĳ��Ʈ
        BroadcastCurrentLocation();
    }

    private void Update()
    {
        // ���� ���°� �����ϸ� �Է� ó���� ������Ʈ�� ���� ��ü�� �����Ѵ�.
        if (_currentState != null)
        {
            _currentState.HandleInput();
            _currentState.Update();
        }
    }

    /// �� ���� �ε�� ���� ȣ��ȴ�.
    ///
    /// 1. �� ��ȯ �÷��� ����
    /// 2. Ȱ�� �� �̸��� �������� ���� ���� ����ȭ
    /// 3. ���� ��ġ�� ��ε�ĳ��Ʈ
    /// 4. DataManager�� ����� ���� ���� ������
    /// 5. �ε� UI �����

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _isTransitioning = false;

        BroadcastCurrentLocation();
        ApplyGlobalSettingsToScene();
        HideLoading();

        if (scene.name == "S1_PetTown")
        {
            if (_previousSceneName == "S0_Lobby")
            {
                Debug.Log("[GameManager] Lobby -> PetTown : �α��� ���� ������ �ʱ�ȭ");


                if (NetworkManager.Instance != null)
                {
                    NetworkManager.Instance.RequestInventoryData();
                }
            }
            else // �ٸ� ������ �� ���
            {
                Debug.Log("[GameManager] Other Scene -> PetTown : �κ��丮 ������ ����");

                if (NetworkManager.Instance != null)
                {
                    NetworkManager.Instance.RequestInventoryData();
                }
            }
        }
   }

    private void OnDestroy()
    {
        // ���� ������Ʈ�� ���� �̱��� �ν��Ͻ��� ���� �̺�Ʈ ����.
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // =========================================================
    // 8. ���� ���� ���� �޼���

    /// ���� ���� ���¸� �����Ѵ�. (���� ���� \ ������Ʈ ���� X)
  
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

    /// ���� ���°� Ư�� Ÿ������ Ȯ���Ѵ�.
    public bool IsCurrentState<T>() where T : GameState
    {
        return _currentState is T;
    }


    // =========================================================
    // 9. �� ��ȯ ���� �޼���

    /// GameState ���� �������� �� ��ȯ�� �����Ѵ�.
    /// ���������� �ش� ���¿� �����ϴ� �� �ε����� ���� ��ȯ�Ѵ�.
    public void TransitionToScene(int sceneIndex)
    {
        // �̹� �� ��ȯ ���̸� �ߺ� ȣ���� ���´�.
        if (_isTransitioning)return;
        if (sceneIndex < 0 || sceneIndex >= _sceneNames.Count)
        {
            Debug.LogError($"Invalid scene index: {sceneIndex}");
            return;
        }

        _previousSceneName = SceneManager.GetActiveScene().name;
        _isTransitioning = true;
        ShowLoading();
        string targetSceneName = _sceneNames[sceneIndex];


        SceneManager.LoadScene(targetSceneName);
    }

    /// Lobby ������ �̵��Ѵ�.
    public void GoToLobby()
    {
        TransitionToScene(0);
    }

    /// ���� ����� PetTown ������ �̵��Ѵ�.
    public void GoToPetTown()
    {
        TransitionToScene(1);
    }

    /// PetRoom ������ �̵��Ѵ�.
    public void GoToPetRoom()
    {
        TransitionToScene(2);
    }

    /// Island ������ �̵��Ѵ�.
    public void GoToIsland()
    {
        TransitionToScene(3);
    }

    // =========================================================
    // 10. ���� UI / ����� ����

    /// ���� Ȱ�� �� �̸��� UI�� ��ġ ���ڿ��� �ٲ� �̺�Ʈ�� �����Ѵ�.
    private void BroadcastCurrentLocation()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        OnLocationChanged?.Invoke(activeSceneName);
    }

    /// DataManager�� ����� ���� �������� ���� �� ��ü���� �ٽ� ������ �� �ֵ��� ��ε�ĳ��Ʈ�Ѵ�.
    ///
    /// ����:
    /// DataManager�� event�� �ܺ� Ŭ�������� ���� Invoke �� �� �����Ƿ�
    /// DataManager ������ BroadcastAudioSettings() �޼��带 ȣ���ؾ� �Ѵ�.
    private void ApplyGlobalSettingsToScene()
    {
        if (DataManager.Data != null)
        {
            DataManager.Data.BroadcastAudioSettings();
        }
    }

    // =========================================================
    // 13. �ε� UI ����
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