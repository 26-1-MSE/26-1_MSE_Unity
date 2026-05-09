using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Enables Tab-key navigation through a sequence of TMP_InputFields,
/// cycling focus to the next field in order.
/// </summary>
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
                    // Wrap around to the first field after the last one
                    int next = (i + 1) % inputFields.Length;
                    inputFields[next].Select();
                    inputFields[next].ActivateInputField();
                    break;
                }
            }
        }
    }
}
