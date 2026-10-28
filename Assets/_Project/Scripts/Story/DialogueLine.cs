using System;
using UnityEngine;
namespace TheLightHouse {
[Serializable] public sealed class DialogueLine {
public string speaker;
[TextArea] public string text;
public AudioClip clip;
[Min(0.1f)] public float duration=4f;
public DialogueLine(string speaker,string text) { this.speaker=speaker; this.text=text; }
}}
