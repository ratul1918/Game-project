using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace TheLightHouse
{
    public sealed class AuthClient : MonoBehaviour
    {
        [Serializable] sealed class Credentials { public string username; public string password; }
        [Serializable] sealed class TokenResponse { public string token; }
        public string Token { get; private set; }
        public string Username { get; private set; }
        public void Authenticate(NetworkConfig config, Action<string> completed) => StartCoroutine(AuthenticateRoutine(config, completed));
        public void Login(NetworkConfig config, string username, string password, Action<bool> completed) =>
            StartCoroutine(Send(config.BaseUrl + "/auth/login", username, password, token => SetToken(username, token, completed)));
        public void Register(NetworkConfig config, string username, string password, Action<bool> completed) =>
            StartCoroutine(Send(config.BaseUrl + "/auth/register", username, password, token => SetToken(username, token, completed)));
        IEnumerator AuthenticateRoutine(NetworkConfig config, Action<string> completed)
        {
            if (config.AutoRegisterDevUser) yield return Send(config.BaseUrl + "/auth/register", config.DevUsername, config.DevPassword, null);
            string token = null;
            yield return Send(config.BaseUrl + "/auth/login", config.DevUsername, config.DevPassword, value => token = value);
            SetToken(config.DevUsername, token, null); completed?.Invoke(token);
        }
        void SetToken(string username, string token, Action<bool> completed)
        {
            Token = token; Username = token == null ? null : username;
            if (token != null) PlayerPrefs.SetString("lighthouse.username", username);
            completed?.Invoke(token != null);
        }
        IEnumerator Send(string url, string username, string password, Action<string> tokenReceived)
        {
            string body = JsonUtility.ToJson(new Credentials { username = username, password = password });
            using var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
                tokenReceived?.Invoke(JsonUtility.FromJson<TokenResponse>(request.downloadHandler.text)?.token);
        }
    }
}
