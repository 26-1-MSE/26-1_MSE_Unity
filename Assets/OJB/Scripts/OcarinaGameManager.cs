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
    [SerializeField] private int totalNotes = 10;
    [SerializeField] private float noteSpacing = 150f;

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInteraction playerInteraction;

    private KeyCode[] possibleKeys = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F };
    private List<NoteObject> activeNotes = new List<NoteObject>();
    private List<KeyCode> noteSequence = new List<KeyCode>();

    private int currentPetTypeId;
    private int missCount = 0;
    private int clearedNotes = 0;
    private int spawnedNotes = 0;
    private float timeLeft;
    private bool isPlaying = false;
    private bool isSuccess = false;
    private GameObject currentPet;

    public bool IsPlaying => isPlaying;

    [SerializeField] private float judgeLineX = -400f;

    public static OcarinaGameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void StartGame(GameObject pet, int petTypeId)
    {
        if (isPlaying) return;
        currentPet = pet;
        currentPetTypeId = petTypeId;

        missCount = 0;
        clearedNotes = 0;
        spawnedNotes = 0;
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

        StartCoroutine(SpawnNotes());
    }

    private IEnumerator SpawnNotes()
    {
        int initialSpawn = Mathf.Min(5, totalNotes);
        for (int i = 0; i < initialSpawn; i++)
            SpawnNote();

        yield return null;
    }

    private void SpawnNote()
    {
        if (spawnedNotes >= totalNotes) return;

        GameObject noteObj = Instantiate(notePrefab, noteArea);
        NoteObject note = noteObj.GetComponent<NoteObject>();

        float spawnX = judgeLineX + noteSpacing * (activeNotes.Count + 1);
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
        activeNotes.RemoveAt(0);
        Destroy(firstNote.gameObject);

        if (key == firstNote.requiredKey)
        {
            clearedNotes++;
            Debug.Log($"클리어: {clearedNotes} / {totalNotes}");
        }
        else
        {
            missCount++;
            if (missCount <= hearts.Length)
                hearts[missCount - 1].SetActive(false);

            if (missCount >= 3)
            {
                Fail();
                return;
            }
        }

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
        if (NetworkManager.Instance) NetworkManager.Instance.RequestAcquirePet(currentPetTypeId);
        resultText.text = "Congratulations!\nYou got a pet!";
        resultPopup.SetActive(true);
        StartCoroutine(ClosePopupAfterDelay());
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

        resultText.text = "Failed.\nTry again next time.";
        resultPopup.SetActive(true);
        StartCoroutine(ClosePopupAfterDelay());
    }

    public void CloseGame()
    {
        ocarinaUI.SetActive(false);
    }

    private IEnumerator ClosePopupAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        resultPopup.SetActive(false);
        ocarinaUI.SetActive(false);

        // 실패했을 때만 재도전 가능하게
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
        SpawnNote();

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