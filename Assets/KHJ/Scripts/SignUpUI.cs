using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Handles the sign-up form UI, validates all input fields including password
/// confirmation, and delegates registration to NetworkManager.
/// </summary>
public class SignUpUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField idInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField verifyPasswordInputField;
    [SerializeField] private TMP_InputField nicknameInputField;
    [SerializeField] private TMP_InputField shopNameInputField;

    [SerializeField] private ToastMessage toastMessage;

    [SerializeField] private Button checkIdButton;
    [SerializeField] private Button signUpButton;

    [SerializeField] private UnityEvent onSignUpSuccess;

    // Bound to the ID duplicate check button
    public void OnClickCheckIdButton()
    {
        string id = idInputField.text;

        if (string.IsNullOrWhiteSpace(id))
        {
            toastMessage.ShowToast("Please enter an ID");
            return;
        }

        checkIdButton.interactable = false;

        NetworkManager.Instance.CheckUserIdDuplicate(id, (success, msg) =>
        {
            checkIdButton.interactable = true;
            toastMessage.ShowToast(msg);
        });
    }

    // Bound to the sign-up submit button
    public void OnClickSignUpButton()
    {
        string id = idInputField.text;
        string password = passwordInputField.text;
        string verifyPassword = verifyPasswordInputField.text;
        string nickname = nicknameInputField.text;
        string shopName = shopNameInputField.text;

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(verifyPassword) || string.IsNullOrWhiteSpace(nickname) ||
            string.IsNullOrWhiteSpace(shopName))
        {
            toastMessage.ShowToast("Fields are not fully filled in");
            return;
        }

        if (password != verifyPassword)
        {
            toastMessage.ShowToast("Password mismatch");
            return;
        }

        // Disable while the request is in flight to prevent duplicate submissions
        signUpButton.interactable = false;

        NetworkManager.Instance.SendSignUpRequest(id, password, nickname, shopName, (success, msg) =>
        {
            signUpButton.interactable = true;
            toastMessage.ShowToast(msg);
            onSignUpSuccess?.Invoke();
        });
    }
}
