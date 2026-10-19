using EmberBlade.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EmberBlade.UI
{
    public class LoginUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button loginButton;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private LoginManager loginManager;
        [SerializeField] private NetworkManager networkManager;

        private void OnEnable()
        {
            loginButton.onClick.AddListener(Login);
        }

        private void OnDisable()
        {
            loginButton.onClick.RemoveListener(Login);
        }

        private void Login()
        {
            statusText.text = "Logging in...";
            loginButton.interactable = false;
            loginManager.Login(usernameInput.text, passwordInput.text, HandleLoginSuccess, HandleLoginFailure);
        }

        private void HandleLoginSuccess(string token)
        {
            statusText.text = "Connected.";
            loginPanel.SetActive(false);
            networkManager.Connect(token);
        }

        private void HandleLoginFailure(string error)
        {
            statusText.text = $"Login failed: {error}";
            loginButton.interactable = true;
        }
    }
}
