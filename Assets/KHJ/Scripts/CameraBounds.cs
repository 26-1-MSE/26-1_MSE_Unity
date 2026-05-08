using UnityEngine;

public class CameraBounds : MonoBehaviour
{
    private Camera cam;

    // 플레이어 Transform을 직접 참조
    [SerializeField] private Transform target;

    [SerializeField] private float MAP_MIN_X = -84.66f;
    [SerializeField] private float MAP_MAX_X = 10.25f;
    [SerializeField] private float MAP_MIN_Y = -5.28f;
    [SerializeField] private float MAP_MAX_Y = 16.36f;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        float halfHeight = cam.orthographicSize;
        float halfWidth  = cam.orthographicSize * cam.aspect;

        // 플레이어 위치를 기준으로 클램프
        float clampedX = Mathf.Clamp(target.position.x, MAP_MIN_X + halfWidth,  MAP_MAX_X - halfWidth);
        float clampedY = Mathf.Clamp(target.position.y, MAP_MIN_Y + halfHeight, MAP_MAX_Y - halfHeight);

        // 월드 좌표로 직접 설정
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}