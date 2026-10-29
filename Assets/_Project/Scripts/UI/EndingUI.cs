using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TheLightHouse
{
    public sealed class EndingUI : MonoBehaviour
    {
        [SerializeField] CanvasGroup group;
        [SerializeField] Text title;
        [SerializeField] Text credits;
        [SerializeField] float fadeDuration=3f;
        void Awake(){if(group)group.alpha=0;}
        void OnEnable()=>GameEvents.GameStateChanged+=State;
        void OnDisable()=>GameEvents.GameStateChanged-=State;
        void State(GameStateChangedEvent e){if(e.Current==GameState.Ending)StartCoroutine(Fade());}
        IEnumerator Fade(){if(title)title.text="The LightHouse";if(credits)credits.text="Credits placeholder";float t=0;while(t<fadeDuration){t+=Time.deltaTime;if(group)group.alpha=Mathf.Clamp01(t/fadeDuration);yield return null;}Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
    }
}
