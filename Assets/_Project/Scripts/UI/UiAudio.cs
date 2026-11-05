using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLightHouse
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class UiAudio : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        [SerializeField] AudioClip click;
        [SerializeField] AudioClip hover;
        [SerializeField] AudioClip pageTurn;
        AudioSource source;
        void Awake(){source=GetComponent<AudioSource>();source.playOnAwake=false;}
        public void OnPointerEnter(PointerEventData eventData){if(hover)source.PlayOneShot(hover);}
        public void OnPointerClick(PointerEventData eventData){if(click)source.PlayOneShot(click);}
        public void PlayPageTurn(){if(pageTurn)source.PlayOneShot(pageTurn);}
    }
}
