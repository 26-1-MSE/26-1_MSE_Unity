using UnityEngine;

public enum GameState
{
    Intro,
    Room1,
    Room2,
    Victory,
    Defeat
}

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance for global access
    /// </summary>
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameState _gameState = GameState.Intro;
    /// <summary>
    /// Current game state
    /// </summary>
    public GameState GameState => _gameState;
    
    [Header("Audio Components")]
    [SerializeField] public AudioManager AudioManager;
    
    [Header("Scene Configuration")]
#if UNITY_EDITOR
    [SerializeField] private List<SceneAsset> _sceneAssets = new List<SceneAsset>(); // Scene asset list for editor
#endif
    [SerializeField] private List<string> _sceneNames = new List<string>(); // Scene name list for build

    /// <summary>
    /// Initialize the AudioManager instance and set up DontDestroyOnLoad if enabled
    /// </summary>
    void Awake()
    {
        // Implement singleton pattern
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Subscribe to scene loading events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    /// <summary>
    /// Called when a scene finishes loading
    /// </summary>
    /// <param name="scene">The loaded scene</param>
    /// <param name="mode">The scene load mode</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Initialize Scene 0 if flag is set
        if (_shouldInitializeScene0Load && scene.name == _sceneNames[0])
        {
            _shouldInitializeScene0Load = false;
            Initialize();
        }
    }
    
    /// <summary>
    /// Changes the current game state and handles state-specific logic
    /// </summary>
    /// <param name="newGameState">The new game state to set</param>
    public void SetGameState(GameState newGameState)
    {
        // Avoid redundant state changes
        if (_gameState == newGameState)
            return;

        this._gameState = newGameState;

        // Handle state-specific logic
        switch (_gameState)
        {
            case GameState.Victory:
                // Transition to ending scene after delay
                TransitionToScene(3);
                break;
            case GameState.Defeat:
                _isDead = true;
                Debug.Log("Defeat");
                SetFailUIActive(true);
                if (AudioManager) AudioManager.PlayAudio(1);
                SetMissionState(MissionState.Ending);

                // Return to main scene after delay
                TransitionToScene(0);
                break;
            default:
                break;
        }
    }
    
    /// <summary>
    /// Transitions to the specified scene by index
    /// </summary>
    /// <param name="sceneIndex">Index of the target scene in the scene list</param>
    public void TransitionToScene(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= _sceneNames.Count)
        {
            Debug.LogError($"Invalid scene index: {sceneIndex}. Available scenes: {_sceneNames.Count}");
            return;
        }

        if (sceneIndex == 0)
        {
            _shouldInitializeScene0Load = true;
            Initialize();

            if (AudioManager.BGMInstance)
                Destroy(AudioManager.BGMInstance.gameObject);
        }

        SceneManager.LoadScene(_sceneNames[sceneIndex]);
    }

    
    /// <summary>
    /// Clean up event subscriptions when destroyed
    /// </summary>
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
