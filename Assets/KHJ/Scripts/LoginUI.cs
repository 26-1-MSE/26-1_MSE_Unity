using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField idInputField;
    [SerializeField] private TMP_InputField passwordInputField;

    [SerializeField] private ToastMessage toastMessage;
    [SerializeField] private UnityEvent onLoginSuccess;
    [SerializeField] private Button loginButton;

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