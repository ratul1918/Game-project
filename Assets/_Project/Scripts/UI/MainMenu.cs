using UnityEngine;

namespace TheLightHouse
{
    public sealed class MainMenu : MonoBehaviour
    {
        [SerializeField] ActFlowController flow;
        [SerializeField] GameObject root;
        [SerializeField] GameOptionsData options=new();
        public void StartGame(){root?.SetActive(false);flow?.StartNew();}
        public void ContinueGame(){root?.SetActive(false);flow?.Continue();}
        public void SetReducedStorm(bool value){options.reducedStorm=value;flow?.ApplyOptions(options);}
        public void SetDisableBob(bool value){options.disableBobAndFov=value;flow?.ApplyOptions(options);}
        public void SetSensitivity(float value){options.sensitivity=value;flow?.ApplyOptions(options);}
        public void SetVolume(float value){options.volume=value;AudioListener.volume=value;flow?.ApplyOptions(options);}
        public void Quit(){Application.Quit();}
    }
}
