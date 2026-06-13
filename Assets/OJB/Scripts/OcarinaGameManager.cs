using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;


// Manages the rhythm mini-game flow including note spawning, input judgment, and success/failure handling.
public class OcarinaGameManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject ocarinaUI;
    [SerializeField] private Transform noteArea;
    [SerializeField] private GameObject notePrefab;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject[] hearts; // Heart icons representing remaining miss chances
    [SerializeField] private GameObject resultPopup;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private ToastMessage toastMessage;

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 30f;
    [SerializeField] private int totalNotes = 14;
    [SerializeField] private float noteSpacing = 150f; // Horizontal spacing between spawned notes

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private GameObject UIManager;

    [Header("Audio Manager Clip IDs")]
    [SerializeField] private int successSoundId;
    [SerializeField] private int failSoundId;
    [SerializeField] private int wrongSFXId;
    [SerializeField] private int[] twinkleNoteIds; // Clip IDs for each note of the melody (14 total)

    [Header("Note Timing")]
    [SerializeField] private float[] noteTiming = {
        0f,    // 도
        0.5f,  // 도
        1.0f,  // 솔
        1.5f,  // 솔
        2.0f,  // 라
        2.5f,  // 라
        3.0f,  // 솔
        4.0f,  // 파
        4.5f,  // 파
        5.0f,  // 미
        5.5f,  // 미
        6.0f,  // 레
        6.5f,  // 레
        7.0f,  // 도
    };

    private KeyCode[] possibleKeys = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F };
    private List<NoteObject> activeNotes = new List<NoteObject>();
    private List<KeyCode> noteSequence = new List<KeyCode>();

    private int currentPetTypeId;
    private int missCount = 0;
    private int clearedNotes = 0;
    private int spawnedNotes = 0;
    private int currentNoteIndex = 0; // Index into twinkleNoteIds for sequential melody playback
    private float timeLeft;
    private bool isPlaying = false;
    private bool isSuccess = false;
    private GameObject currentPet; // The wild pet object being tamed

    public bool IsPlaying => isPlaying;

    public static OcarinaGameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

   
    // Initializes and starts the rhythm mini-game for the given pet.
    public void StartGame(GameObject pet, int petTypeId)
    {
        if (isPlaying) return;

        StopAllCoroutines();

        // Clear any leftover notes from a previous session
        foreach (var note in activeNotes)
            if (note != null) Destroy(note.gameObject);
        activeNotes.Clear();

        foreach (var note in FindObjectsByType<NoteObject>(FindObjectsSortMode.None))
            Destroy(note.gameObject);

        currentPet = pet;
        currentPetTypeId = petTypeId;

        missCount = 0;
        clearedNotes = 0;
        spawnedNotes = 0;
        currentNoteIndex = 0;
        isSuccess = false;
        timeLeft = gameDuration;
        isPlaying = true;
        activeNotes.Clear();
        noteSequence.Clear();

        foreach (var heart in hearts)
            heart.SetActive(true);

        resultPopup.SetActive(false);

        // Generate a random key sequence for the notes
        for (int i = 0; i < totalNotes; i++)
            noteSequence.Add(possibleKeys[Random.Range(0, possibleKeys.Length)]);

        ocarinaUI.SetActive(true);
        playerMovement.enabled = false;
        playerInteraction.enabled = false;

        StartCoroutine(SpawnNotesWithTiming());
    }

    // Spawns notes one by one with a fixed interval
    private IEnumerator SpawnNotesWithTiming()
    {
        for (int i = 0; i < totalNotes; i++)
        {
            SpawnNote();
            yield return new WaitForSeconds(1.0f);
        }
    }

    // Instantiates a single note and positions it to the right of existing notes
    private void SpawnNote()
    {
        if (spawnedNotes >= totalNotes) return;

        GameObject noteObj = Instantiate(notePrefab, noteArea);
        NoteObject note = noteObj.GetComponent<NoteObject>();

        float spawnX = noteSpacing * (activeNotes.Count + 1);
        noteObj.transform.localPosition = new Vector3(spawnX, 0, 0);

        note.Init(noteSequence[spawnedNotes]);
        activeNotes.Add(note);
        spawnedNotes++;
    }

    private void Update()
    {
        if (!isPlaying) return;

        timeLeft -= Time.deltaTime;
        timerText.text = Mathf.CeilToInt(timeLeft).ToString();

        if (timeLeft <= 0)
        {
            Fail();
            return;
        }

        if (Keyboard.current.aKey.wasPressedThisFrame) HandleInput(KeyCode.A);
        else if (Keyboard.current.sKey.wasPressedThisFrame) HandleInput(KeyCode.S);
        else if (Keyboard.current.dKey.wasPressedThisFrame) HandleInput(KeyCode.D);
        else if (Keyboard.current.fKey.wasPressedThisFrame) HandleInput(KeyCode.F);
    }

    // Processes a key input against the frontmost active note
    private void HandleInput(KeyCode key)
    {
        if (activeNotes.Count == 0) return;

        NoteObject firstNote = activeNotes[0];

        // Ignore input if the note is not within the judge range
        if (!firstNote.IsJudgeable) return;

        firstNote.MarkHandled();
        activeNotes.RemoveAt(0);
        Destroy(firstNote.gameObject);

        if (key == firstNote.requiredKey)
        {
            clearedNotes++;
            // Debug.Log($"클리어: {clearedNotes} / {totalNotes}");

            // Play the corresponding melody note on a correct hit
            if (AudioManager.SFXInstance != null && twinkleNoteIds != null && currentNoteIndex < twinkleNoteIds.Length)
                AudioManager.SFXInstance.PlayOneShot(twinkleNoteIds[currentNoteIndex]);
        }
        else
        {
            // Play wrong SFX and deduct a heart on a miss
            if (AudioManager.SFXInstance != null)
                AudioManager.SFXInstance.PlayOneShot(wrongSFXId);

            missCount++;
            if (missCount <= hearts.Length)
                hearts[missCount - 1].SetActive(false);

            if (missCount >= 3)
            {
                Fail();
                return;
            }
        }

        currentNoteIndex++;
        SpawnNote();

        if (clearedNotes + missCount >= totalNotes)
            Success();
    }

    // Handles success: hides the pet, requests server pet acquisition, and closes the game
    private void Success()
    {
        isPlaying = false;
        playerMovement.enabled = true;
        playerInteraction.enabled = true;

        if (currentPet != null)
            currentPet.SetActive(false);

        // Debug.Log("[PET_COLLECT] 펫 획득!");

        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.RequestAcquirePet(
                currentPetTypeId,
                () =>
                {
                    isSuccess = true;
                    // Debug.Log("[PET_COLLECT] 서버 저장 성공");

                    NetworkManager.Instance.RequestInventoryData(
                        response =>
                        {
                            // Debug.Log("[PET_COLLECT] 최신 인벤토리 재조회 성공");

                            if (UIManager != null)
                            {
                                UIManager.GetComponent<DisplayPetUI>()?.RefreshPetInventory();
                                UIManager.GetComponent<ItemInventoryManager>()?.RefreshItemInventory();
                            }

                            AudioManager.SFXInstance?.PlayOneShot(successSoundId);
                            CloseGame();
                        },
                        error =>
                        {
                            Debug.LogError("[PET_COLLECT] 인벤토리 재조회 실패: " + error);
                            toastMessage?.ShowToast(error);

                            AudioManager.SFXInstance?.PlayOneShot(successSoundId);
                            CloseGame();
                        }
                    );
                },
                error =>
                {
                    isSuccess = false;
                    Debug.LogError("[PET_COLLECT] 서버 저장 실패: " + error);

                    toastMessage?.ShowToast(error);

                    // Restore the pet if server request fails
                    if (currentPet != null)
                        currentPet.SetActive(true);

                    CloseGame();
                }
            );
        }
        else
        {
            // Local test fallback when NetworkManager is not present
            isSuccess = true;
            // Debug.Log("[PET_COLLECT] NetworkManager 없음 - 로컬 테스트 중");

            if (AudioManager.SFXInstance != null)
                AudioManager.SFXInstance.PlayOneShot(successSoundId);

            CloseGame();
        }
    }

    // Handles failure: clears remaining notes and closes the game
    private void Fail()
    {
        isSuccess = false;
        isPlaying = false;
        playerMovement.enabled = true;
        playerInteraction.enabled = true;

        foreach (var note in activeNotes)
            if (note != null) Destroy(note.gameObject);
        activeNotes.Clear();

        if (AudioManager.SFXInstance != null)
            AudioManager.SFXInstance.PlayOneShot(failSoundId);

        CloseGame();
    }


    // Closes the game UI and resets the pet interactable on failure for retry.
    public void CloseGame()
    {
        if (resultPopup != null)
            resultPopup.SetActive(false);

        if (ocarinaUI != null)
            ocarinaUI.SetActive(false);

        if (!isSuccess && currentPet != null)
        {
            currentPet.SetActive(true);

            PetInteractable pet = currentPet.GetComponent<PetInteractable>();
            if (pet != null)
                pet.ResetInteraction();
        }
    }


    //Called by NoteObject when a note passes the miss line without being hit.
    public void OnNoteMissed(NoteObject note)
    {
        if (!isPlaying) return;
        if (!activeNotes.Contains(note)) return;

        activeNotes.Remove(note);
        Destroy(note.gameObject);

        if (AudioManager.SFXInstance != null)
            AudioManager.SFXInstance.PlayOneShot(wrongSFXId);
        currentNoteIndex++;

        missCount++;
        if (missCount <= hearts.Length)
            hearts[missCount - 1].SetActive(false);

        if (missCount >= 3)
        {
            Fail();
            return;
        }

        if (clearedNotes + missCount >= totalNotes)
            Success();
    }
}