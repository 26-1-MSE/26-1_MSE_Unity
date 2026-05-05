using UnityEngine;

public class LobbyUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void GoToPetTown()
    {
        GameManager.Instance.GoToPetTown();
    }
}
