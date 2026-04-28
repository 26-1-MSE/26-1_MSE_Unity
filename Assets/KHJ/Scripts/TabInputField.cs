using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TabInputField : MonoBehaviour
{
    [SerializeField] private TMP_InputField[] inputFields;

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            for (int i = 0; i < inputFields.Length; i++)
            {
                if (inputFields[i].isFocused)
                {
                    int next = (i + 1) % inputFields.Length;
                    inputFields[next].Select();
                    inputFields[next].ActivateInputField();
                    break;
                }
            }
        }
    }
}