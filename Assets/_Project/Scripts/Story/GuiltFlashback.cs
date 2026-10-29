using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TheLightHouse
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class GuiltFlashback : MonoBehaviour
    {
        [SerializeField] Image vignette;
        [SerializeField] AudioClip cue;
        [SerializeField] float fadeDuration=.6f;
        [SerializeField] float holdDuration=2f;
        [SerializeField,Range(0f,1f)] float peakAlpha=.7f;
        void OnEnable()=>GameEvents.LedgerRead+=Read;
        void OnDisable()=>GameEvents.LedgerRead-=Read;
        void Read(LedgerReadEvent e){StopAllCoroutines();StartCoroutine(Play());}
        IEnumerator Play()
        {
            if(cue)GetComponent<AudioSource>().PlayOneShot(cue);
            GameEvents.RequestDialogue(new("Daniel","Six men called from the water."));
            GameEvents.RequestDialogue(new("Daniel","I turned the boat away."));
            if(!vignette)yield break;
            yield return Fade(0f,peakAlpha);yield return new WaitForSeconds(holdDuration);yield return Fade(peakAlpha,0f);
        }
        IEnumerator Fade(float from,float to){float t=0;while(t<fadeDuration){t+=Time.deltaTime;var c=vignette.color;c.a=Mathf.Lerp(from,to,t/fadeDuration);vignette.color=c;yield return null;}}
    }
}
