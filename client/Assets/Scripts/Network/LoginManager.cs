using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace EmberBlade.Network
{
    public class LoginManager : MonoBehaviour
    {
        [SerializeField] private string loginUrl = "http://localhost:8080/api/auth/login";

        public string Token { get; private set; }

        public void Login(string username, string password, Action<string> onSuccess, Action<string> onFailure)
        {
            StartCoroutine(LoginRoutine(username, password, onSuccess, onFailure));
        }

        private IEnumerator LoginRoutine(
            string username,
            string password,
            Action<string> onSuccess,
            Action<string> onFailure)
        {
            string json = JsonUtility.ToJson(new LoginRequest(username, password));
            using UnityWebRequest request = new UnityWebRequest(loginUrl, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onFailure?.Invoke(request.error);
                yield break;
            }

            LoginResponse response;
            try
            {
                response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
            }
            catch (Exception exception)
            {
                onFailure?.Invoke($"Invalid login response: {exception.Message}");
                yield break;
            }

            if (string.IsNullOrWhiteSpace(response.token))
            {
                onFailure?.Invoke("Login response did not contain a token.");
                yield break;
            }

            Token = response.token;
            onSuccess?.Invoke(Token);
        }

        [Serializable]
        private class LoginRequest
        {
            public string username;
            public string password;

            public LoginRequest(string username, string password)
            {
                this.username = username;
                this.password = password;
            }
        }

        [Serializable]
        private class LoginResponse
        {
            public string token;
        }
    }
}
