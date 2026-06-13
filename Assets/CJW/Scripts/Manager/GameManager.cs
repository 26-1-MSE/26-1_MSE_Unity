using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


// <summary>
/// Singleton manager that controls the overall game flow and scene transitions.
/// It also handles loading UI, scene change notifications, and scene-specific initialization.
/// </summary>

public class GameManager : MonoBehaviour
{
  
    // Global singleton instance of GameManager
    public static GameManager Instance { get; private set; }


    // Returns the index of the currently active scene based on the scene name list.
    public int CurrentSceneIndex => _sceneNames.IndexOf(SceneManager.GetActiveScene().name);

    

    [Header("Managers")]
    [SerializeField] private AudioManager _audioManager;
    public AudioManager AudioManager => _audioManager;


    [Header("Scene Configuration")]
#if UNITY_EDITOR

    // Scene asset list used only in the Unity Editor for reference.
    [SerializeField] private List<SceneAsset> _sceneAssets = new List<SceneAsset>();
#endif

    // List of scene names used for scene transitions.
    // 0 = Lobby, 1 = PetTown, 2 = PetRoom, 3 = Island.
    [SerializeField]
    private List<string> _sceneNames = new List<string>()
    {
        "S0_Lobby",    // index 0
        "S1_PetTown",  // index 1
        "S2_PetRoom",  // index 2
        "S3_Island"    // index 3
    };


    [Header("UI")]
    [SerializeField] private GameObject _loadingPanel;

 
    // Event invoked when the current scene changes.
    public event Action<string> OnLocationChanged;

    // Indicates whether a scene transition is currently in progress.
    private bool _isTransitioning;

    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        BroadcastCurrentLocation();
    }

    /// <summary>
    /// Called automatically after a scene has finished loading.
    /// Resets transition state, applies global settings, hides loading UI,
    /// and requests updated user data when entering PetTown.
    /// </summary>

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _isTransitioning = false;

        BroadcastCurrentLocation();
        ApplyGlobalSettingsToScene();


        // When entering PetTown, refresh user and pet data from the server.
        if (scene.name == "S1_PetTown")
        {
            Debug.Log("[GameManager] Enter PetTown  : auth/status request");

            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.RequestAuthStatus(
                    response =>
                    {
                        Debug.Log("[GameManager] auth/status success");

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
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    /// <summary>
    /// Loads the scene at the given index.
    /// Prevents duplicate transitions and shows the loading panel before loading.
    /// </summary>
    
    public void TransitionToScene(int sceneIndex)
    {
        if (_isTransitioning)return;
        if (sceneIndex < 0 || sceneIndex >= _sceneNames.Count)
        {
            Debug.LogError($"Invalid scene index: {sceneIndex}");
            return;
        }

        _isTransitioning = true;
        string targetSceneName = _sceneNames[sceneIndex];


        SceneManager.LoadScene(targetSceneName);
    }

 
    public void GoToLobby()
    {
        TransitionToScene(0);
    }

    public void GoToPetTown()
    {
        TransitionToScene(1);
    }

    public void GoToPetRoom()
    {
        TransitionToScene(2);
    }

    public void GoToIsland()
    {
        TransitionToScene(3);
    }

    /// <summary>
    /// Broadcasts the current active scene name to listeners.
    /// Used for UI or location-related updates.
    /// </summary>
    
    private void BroadcastCurrentLocation()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;
        OnLocationChanged?.Invoke(activeSceneName);
    }

    /// <summary>
    /// Re-applies global settings stored in DataManager when a new scene is loaded.
    /// Currently used for audio setting synchronization.
    /// </summary>
    
    private void ApplyGlobalSettingsToScene()
    {
        if (DataManager.Data != null)
        {
            DataManager.Data.BroadcastAudioSettings();
        }
    }

}