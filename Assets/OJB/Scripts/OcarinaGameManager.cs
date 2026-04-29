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
    [SerializeField] private PlayerInteraction playerInteraction; // 추가: E키 입력 방지용

    private KeyCode[] possibleKeys = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F };
    private List<NoteObject> activeNotes = new List<NoteObject>();
    private List<KeyCode> noteSequence = new List<KeyCode>();

    private int missCount = 0;
    private int clearedNotes = 0;
    private int spawnedNotes = 0;
    private float timeLeft;
    private bool isPlaying = false;

    [SerializeField] private float judgeLineX = -400f;

    public static OcarinaGameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void StartGame()
    {
        if (isPlaying) return; // 추가: 이미 플레이 중이면 중복 실행 방지

        missCount = 0;
        clearedNotes = 0;
        spawnedNotes = 0;
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

        // 추가: 플레이어 이동 + 상호작용 둘 다 비활성화
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

        if (key == firstNote.requiredKey)
        {
            activeNotes.RemoveAt(0);
            Destroy(firstNote.gameObject);
            clearedNotes++;

            SpawnNote();

            if (clearedNotes >= totalNotes)
                Success();
        }
        else
        {
            activeNotes.RemoveAt(0);
            Destroy(firstNote.gameObject);
            SpawnNote();

            missCount++;
            if (missCount <= hearts.Length)
                hearts[missCount - 1].SetActive(false);

            if (missCount >= 3)
                Fail();
        }
    }

    private void Success()
    {
        isPlaying = false;
        // 추가: 플레이어 이동 + 상호작용 다시 활성화
        playerMovement.enabled = true;
        playerInteraction.enabled = true;
        resultText.text = "성공!";
        resultPopup.SetActive(true);
    }

    private void Fail()
    {
        isPlaying = false;
        // 추가: 플레이어 이동 + 상호작용 다시 활성화
        playerMovement.enabled = true;
        playerInteraction.enabled = true;

        foreach (var note in activeNotes)
            if (note != null) Destroy(note.gameObject);
        activeNotes.Clear();

        resultText.text = "다음에 다시 도전해요";
        resultPopup.SetActive(true);
    }

    public void CloseGame()
    {
        ocarinaUI.SetActive(false);
    }
}