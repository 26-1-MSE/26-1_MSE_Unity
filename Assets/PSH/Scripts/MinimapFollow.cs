using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player;

    public float zOffset = -10f;
    public float yOffset = 2f;

    [Header("Camera Limit")]
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    void LateUpdate()
    {
        if (player == null) return;

        float targetX = player.position.x;
        float targetY = player.position.y + yOffset;

        targetX = Mathf.Clamp(targetX, minX, maxX);
        targetY = Mathf.Clamp(targetY, minY, maxY);

        transform.position = new Vector3(targetX, targetY, zOffset);
    }
}