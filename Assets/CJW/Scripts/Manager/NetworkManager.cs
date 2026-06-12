using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Singleton manager responsible for all client-server communication.
/// Handles HTTP requests, authentication, response parsing,
/// and synchronization with DataManager.
/// </summary>

public class NetworkManager : MonoBehaviour
{
    // Global singleton instance used throughout the client.
    public static NetworkManager Instance { get; private set; }

    [Header("Server Setting")]
    /// Base URL of the backend server.
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

    /// <summary>
    /// Sends a GET request to the specified endpoint.
    /// Automatically attaches the access token if available.
    /// </summary>
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

    /// <summary>
    /// Sends a POST request with a JSON body.
    /// Automatically attaches authentication headers when needed.
    /// </summary>
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

    /// <summary>
    /// Checks whether the request failed due to a connection
    /// or data processing error.
    /// </summary>
    private bool IsNetworkError(UnityWebRequest request)
    {
        return request.result == UnityWebRequest.Result.ConnectionError ||
               request.result == UnityWebRequest.Result.DataProcessingError;
    }

    /// <summary>
    /// Attempts to deserialize a JSON response into the specified type.
    /// Returns null if parsing fails.
    /// </summary>
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

    private string GetUserMessageFromError(string error)
    {
        switch (error)
        {
            case "PET_LIMIT_EXCEEDED":
                return "You cannot bring any more pets.";

            default:
                return string.IsNullOrEmpty(error) ? "Unknown error" : error;
        }
    }

    public void TestConnection() => StartCoroutine(GetRoutine("", (code, raw) =>
    {
        if (code == -1) Debug.LogError("[NetworkManager] 연결 실패");
        else Debug.Log("[NetworkManager] 연결 성공: " + raw);
    }));

    /// <summary>
    /// Sends a login request and stores the returned user session,
    /// pet information, and authentication token.
    /// </summary>
    public void SendLoginRequest(string loginId, string password,
        Action onSuccess = null, Action<string> onFail = null)
    {
        StartCoroutine(PostRoutine("/auth/login",
            JsonUtility.ToJson(new LoginRequest { userId = loginId, password = password }),
            (code, raw) =>
            {
                Debug.Log("[NetworkManager] 로그인 응답 code: " + code + " / raw: " + raw);

                if (code == -1) { onFail?.Invoke("Server connection failed"); return; }

                if (code == 404) { onFail?.Invoke("Server connection failed"); return; }
                if (code != 200)               { onFail?.Invoke("User not found"); return; }

                LoginResponse response = TryParseJson<LoginResponse>(raw);
                if (response == null) { onFail?.Invoke("Response parse error"); return; }

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

    /// <summary>
    /// Checks whether the specified user ID already exists.
    /// </summary>
    
    public void CheckUserIdDuplicate(string userId, Action<bool, string> onResult)
    {
        StartCoroutine(GetRoutine("/auth/check/" + userId, (code, raw) =>
        {
            if (code == -1) { onResult?.Invoke(false, "Server connection failed"); return; }

            bool isDuplicate = raw == "true";
            onResult?.Invoke(!isDuplicate, isDuplicate ? "ID already exists" : "ID is available");
        }));
    }

  
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

    /// <summary>
    /// Retrieves the latest authenticated user information.
    /// Updates nickname, shop name, mail state, and owned pets.
    /// </summary>

    public void RequestAuthStatus(
    Action<LoginResponse> onSuccess = null,
    Action<string> onFail = null)
    {
        Debug.Log("[NetworkManager] auth/status 요청 시작");

        StartCoroutine(GetRoutine("/auth/status", (code, raw) =>
        {
            Debug.Log("[NetworkManager] auth/status 응답 code: " + code);
            Debug.Log("[NetworkManager] auth/status raw: " + raw);

            if (code == -1)
            {
                onFail?.Invoke("Server connection failed");
                return;
            }

            if (code != 200)
            {
                onFail?.Invoke("auth/status failed: " + code);
                return;
            }

            LoginResponse response = TryParseJson<LoginResponse>(raw);

            if (response == null)
            {
                onFail?.Invoke("Response parse error");
                return;
            }

            if (DataManager.Data != null)
            {
                DataManager.Data.SetUserSession(-1, "", response.nickname, response.shopName);
                DataManager.Data.SetUnreadMailState(response.hasUnreadMail);
                DataManager.Data.SetOwnedPets(response.ownedPets);
            }

            Debug.Log("[NetworkManager] auth/status DataManager 저장 완료");

            onSuccess?.Invoke(response);
        }));
    }

    /// <summary>
    /// Requests detailed information about a specific pet
    /// displayed in the Pet Room scene.
    /// </summary>
    
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
            if (DataManager.Data != null && response.data.items != null)
            {
                Debug.Log("[NetworkManager] 펫룸 items DataManager 저장 호출");
                DataManager.Data.SetOwnedItems(response.data.items);
            }
            else
            {
                Debug.LogWarning("[NetworkManager] DataManager 또는 items null");
            }
            
            onSuccess?.Invoke(response);
        }));
    }

