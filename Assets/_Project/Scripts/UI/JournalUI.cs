using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TheLightHouse
{
    public sealed class JournalUI : MonoBehaviour
    {
        [SerializeField] LogbookJournal journal;
        [SerializeField] GameObject panel;
        [SerializeField] Text listLabel;
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] FirstPersonController player;
        [SerializeField] PlayerHealthState health;
        [SerializeField] Interactor interactor;
        [SerializeField] ActFourSettings settings;
        InputAction toggle;
        void Awake(){toggle=inputActions?.FindActionMap("Player")?.FindAction("Journal");if(panel)panel.SetActive(false);}
        void OnEnable()=>toggle?.Enable();
        void OnDisable(){toggle?.Disable();if(panel&&panel.activeSelf){panel.SetActive(false);if(!health||(!health.IsCaught&&!health.IsHidden)){player?.SetMovementLocked(false);player?.SetLookLocked(false);if(interactor)interactor.enabled=true;}GameEvents.RaiseReadingStateChanged(new(false,1,1));}}
        void Update(){if(toggle?.WasPressedThisFrame()??false)Toggle();}
        void Toggle()
        {
            if(!panel||!journal)return;
            bool show=!panel.activeSelf;panel.SetActive(show);
            if(show){player?.SetMovementLocked(true);player?.SetLookLocked(true);if(interactor)interactor.enabled=false;GameEvents.RaiseReadingStateChanged(new(true,settings?settings.readingThreatSpeed:.35f,settings?settings.readingLampDecayFactor:.3f));}
            else {if(!health||(!health.IsCaught&&!health.IsHidden)){player?.SetMovementLocked(false);player?.SetLookLocked(false);if(interactor)interactor.enabled=true;}GameEvents.RaiseReadingStateChanged(new(false,1,1));return;}
            var text=new StringBuilder("JOURNAL\n\n");
            foreach(var page in journal.Pages)text.Append(journal.HasRead(page.id)?"[read] ":"[new] ").AppendLine(page.title);
            listLabel.text=text.ToString();
        }
    }
}
