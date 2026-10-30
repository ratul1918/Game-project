using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace TheLightHouse
{
    public sealed class MainMenu : MonoBehaviour
    {
        [SerializeField] ActFlowController flow;
        [SerializeField] GameObject root;
        [SerializeField] GameOptionsData options=new();
        [SerializeField] GameObject loginPanel;
        [SerializeField] NetworkConfig networkConfig;
        [SerializeField] InputField usernameInput;
        [SerializeField] InputField passwordInput;
        [SerializeField] Text connectionStatus;
        [SerializeField] Text telemetryNote;
        void Awake(){if(telemetryNote)telemetryNote.text="Telemetry collects gameplay events only; no personal data beyond your username.";}
        public void StartGame(){root?.SetActive(false);flow?.StartNew();}
        public void PlayOffline(){root?.SetActive(false);flow?.StartNew();}
        public void ShowLogin(){loginPanel?.SetActive(true);}
        public void Login(){Authenticate(false);}
        public void Register(){Authenticate(true);}
        void Authenticate(bool register)
        {
            var auth=FindAnyObjectByType<AuthClient>();
            if(!auth||!networkConfig){if(connectionStatus)connectionStatus.text="Offline";return;}
            Action<bool> completed=success=>{if(connectionStatus)connectionStatus.text=success?"Connected":"Offline";};
            if(register)auth.Register(networkConfig,usernameInput?.text,passwordInput?.text,completed);
            else auth.Login(networkConfig,usernameInput?.text,passwordInput?.text,completed);
        }
        public void ContinueGame(){root?.SetActive(false);flow?.Continue();}
        public void SetReducedStorm(bool value){options.reducedStorm=value;flow?.ApplyOptions(options);}
        public void SetDisableBob(bool value){options.disableBobAndFov=value;flow?.ApplyOptions(options);}
        public void SetSensitivity(float value){options.sensitivity=value;flow?.ApplyOptions(options);}
        public void SetVolume(float value){options.volume=value;AudioListener.volume=value;flow?.ApplyOptions(options);}
        public void SetRenderScale(float value){options.renderScale=Mathf.Clamp(value,.7f,1f);if(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset pipeline)pipeline.renderScale=options.renderScale;flow?.ApplyOptions(options);}
        public void SetTelemetryEnabled(bool value){options.telemetryEnabled=value;flow?.ApplyOptions(options);}
        public void Quit(){Application.Quit();}
    }
}
