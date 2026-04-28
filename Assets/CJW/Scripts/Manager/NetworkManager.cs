using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 서버와 Unity 클라이언트 사이의 통신을 담당하는 매니저.
/// 현재는 서버 완성 전이므로 API 연결을 위한 기본 구조만 작성해 둔다.
/// 
/// 담당 역할:
/// 1. 서버 주소 관리
/// 2. 로그인 / 회원가입 / 유저 데이터 요청 함수 제공
/// 3. 서버 응답을 받은 뒤 DataManager에 전달
/// 
/// 주의:
/// - 실제 유저 데이터 저장은 DataManager가 담당한다.
/// - NetworkManager는 서버 요청과 응답 처리만 담당한다.
/// </summary>

public class NetworkManager : MonoBehaviour
{
    /// 전역 접근용 싱글톤 인스턴스
    public static NetworkManager Instance { get; private set; }

    [Header("Server Setting")]

    /// 서버 기본 주소
    /// 서버 담당자가 서버 IP와 포트를 알려주면 수정한다.
    /// 예: http://192.168.0.15:3000
    [SerializeField] private string baseUrl = "http://서버IP:포트";

    /// 서버 요청 제한 시간
    [SerializeField] private int timeout = 10;

    private void Awake()
    {
        // 이미 NetworkManager가 존재하면 중복 생성 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 싱글톤 등록
        Instance = this;

        // 씬이 바뀌어도 유지
        DontDestroyOnLoad(gameObject);
    }

    // =========================================================
    // 1. 서버 연결 테스트

    /// 서버가 켜져 있는지 확인하는 테스트용 함수
    public void TestConnection()
    {
        StartCoroutine(TestConnectionRoutine());
    }

    private IEnumerator TestConnectionRoutine()
    {
        Debug.Log("[NetworkManager] 서버 연결 테스트 시작: " + baseUrl);

        using UnityWebRequest request = UnityWebRequest.Get(baseUrl);
        request.timeout = timeout;

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[NetworkManager] 서버 연결 실패: " + request.error);
        }
        else
        {
            Debug.Log("[NetworkManager] 서버 연결 성공: " + request.downloadHandler.text);
        }
    }

    // =========================================================
    // 2. 로그인 요청

    /// 로그인 요청 함수
    /// 서버 API가 완성되면 LoginRoutine 내부 URL과 JSON 형식을 서버 코드에 맞춘다.
    public void SendLoginRequest(string loginId, string password)
    {
        StartCoroutine(LoginRoutine(loginId, password));
    }

    private IEnumerator LoginRoutine(string loginId, string password)
    {
        string url = baseUrl + "/login";

        LoginRequest requestData = new LoginRequest
        {
            id = loginId,
            password = password
        };

        string json = JsonUtility.ToJson(requestData);

        Debug.Log("[NetworkManager] 로그인 요청 JSON: " + json);

        using UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = timeout;

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("[NetworkManager] 로그인 요청 실패: " + request.error);
            yield break;
        }

        Debug.Log("[NetworkManager] 로그인 응답: " + request.downloadHandler.text);

        LoginResponse response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);

        if (response.success)
        {
            PlayerPrefs.SetString("accessToken", response.accessToken);
            PlayerPrefs.SetString("refreshToken", response.refreshToken);

            DataManager.Data.SetUserSession(
                -1,
                loginId,
                response.nickname,
                response.shopName
            );

            DataManager.Data.SetUnreadMailState(response.hasUnreadMail);

            Debug.Log("[NetworkManager] 로그인 성공 / 토큰 및 유저 정보 저장 완료");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToPetTown();
            }
        }
        else
        {
            Debug.LogWarning("[NetworkManager] 로그인 실패: " + response.error);
        }
    }

    // =========================================================
    // 3. 회원가입 요청 - 서버 완성 후 구현

    public void SendSignUpRequest(string loginId, string password, string nickname, string petShopName)
    {
        Debug.Log("[NetworkManager] 회원가입 요청 예정");
        // StartCoroutine(SignUpRoutine(...));
    }

    // =========================================================
    // 4. 유저 데이터 요청 - 서버 완성 후 구현

    public void RequestUserData(int userId)
    {
        Debug.Log("[NetworkManager] 유저 데이터 요청 예정 / userId: " + userId);
    }

    public void RequestPetData(int userId)
    {
        Debug.Log("[NetworkManager] 펫 데이터 요청 예정 / userId: " + userId);
    }

    public void RequestInventoryData(int userId)
    {
        Debug.Log("[NetworkManager] 인벤토리 데이터 요청 예정 / userId: " + userId);
    }

    public void RequestLetterData(int userId)
    {
        Debug.Log("[NetworkManager] 쪽지 데이터 요청 예정 / userId: " + userId);
    }
}

// =========================================================
// 서버 요청 / 응답 데이터 형식

[Serializable]
public class LoginRequest
{
    public string id;
    public string password;
}

[Serializable]
public class LoginResponse
{
    public string accessToken;
    public string refreshToken;
    public int expiresIn;

    public string nickname;
    public string shopName;
    public bool hasUnreadMail;

    public bool success;
    public string error;
}