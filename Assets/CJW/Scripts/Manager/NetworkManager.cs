using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Unity 클라이언트 서버 통신을 관리하는 매니저.
/// 싱글턴으로 씬 간 유지되므로 API 요청의 기본 틀은 여기에 작성한다.
///
/// 추가 예정:
/// 1. 서버 주소 설정
/// 2. 로그인 / 회원가입 / 중복 체크 요청 함수 추가
/// 3. 서버 응답 파싱 후 DataManager에 저장
///
/// 주의:
/// - 모든 서버 통신 결과는 DataManager에 저장한다.
/// - NetworkManager는 통신 요청과 응답 처리만 담당한다.
/// </summary>
public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    [Header("Server Setting")]
    /// 서버 기본 주소
    [SerializeField] private string baseUrl = "http://localhost:8080";

    [SerializeField] private int timeout = 10;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // =========================================================
    // 공통 HTTP 헬퍼

    /// GET 요청 공통 처리
    private IEnumerator GetRoutine(string url, Action<long, string> onComplete)
    {
        using UnityWebRequest request = UnityWebRequest.Get(baseUrl + url);

        string token = PlayerPrefs.GetString("accessToken");
        if (!string.IsNullOrEmpty(token))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
        }

        request.timeout = timeout;
        yield return request.SendWebRequest();

        if (IsNetworkError(request))
        {
            onComplete?.Invoke(-1, null);
            yield break;
        }

        onComplete?.Invoke(request.responseCode, request.downloadHandler.text.Trim());
    }

    /// POST 요청 공통 처리
    private IEnumerator PostRoutine(string url, string json, Action<long, string> onComplete)
    {
        using UnityWebRequest request = new UnityWebRequest(baseUrl + url, "POST");

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        string token = PlayerPrefs.GetString("accessToken");
        if (!string.IsNullOrEmpty(token))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
        }

        request.timeout = timeout;
        yield return request.SendWebRequest();

        if (IsNetworkError(request))
        {
            onComplete?.Invoke(-1, null);
            yield break;
        }

        onComplete?.Invoke(request.responseCode, request.downloadHandler.text.Trim());
    }

    /// 네트워크/처리 오류 여부 확인
    private bool IsNetworkError(UnityWebRequest request)
    {
        return request.result == UnityWebRequest.Result.ConnectionError ||
               request.result == UnityWebRequest.Result.DataProcessingError;
    }

    /// JSON 파싱 시도 — 실패 시 null 반환
    private T TryParseJson<T>(string raw) where T : class
    {
        if (string.IsNullOrEmpty(raw) || !raw.StartsWith("{")) return null;
        try { return JsonUtility.FromJson<T>(raw); }
        catch (Exception e)
        {
            Debug.LogWarning("[NetworkManager] JSON 파싱 실패: " + e.Message + " / raw: " + raw);
            return null;
        }
    }

    // =========================================================
    // 1. 연결 테스트

    public void TestConnection() => StartCoroutine(GetRoutine("", (code, raw) =>
    {
        if (code == -1) Debug.LogError("[NetworkManager] 연결 실패");
        else Debug.Log("[NetworkManager] 연결 성공: " + raw);
    }));

    // =========================================================
    // 2. 로그인

    public void SendLoginRequest(string loginId, string password,
        Action onSuccess = null, Action<string> onFail = null)
    {
        StartCoroutine(PostRoutine("/auth/login",
            JsonUtility.ToJson(new LoginRequest { userId = loginId, password = password }),
            (code, raw) =>
            {
                Debug.Log("[NetworkManager] 로그인 응답 code: " + code + " / raw: " + raw);

                if (code == -1) { onFail?.Invoke("Server connection failed"); return; }
                // 404는 서버/URL 문제, 401·403은 인증 실패로 구분
                if (code == -1 || code == 404) { onFail?.Invoke("Server connection failed"); return; }
                if (code != 200)               { onFail?.Invoke("User not found"); return; }

                LoginResponse response = TryParseJson<LoginResponse>(raw);
                if (response == null) { onFail?.Invoke("Response parse error"); return; }

                // 🔥 ownedPets 확인 로그
                if (response.ownedPets != null)
                {
                    Debug.Log("[NetworkManager] 보유 펫 수: " + response.ownedPets.Length);

                    for (int i = 0; i < response.ownedPets.Length; i++)
                    {
                        Debug.Log($"ownedPet[{i}] petId: {response.ownedPets[i].petId}, petTypeId: {response.ownedPets[i].petTypeId}");
                    }
                }
                else
                {
                    Debug.Log("[NetworkManager] 보유 펫 없음");
                }

                // ✨ 파싱 결과 확인 로그
                Debug.Log("[NetworkManager] 파싱 결과");
                Debug.Log("  accessToken: " + response.accessToken);
                Debug.Log("  nickname: "    + response.nickname);
                Debug.Log("  shopName: "    + response.shopName);
                Debug.Log("  hasUnreadMail: " + response.hasUnreadMail);
                
                PlayerPrefs.SetString("accessToken", response.accessToken);
                DataManager.Data.SetUserSession(-1, loginId, response.nickname, response.shopName);
                DataManager.Data.SetUnreadMailState(response.hasUnreadMail);
                DataManager.Data.SetOwnedPets(response.ownedPets);
                onSuccess?.Invoke();
            }));
    }

    // =========================================================
    // 3. 아이디 중복 체크

    public void CheckUserIdDuplicate(string userId, Action<bool, string> onResult)
    {
        StartCoroutine(GetRoutine("/auth/check/" + userId, (code, raw) =>
        {
            if (code == -1) { onResult?.Invoke(false, "Server connection failed"); return; }

            bool isDuplicate = raw == "true";
            onResult?.Invoke(!isDuplicate, isDuplicate ? "ID already exists" : "ID is available");
        }));
    }

    // =========================================================
    // 4. 회원가입

    public void SendSignUpRequest(string loginId, string password, string nickname, string petShopName,
        Action<bool, string> onResult = null)
    {
        StartCoroutine(PostRoutine("/auth/register",
            JsonUtility.ToJson(new SignUpRequest
            {
                userId = loginId, password = password,
                nickname = nickname, shopName = petShopName
            }),
            (code, raw) =>
            {
                Debug.Log("[NetworkManager] 회원가입 응답 code: " + code + " / raw: " + raw);

                if (code == -1) { onResult?.Invoke(false, "Server connection failed"); return; }

                // 응답 body 없이 200/201로만 성공 처리하는 서버 대응
                if (string.IsNullOrEmpty(raw) || !raw.StartsWith("{"))
                {
                    onResult?.Invoke(code == 200 || code == 201, code == 200 || code == 201
                        ? "Sign up successful"
                        : "Unexpected response: " + code);
                    return;
                }

                ApiResponse response = TryParseJson<ApiResponse>(raw);
                if (response == null) { onResult?.Invoke(false, "Response parse error"); return; }

                if (response.success)
                    onResult?.Invoke(true, "Sign up successful");
                else if (response.error == "ID_ALREADY_EXISTS")
                    onResult?.Invoke(false, "ID already exists");
                else
                    onResult?.Invoke(false, response.error);
            }));
    }

    // =========================================================
    // 5. 추가 데이터 요청 — 추후 구현 예정

    public void RequestUserData(int userId)      => Debug.Log("[NetworkManager] 유저 데이터 요청 예정 / userId: " + userId);
    // 펫룸에 표시할 특정 펫 데이터 요청
    public void RequestPetData(int petId, Action<PetRoomResponse> onSuccess = null, Action<string> onFail = null)
    {
        StartCoroutine(GetRoutine("/pet/petroom?petId=" + petId, (code, raw) =>
        {
            Debug.Log("[NetworkManager] 펫 데이터 응답 code: " + code);
            Debug.Log("[NetworkManager] raw: " + raw);

            if (code == -1)
            {
                Debug.LogError("[NetworkManager] 서버 연결 실패");
                return;
            }

            if (code != 200)
            {
                Debug.LogError("[NetworkManager] 펫 데이터 요청 실패: " + code);
                return;
            }

            PetRoomResponse response = TryParseJson<PetRoomResponse>(raw);

            if (response == null)
            {
                Debug.LogError("[NetworkManager] 펫 데이터 JSON 파싱 실패");
                return;
            }

            if (!response.success)
            {
                Debug.LogError("[NetworkManager] 펫 데이터 요청 실패");
                return;
            }

            Debug.Log("[NetworkManager] 펫 데이터 요청 성공");
            Debug.Log("petId: " + response.data.pet.petId);
            Debug.Log("petTypeId: " + response.data.pet.petTypeId);
            Debug.Log("level: " + response.data.pet.level);
            Debug.Log("food: " + response.data.pet.food.current + " / " + response.data.pet.food.max);
            Debug.Log("water: " + response.data.pet.water.current + " / " + response.data.pet.water.max);
            Debug.Log("items count: " + response.data.items.Length);
            onSuccess?.Invoke(response);
        }));
    }

    // =========================================================
    // 6. S3 펫 획득 API 함수

    public void RequestAcquirePet(int petTypeId, Action onSuccess = null, Action<string> onFail = null)
    {
        string json = JsonUtility.ToJson(new AcquirePetRequest
        {
            petTypeId = petTypeId
        });

        StartCoroutine(PostRoutine("/pet/acquire", json, (code, raw) =>
        {
            Debug.Log("[NetworkManager] 펫 획득 응답 code: " + code + " / raw: " + raw);

            if (code == -1)
            {
                onFail?.Invoke("Server connection failed");
                return;
            }

            if (code != 200)
            {
                onFail?.Invoke("Pet acquire failed: " + code);
                return;
            }

            AcquirePetResponse response = TryParseJson<AcquirePetResponse>(raw);

            if (response == null)
            {
                onFail?.Invoke("Response parse error");
                return;
            }

            if (!response.success)
            {
                onFail?.Invoke(response.error);
                return;
            }

            Debug.Log("[NetworkManager] 펫 획득 성공");
            Debug.Log("petId: " + response.data.pet.petId);
            Debug.Log("petTypeId: " + response.data.pet.petTypeId);
            Debug.Log("level: " + response.data.pet.level);

            onSuccess?.Invoke();
        }));
    }

    public void RequestInventoryData(int userId) => Debug.Log("[NetworkManager] 인벤토리 데이터 요청 예정 / userId: " + userId);
    public void RequestLetterData(int userId)    => Debug.Log("[NetworkManager] 편지 데이터 요청 예정 / userId: " + userId);
}



    

// =========================================================
// 요청 / 응답 데이터 모델

[Serializable]
public class LoginRequest
{
    public string userId;
    public string password;
}

[Serializable]
public class LoginResponse
{
    public string accessToken;
    public string nickname;
    public string shopName;
    public bool hasUnreadMail;

    public OwnedPetData[] ownedPets;
}

[Serializable]
public class OwnedPetData
{
    public int petId;
    public int petTypeId;
}

[Serializable]
public class SignUpRequest
{
    public string userId;
    public string password;
    public string nickname;
    public string shopName;
}

[Serializable]
public class ApiResponse
{
    public bool success;
    public string error;
}