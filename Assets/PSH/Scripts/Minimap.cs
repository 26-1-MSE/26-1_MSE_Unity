using UnityEngine;
using UnityEngine.UI;

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