    /// <summary>
    /// Sends a pet acquisition request after a successful mini-game.
    /// Updates owned pet data on success.
    /// </summary>
    
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
                onFail?.Invoke(GetUserMessageFromError(response.error));
                return;
            }

            Debug.Log("[NetworkManager] 펫 획득 성공");
            Debug.Log("petId: " + response.data.pet.petId);
            Debug.Log("petTypeId: " + response.data.pet.petTypeId);
            Debug.Log("level: " + response.data.pet.level);

            if (DataManager.Data != null)
            {
                DataManager.Data.AddOwnedPet(
                    response.data.pet.petId,
                    response.data.pet.petTypeId
                );
            }
            else
            {
                Debug.LogWarning("[NetworkManager] DataManager.Data가 없어서 보유 펫을 갱신할 수 없습니다.");
            }

            onSuccess?.Invoke();
        }));
    }

    /// <summary>
    /// Retrieves the latest inventory data including
    /// owned pets and items.
    /// </summary>
    
    public void RequestInventoryData(Action<InventoryResponse> onSuccess = null, Action<string> onFail = null)
    {
        StartCoroutine(GetRoutine("/inventory", (code, raw) =>
        {
            Debug.Log("[NetworkManager] 인벤토리 응답 code: " + code);
            Debug.Log("[NetworkManager] raw: " + raw);

            if (code == -1)
            {
                onFail?.Invoke("Server connection failed");
                return;
            }

            if (code != 200)
            {
                onFail?.Invoke("Inventory request failed: " + code);
                return;
            }

            InventoryResponse response = TryParseJson<InventoryResponse>(raw);

            if (response == null)
            {
                onFail?.Invoke("Response parse error");
                return;
            }

            if (!response.success)
            {
                onFail?.Invoke("Inventory request failed");
                return;
            }

            if (DataManager.Data != null)
            {
                OwnedPetData[] ownedPets = new OwnedPetData[response.data.pets.Length];

                for (int i = 0; i < response.data.pets.Length; i++)
                {
                    ownedPets[i] = new OwnedPetData
                    {
                        petId = response.data.pets[i].petId,
                        petTypeId = response.data.pets[i].petTypeId,
                        level = response.data.pets[i].level
                    };
                }

                DataManager.Data.SetOwnedPets(ownedPets);
                DataManager.Data.SetOwnedItems(response.data.items);
            }

            onSuccess?.Invoke(response);
        }));
    }

    /// <summary>
    /// Sends an item usage request for a specific pet.
    /// The updated pet status is returned from the server.
    /// </summary>
    
    public void RequestUseItem(
    int petId,
    int itemTypeId,
    Action<UseItemResponse> onSuccess = null,
    Action<string> onFail = null)
    {
        UseItemRequest request = new UseItemRequest
        {
            petId = petId,
            itemTypeId = itemTypeId
        };

        string json = JsonUtility.ToJson(request);

        Debug.Log("[NetworkManager] item/use json: " + json);

        StartCoroutine(PostRoutine("/item/use", json, (code, raw) =>
        {
            Debug.Log("[NetworkManager] 아이템 사용 응답 code: " + code);
            Debug.Log("[NetworkManager] raw: " + raw);

            if (code == -1)
            {
                onFail?.Invoke("서버 연결 실패");
                return;
            }

            if (code != 200)
            {
                onFail?.Invoke("아이템 사용 실패: " + code);
                return;
            }

            UseItemResponse response = TryParseJson<UseItemResponse>(raw);

            if (response == null)
            {
                onFail?.Invoke("JSON 파싱 실패");
                return;
            }

            if (!response.success)
            {
                onFail?.Invoke(response.error);
                return;
            }

            onSuccess?.Invoke(response);
        }));
    }

    /// <summary>
    /// Sends an item acquisition request when the player
    /// collects resources from the Island scene.
    /// </summary>
    
    public void RequestAcquireItem(int itemTypeId, int count, Action onSuccess = null, Action<string> onFail = null)
    {
        string json = JsonUtility.ToJson(new AcquireItemRequest
        {
            itemTypeId = itemTypeId,
            count = count
        });

        StartCoroutine(PostRoutine("/item/acquire", json, (code, raw) =>
        {
            Debug.Log("[NetworkManager] 아이템 획득 응답 code: " + code + " / raw: " + raw);

            if (code == -1)
            {
                onFail?.Invoke("Server connection failed");
                return;
            }

            if (code != 200)
            {
                onFail?.Invoke("Item acquire failed: " + code);
                return;
            }

            AcquireItemResponse response = TryParseJson<AcquireItemResponse>(raw);

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

            Debug.Log("[NetworkManager] 아이템 획득 성공");
            Debug.Log("itemId: " + response.data.item.itemId);
            Debug.Log("itemTypeId: " + response.data.item.itemTypeId);
            Debug.Log("count: " + response.data.item.count);

            if (DataManager.Data != null)
            {
                DataManager.Data.AddOwnedItem(
                    response.data.item.itemId,
                    response.data.item.itemTypeId,
                    response.data.item.count
                );
            }

            onSuccess?.Invoke();
        }));
    }

    /// <summary>
    /// Requests the player's mailbox list.
    /// </summary>
    
    public void RequestMailList(
        Action<MailListResponse> onSuccess = null,
        Action<string> onFail = null)
    {
        StartCoroutine(GetRoutine("/mail/list", (code, raw) =>
        {
            Debug.Log("[NetworkManager] 메일 목록 응답 code: " + code);
            Debug.Log("[NetworkManager] raw: " + raw);

            if (code == -1)
            {
                onFail?.Invoke("서버 연결 실패");
                return;
            }

            if (code != 200)
            {
                onFail?.Invoke("메일 목록 조회 실패: " + code);
                return;
            }

            MailListResponse response = TryParseJson<MailListResponse>(raw);

            if (response == null)
            {
                onFail?.Invoke("메일 목록 JSON 파싱 실패");
                return;
            }

            if (!response.success)
            {
                onFail?.Invoke(response.error);
                return;
            }

            onSuccess?.Invoke(response);
        }));
    }

    /// <summary>
    /// Requests detailed information for a specific mail.
    /// </summary>
    public void RequestMailDetail(
    int mailId,
    Action<MailDetailResponse> onSuccess = null,
    Action<string> onFail = null)
    {
        StartCoroutine(GetRoutine("/mail/" + mailId, (code, raw) =>
        {
            Debug.Log("[NetworkManager] 메일 상세 응답 code: " + code);
            Debug.Log("[NetworkManager] raw: " + raw);

            if (code == -1)
            {
                onFail?.Invoke("서버 연결 실패");
                return;
            }

            if (code != 200)
            {
                onFail?.Invoke("메일 상세 조회 실패: " + code);
                return;
            }

            MailDetailResponse response = TryParseJson<MailDetailResponse>(raw);

            if (response == null)
            {
                onFail?.Invoke("메일 상세 JSON 파싱 실패");
                return;
            }

            if (!response.success)
            {
                onFail?.Invoke(response.error);
                return;
            }

            onSuccess?.Invoke(response);
        }));
    }
}

// =========================================================
// Request / Response DTOs

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
    public int level;
}

[Serializable]
public class OwnedItemData
{
    public int itemId;
    public int itemTypeId;
    public int count;
}

[Serializable]
public class UseItemRequest
{
    public int petId;
    public int itemTypeId;
}

[Serializable]
public class UseItemResponse
{
    public bool success;
    public string error;
    public UseItemData data;
}

[Serializable]
public class UseItemData
{
    public bool success;
    public PetData pet;
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
public class MailListResponse
{
    public bool success;
    public string error;
    public MailListData data;
}

[Serializable]
public class MailListData
{
    public MailSummaryData[] mails;
}

[Serializable]
public class MailSummaryData
{
    public int mailId;
    public string title;
    public string sender;
    public bool isRead;
    public string createdAt;
}

[Serializable]
public class MailDetailResponse
{
    public bool success;
    public string error;
    public MailDetailData data;
}

[Serializable]
public class MailDetailData
{
    public int mailId;
    public string title;
    public string nickname;
    public string sender;
    public string content;
    public bool isRead;
    public string createdAt;
}

[Serializable]
public class ApiResponse
{
    public bool success;
    public string error;
}