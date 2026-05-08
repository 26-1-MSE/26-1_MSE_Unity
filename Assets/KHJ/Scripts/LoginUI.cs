using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Handles the login form UI, validates input fields, and delegates
/// authentication to NetworkManager.
/// </summary>
public class LoginUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField idInputField;
    [SerializeField] private TMP_InputField passwordInputField;

    [SerializeField] private ToastMessage toastMessage;
    [SerializeField] private UnityEvent onLoginSuccess;
    [SerializeField] private Button loginButton;

    // Bound to the login button's OnClick event
    public void OnClickLoginButton()
    {
        string id = idInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrWhiteSpace(id) ||
            string.IsNullOrWhiteSpace(password))
        {
            toastMessage.ShowToast("Fields are not fully filled in");
            return;
        }

        // Disable while the request is in flight to prevent duplicate submissions
        loginButton.interactable = false;

        NetworkManager.Instance.SendLoginRequest(
            id, password,
            onSuccess: () =>
            {
                loginButton.interactable = true;
                onLoginSuccess?.Invoke();
            },
            onFail: (msg) =>
            {
                loginButton.interactable = true;
                toastMessage.ShowToast(msg);
            }
        );
    }
}
