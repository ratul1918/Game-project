using System;
using UnityEngine;

namespace TheLightHouse.Voice
{
    [CreateAssetMenu(fileName = "VoiceLine", menuName = "TheLightHouse/Voice/Voice Line")]
    public sealed class VoiceLine : ScriptableObject
    {
        [SerializeField] private string lineId;
        [SerializeField] private SpeakerId speaker;
        [TextArea(2, 5)]
        [SerializeField] private string text;
        [SerializeField] private string actingDirection;
        [SerializeField] private string context;
        [SerializeField] private AudioClip clip;
        [SerializeField] private VoiceAudioType audioType = VoiceAudioType.Spoken;
        [SerializeField] private SpatialMode spatialMode = SpatialMode.TwoDimensional;
        [Range(0, 100)]
        [SerializeField] private int priority = 50; // 0 = Ambient/Reaction, 50 = Dialogue, 100 = Narrative Critical
        [SerializeField] private bool interruptible = true;
        [SerializeField] private float subtitleOverrideDuration;
        [SerializeField] private bool isPlaceholder = true;

        public string LineId { get => lineId; set => lineId = value; }
        public SpeakerId Speaker { get => speaker; set => speaker = value; }
        public string Text { get => text; set => text = value; }
        public string ActingDirection { get => actingDirection; set => actingDirection = value; }
        public string Context { get => context; set => context = value; }
        public AudioClip Clip { get => clip; set => clip = value; }
        public VoiceAudioType AudioType { get => audioType; set => audioType = value; }
        public SpatialMode SpatialMode { get => spatialMode; set => spatialMode = value; }
        public int Priority { get => priority; set => priority = value; }
        public bool Interruptible { get => interruptible; set => interruptible = value; }
        public float SubtitleOverrideDuration { get => subtitleOverrideDuration; set => subtitleOverrideDuration = value; }
        public bool IsPlaceholder { get => isPlaceholder; set => isPlaceholder = value; }

        public float GetEffectiveDuration()
        {
            if (clip != null && clip.length > 0.05f)
            {
                return clip.length;
            }

            if (subtitleOverrideDuration > 0.05f)
            {
                return subtitleOverrideDuration;
            }

            int wordCount = string.IsNullOrEmpty(text) ? 0 : text.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
            return Mathf.Max(2.0f, wordCount * 0.35f);
        }

        public DialogueLine ToDialogueLine()
        {
            return new DialogueLine(speaker.ToString(), text)
            {
                clip = clip,
                duration = GetEffectiveDuration()
            };
        }
    }
}
