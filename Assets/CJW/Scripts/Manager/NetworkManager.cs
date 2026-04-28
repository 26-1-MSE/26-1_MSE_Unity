using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// ������ Unity Ŭ���̾�Ʈ ������ ����� ����ϴ� �Ŵ���.
/// ����� ���� �ϼ� ���̹Ƿ� API ������ ���� �⺻ ������ �ۼ��� �д�.
/// 
/// ��� ����:
/// 1. ���� �ּ� ����
/// 2. �α��� / ȸ������ / ���� ������ ��û �Լ� ����
/// 3. ���� ������ ���� �� DataManager�� ����
/// 
/// ����:
/// - ���� ���� ������ ������ DataManager�� ����Ѵ�.
/// - NetworkManager�� ���� ��û�� ���� ó���� ����Ѵ�.
/// </summary>
public class NetworkManager : MonoBehaviour
{
    /// ���� ���ٿ� �̱��� �ν��Ͻ�
    public static NetworkManager Instance { get; private set; }

    [Header("Server Setting")]
    /// ���� �⺻ �ּ�
    /// ���� ����ڰ� ���� IP�� ��Ʈ�� �˷��ָ� �����Ѵ�.
    /// ��: http://192.168.0.15:8080
    [SerializeField]
    private string baseUrl = "http://localhost:8080";

    /// ���� ��û ���� �ð�
    [SerializeField] private int timeout = 10;

    private void Awake()
    {
        // �̹� NetworkManager�� �����ϸ� �ߺ� ���� ����
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // �̱��� ���
        Instance = this;

        // ���� �ٲ� ����
        DontDestroyOnLoad(gameObject);
    }

    // =========================================================
    // 1. ���� ���� �׽�Ʈ

    /// ������ ���� �ִ��� Ȯ���ϴ� �׽�Ʈ�� �Լ�
    public void TestConnection()
    {
        StartCoroutine(TestConnectionRoutine());
    }

    private IEnumerator TestConnectionRoutine()
    {
        Debug.Log("[NetworkManager] ���� ���� �׽�Ʈ ����: " + baseUrl);

        using UnityWebRequest request = UnityWebRequest.Get(baseUrl);
        request.timeout = timeout;

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[NetworkManager] ���� ���� ����: " + request.error);
        }
        else
        {
            Debug.Log("[NetworkManager] ���� ���� ����: " + request.downloadHandler.text);
        }
    }

    // =========================================================
    // 2. �α��� ��û

    /// �α��� ��û �Լ�
    /// ���� API�� �ϼ��Ǹ� LoginRoutine ���� URL�� JSON ������ ���� �ڵ忡 �����.
    public void SendLoginRequest(string loginId, string password, Action onSuccess = null, Action<string> onFail = null)
    {
        StartCoroutine(LoginRoutine(loginId, password, onSuccess, onFail));
    }

    private IEnumerator LoginRoutine(string loginId, string password, Action onSuccess, Action<string> onFail)
    {
        string url = baseUrl + "/auth/login";

        LoginRequest requestData = new LoginRequest
        {
            userId = loginId, // 서버 필드명 맞춤 (기존 id → userId)
            password = password
        };

        string json = JsonUtility.ToJson(requestData);

        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = timeout;

        yield return request.SendWebRequest();

        // 서버가 예외를 throw하면 200이 아닌 코드로 옴
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.DataProcessingError)
        {
            onFail?.Invoke("Server connection failed");  // 서버 꺼짐 / URL 오류
            yield break;
        }

        if (request.responseCode != 200)
        {
            onFail?.Invoke("User not found");
            yield break;
        }

        LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
        PlayerPrefs.SetString("accessToken", response.accessToken);
        DataManager.Data.SetUserSession(-1, loginId, response.nickname, response.shopName);

        onSuccess?.Invoke();
    }

    // =========================================================
// 3. 아이디 중복 체크

    public void CheckUserIdDuplicate(string userId, Action<bool, string> onResult)
    {
        StartCoroutine(CheckUserIdRoutine(userId, onResult));
    }

    private IEnumerator CheckUserIdRoutine(string userId, Action<bool, string> onResult)
    {
        string url = baseUrl + "/auth/check/" + userId;

        using UnityWebRequest request = UnityWebRequest.Get(url);
        request.timeout = timeout;

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.DataProcessingError)
        {
            onResult?.Invoke(false, "Server connection failed");
            yield break;
        }

        // 서버가 true 반환 → 이미 존재, false → 사용 가능
        bool isDuplicate = request.downloadHandler.text.Trim() == "true";

        if (isDuplicate)
            onResult?.Invoke(false, "ID already exists");
        else
            onResult?.Invoke(true, "ID is available");
    }

    // =========================================================
    // 4. 회원가입 요청

    public void SendSignUpRequest(string loginId, string password, string nickname, string petShopName,
        Action<bool, string> onResult = null)
    {
        StartCoroutine(SignUpRoutine(loginId, password, nickname, petShopName, onResult));
    }

    private IEnumerator SignUpRoutine(string loginId, string password, string nickname, string petShopName,
        Action<bool, string> onResult)
    {
        string url = baseUrl + "/auth/register";

        SignUpRequest requestData = new SignUpRequest
        {
            userId = loginId,
            password = password,
            nickname = nickname,
            shopName = petShopName
        };

        string json = JsonUtility.ToJson(requestData);

        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = timeout;

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.DataProcessingError)
        {
            onResult?.Invoke(false, "Server connection failed");
            yield break;
        }

        ApiResponse response = JsonUtility.FromJson<ApiResponse>(request.downloadHandler.text);

        if (response.success)
            onResult?.Invoke(true, "Sign up successful");
        else if (response.error == "ID_ALREADY_EXISTS")
            onResult?.Invoke(false, "ID already exists");
        else
            onResult?.Invoke(false, response.error);
    }

    // =========================================================
    // 5. ���� ������ ��û - ���� �ϼ� �� ����

    public void RequestUserData(int userId)
    {
        Debug.Log("[NetworkManager] ���� ������ ��û ���� / userId: " + userId);
    }

    public void RequestPetData(int userId)
    {
        Debug.Log("[NetworkManager] �� ������ ��û ���� / userId: " + userId);
    }

    public void RequestInventoryData(int userId)
    {
        Debug.Log("[NetworkManager] �κ��丮 ������ ��û ���� / userId: " + userId);
    }

    public void RequestLetterData(int userId)
    {
        Debug.Log("[NetworkManager] ���� ������ ��û ���� / userId: " + userId);
    }
}

// =========================================================
// ���� ��û / ���� ������ ����

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