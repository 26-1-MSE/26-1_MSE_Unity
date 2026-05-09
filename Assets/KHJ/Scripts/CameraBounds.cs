using UnityEngine;

/// <summary>
/// Constrains the camera's position within map bounds, following a target
/// while keeping the viewport inside the defined rectangle.
/// </summary>
public class CameraBounds : MonoBehaviour
{
    private Camera cam;

    // Direct reference to the player Transform the camera follows
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

        // Clamp so the viewport edges never exceed the map boundaries
        float clampedX = Mathf.Clamp(target.position.x, MAP_MIN_X + halfWidth,  MAP_MAX_X - halfWidth);
        float clampedY = Mathf.Clamp(target.position.y, MAP_MIN_Y + halfHeight, MAP_MAX_Y - halfHeight);

        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}
