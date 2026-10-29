using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TheLightHouse
{
    public sealed class LogbookReaderUI : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] Text titleLabel;
        [SerializeField] Text bodyLabel;
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] FirstPersonController player;
        [SerializeField] PlayerHealthState health;
        [SerializeField] Interactor interactor;
        [SerializeField] ActFourSettings settings;
        InputAction interact, cancel;
        float ignoreCloseUntil;
        public bool IsOpen => panel && panel.activeSelf;

        void Awake()
        {
            interact=inputActions?.FindActionMap("Player")?.FindAction("Interact");
            cancel=inputActions?.FindActionMap("UI")?.FindAction("Cancel");
            if(panel)panel.SetActive(false);
        }
        void OnEnable(){interact?.Enable();cancel?.Enable();}
        void OnDisable(){interact?.Disable();cancel?.Disable();if(IsOpen)Close();}
        void Update(){if(IsOpen&&Time.unscaledTime>=ignoreCloseUntil&&((interact?.WasPressedThisFrame()??false)||(cancel?.WasPressedThisFrame()??false)))Close();}
        public void Show(string title,string body)
        {
            if(!panel)return;
            titleLabel.text=title;bodyLabel.text=body;panel.SetActive(true);ignoreCloseUntil=Time.unscaledTime+.2f;
            player?.SetMovementLocked(true);player?.SetLookLocked(true);
            if(interactor)interactor.enabled=false;
            GameEvents.RaiseReadingStateChanged(new(true,settings?settings.readingThreatSpeed:0.35f,settings?settings.readingLampDecayFactor:0.3f));
        }
        public void Close()
        {
            if(panel)panel.SetActive(false);
            if(!health||(!health.IsCaught&&!health.IsHidden)){player?.SetMovementLocked(false);player?.SetLookLocked(false);if(interactor)interactor.enabled=true;}
            GameEvents.RaiseReadingStateChanged(new(false,1f,1f));
        }
    }
}
