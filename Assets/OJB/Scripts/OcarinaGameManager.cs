using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class OcarinaGameManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject ocarinaUI;
    [SerializeField] private Transform noteArea;
    [SerializeField] private GameObject notePrefab;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject[] hearts;
    [SerializeField] private GameObject resultPopup;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("Game Settings")]
    [SerializeField] private float gameDuration = 30f;
    [SerializeField] private int totalNotes = 14;
    [SerializeField] private float noteSpacing = 150f;

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInteraction playerInteraction;

    [Header("Audio Manager Clip IDs")]
    [SerializeField] private int successSoundId;
    [SerializeField] private int failSoundId;
    [SerializeField] private int wrongSFXId;
    [SerializeField] private int[] twinkleNoteIds;

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
    private int currentNoteIndex = 0;
    private float timeLeft;
    private bool isPlaying = false;
    private bool isSuccess = false;
    private GameObject currentPet;

    public bool IsPlaying => isPlaying;

    public static OcarinaGameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void StartGame(GameObject pet, int petTypeId)
    {
        if (isPlaying) return;

        StopAllCoroutines();

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

        for (int i = 0; i < totalNotes; i++)
            noteSequence.Add(possibleKeys[Random.Range(0, possibleKeys.Length)]);

        ocarinaUI.SetActive(true);
        playerMovement.enabled = false;
        playerInteraction.enabled = false;

        StartCoroutine(SpawnNotesWithTiming());
    }

    private IEnumerator SpawnNotesWithTiming()
    {
        for (int i = 0; i < totalNotes; i++)
        {
            SpawnNote();
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void SpawnNote()
    {
        if (spawnedNotes >= totalNotes) return;

        GameObject noteObj = Instantiate(notePrefab, noteArea);
        NoteObject note = noteObj.GetComponent<NoteObject>();

        // 오른쪽에서 스폰
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

    private void HandleInput(KeyCode key)
    {
        if (activeNotes.Count == 0) return;

        NoteObject firstNote = activeNotes[0];

        if (!firstNote.IsJudgeable) return;

        firstNote.MarkHandled();
        activeNotes.RemoveAt(0);
        Destroy(firstNote.gameObject);

        if (key == firstNote.requiredKey)
        {
            clearedNotes++;
            Debug.Log($"클리어: {clearedNotes} / {totalNotes}");

            if (AudioManager.SFXInstance != null && twinkleNoteIds != null && currentNoteIndex < twinkleNoteIds.Length)
                AudioManager.SFXInstance.PlayOneShot(twinkleNoteIds[currentNoteIndex]);
        }
        else
        {
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

    private void Success()
    {
        isSuccess = true;
        isPlaying = false;
        playerMovement.enabled = true;
        playerInteraction.enabled = true;

        if (currentPet != null)
            currentPet.SetActive(false);

        Debug.Log("[PET_COLLECT] 펫 획득!");

        if (NetworkManager.Instance != null)
            NetworkManager.Instance.RequestAcquirePet(currentPetTypeId);
        else
            Debug.Log("[PET_COLLECT] NetworkManager 없음 - 로컬 테스트 중");

        if (AudioManager.SFXInstance != null)
            AudioManager.SFXInstance.PlayOneShot(successSoundId);

        CloseGame();
    }

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

    public void CloseGame()
    {
        resultPopup.SetActive(false);
        ocarinaUI.SetActive(false);

        if (!isSuccess && currentPet != null)
        {
            PetInteractable pet = currentPet.GetComponent<PetInteractable>();
            if (pet != null)
                pet.ResetInteraction();
        }
    }

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