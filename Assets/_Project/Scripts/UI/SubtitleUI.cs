using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace TheLightHouse {
[RequireComponent(typeof(AudioSource))]
public sealed class SubtitleUI : MonoBehaviour {
[SerializeField] Text label;
[SerializeField] Image background;
[SerializeField,Min(0f)] float horizontalPadding=48f;
[SerializeField,Min(0f)] float verticalPadding=18f;
[SerializeField,Min(0.1f)] float fallbackDuration=4f;
readonly Queue<DialogueLine> queue=new();
public bool Busy=>queue.Count>0 || playing;
bool playing;
void Awake()=>Hide();
public void ApplySettings(HudSettings settings){if(!settings)return;if(label)label.fontSize=Mathf.RoundToInt(26f*settings.SubtitleScale);if(background){Color color=background.color;color.a=settings.SubtitleBackgroundOpacity;background.color=color;}Hide();}
void OnEnable()=>GameEvents.DialogueRequested+=Enqueue;
void OnDisable(){ GameEvents.DialogueRequested-=Enqueue; StopAllCoroutines(); queue.Clear(); playing=false; }
void Enqueue(DialogueLine line){queue.Enqueue(line); if(!playing) StartCoroutine(Play());}
IEnumerator Play(){playing=true; while(queue.Count>0){var line=queue.Dequeue();Show(line);if(line.clip)GetComponent<AudioSource>().PlayOneShot(line.clip);yield return new WaitForSeconds(line.clip?line.clip.length:line.duration>0?line.duration:fallbackDuration);}Hide();playing=false;}
void Show(DialogueLine line)
{
    if(!label)return;
    label.text=$"{line.speaker}: {line.text}";
    label.enabled=true;
    RectTransform labelRect=label.rectTransform;
    Vector2 size=new(Mathf.Max(1f,label.preferredWidth)+horizontalPadding,Mathf.Max(1f,label.preferredHeight)+verticalPadding);
    labelRect.sizeDelta=size;
    if(background){background.enabled=true;background.rectTransform.sizeDelta=size;}
}
void Hide(){if(label){label.text="";label.enabled=false;}if(background)background.enabled=false;}
}}
