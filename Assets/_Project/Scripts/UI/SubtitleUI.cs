using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace TheLightHouse {
[RequireComponent(typeof(AudioSource))]
public sealed class SubtitleUI : MonoBehaviour {
[SerializeField] Text label;
[SerializeField,Min(0.1f)] float fallbackDuration=4f;
readonly Queue<DialogueLine> queue=new();
public bool Busy=>queue.Count>0 || playing;
bool playing;
void OnEnable()=>GameEvents.DialogueRequested+=Enqueue;
void OnDisable(){ GameEvents.DialogueRequested-=Enqueue; StopAllCoroutines(); queue.Clear(); playing=false; }
void Enqueue(DialogueLine line){queue.Enqueue(line); if(!playing) StartCoroutine(Play());}
IEnumerator Play(){playing=true; while(queue.Count>0){var line=queue.Dequeue(); label.text=$"{line.speaker}: {line.text}"; if(line.clip) GetComponent<AudioSource>().PlayOneShot(line.clip); yield return new WaitForSeconds(line.clip?line.clip.length:line.duration>0?line.duration:fallbackDuration); } label.text="";playing=false;}
}}
