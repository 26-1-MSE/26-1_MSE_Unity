using UnityEngine;

/// <summary>
/// Makes the minimap camera follow the player's position directly.
/// </summary>

public class Minimap : MonoBehaviour
{
    public Transform player;
    public float zOffset = -10f;

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 pos = player.position;
        transform.position = new Vector3(pos.x, pos.y, zOffset);
    }
    }