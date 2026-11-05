using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace TheLightHouse
{
    [Serializable] public sealed class CloudSavePayload
    {
        public string act;
        public string checkpoint;
        public string itemsJson;
        public string pagesJson;
        public string flagsJson;
        public string optionsJson;
        public int version;
        public string updatedAt;
    }

    public sealed class CloudSaveClient : MonoBehaviour
    {
        [SerializeField, Min(.1f)] float timeoutSeconds = 10f;
        NetworkConfig config;
        AuthClient auth;
        public void Configure(NetworkConfig value, AuthClient client) { config = value; auth = client; }
        public void GetSave(Action<CloudSavePayload, string> completed) => StartCoroutine(Request("/save", UnityWebRequest.kHttpVerbGET, null, completed));
        public void PutSave(CloudSavePayload payload, Action<CloudSavePayload, string> completed) =>
            StartCoroutine(Request("/save", UnityWebRequest.kHttpVerbPUT, JsonUtility.ToJson(payload), completed));
        public void StartRun(Action<string> completed) => StartCoroutine(RunRequest("/runs/start", null, value => completed?.Invoke(value)));
        public void EndRun(string id, string outcome, int deaths, int duration) {
            if (string.IsNullOrEmpty(id)) return;
            StartCoroutine(RunRequest("/runs/" + id + "/end", JsonUtility.ToJson(new RunEnd { outcome=outcome, deaths=deaths, durationSeconds=duration }), null));
        }
        [Serializable] sealed class RunEnd { public string outcome; public int deaths; public int durationSeconds; }
        [Serializable] sealed class RunResult { public string id; }
        IEnumerator RunRequest(string path, string body, Action<string> completed)
        {
            if (config == null || auth == null || string.IsNullOrEmpty(auth.Token)) { yield break; }
            using var request = new UnityWebRequest(config.BaseUrl + path, body == null ? UnityWebRequest.kHttpVerbPOST : UnityWebRequest.kHttpVerbPOST);
            request.downloadHandler = new DownloadHandlerBuffer();
            if (body != null) request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            request.timeout = Mathf.CeilToInt(timeoutSeconds); request.SetRequestHeader("Authorization", "Bearer " + auth.Token);
            if (body != null) request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success && completed != null) completed(JsonUtility.FromJson<RunResult>(request.downloadHandler.text).id);
        }
        IEnumerator Request(string path, string method, string body, Action<CloudSavePayload, string> completed)
        {
            if (config == null || auth == null || string.IsNullOrEmpty(auth.Token)) { completed?.Invoke(null, "offline"); yield break; }
            using var request = new UnityWebRequest(config.BaseUrl + path, method);
            if (body != null) request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = Mathf.CeilToInt(timeoutSeconds);
            request.SetRequestHeader("Authorization", "Bearer " + auth.Token);
            if (body != null) request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
                completed?.Invoke(JsonUtility.FromJson<CloudSavePayload>(request.downloadHandler.text), null);
            else completed?.Invoke(null, request.responseCode == 404 ? "not_found" : request.error);
        }
    }
}
