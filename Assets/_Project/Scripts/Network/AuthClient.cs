using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace TheLightHouse
{
    public sealed class AuthClient : MonoBehaviour
    {
        [Serializable] sealed class Credentials{public string username;public string password;}
        [Serializable] sealed class TokenResponse{public string token;}
        public string Token{get;private set;}
        public void Authenticate(NetworkConfig config,Action<string> completed)=>StartCoroutine(AuthenticateRoutine(config,completed));
        IEnumerator AuthenticateRoutine(NetworkConfig config,Action<string> completed)
        {
            if(config.AutoRegisterDevUser)yield return Send(config.BaseUrl+"/auth/register",config.DevUsername,config.DevPassword,null);
            string token=null;yield return Send(config.BaseUrl+"/auth/login",config.DevUsername,config.DevPassword,value=>token=value);Token=token;completed?.Invoke(token);
        }
        IEnumerator Send(string url,string username,string password,Action<string> tokenReceived)
        {
            string body=JsonUtility.ToJson(new Credentials{username=username,password=password});using var request=new UnityWebRequest(url,UnityWebRequest.kHttpVerbPOST){uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),downloadHandler=new DownloadHandlerBuffer()};request.SetRequestHeader("Content-Type","application/json");yield return request.SendWebRequest();if(request.result==UnityWebRequest.Result.Success){TokenResponse response=JsonUtility.FromJson<TokenResponse>(request.downloadHandler.text);tokenReceived?.Invoke(response?.token);}
        }
    }
}
