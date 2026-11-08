using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheLightHouse.Voice
{
    [CreateAssetMenu(fileName = "VoiceDatabase", menuName = "TheLightHouse/Voice/Voice Database")]
    public sealed class VoiceDatabase : ScriptableObject
    {
        [SerializeField] private List<VoiceLine> allLines = new();
        private readonly Dictionary<string, VoiceLine> _lookup = new(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyList<VoiceLine> AllLines => allLines;

        public void InitializeLookup()
        {
            _lookup.Clear();
            if (allLines == null) return;

            foreach (var line in allLines)
            {
                if (line != null && !string.IsNullOrEmpty(line.LineId))
                {
                    _lookup[line.LineId] = line;
                }
            }
        }

        public bool TryGetLine(string id, out VoiceLine line)
        {
            if (string.IsNullOrEmpty(id))
            {
                line = null;
                return false;
            }

            if (_lookup.Count != allLines.Count)
            {
                InitializeLookup();
            }

            if (string.Equals(id, "A4_MARTA_REVEAL_001", StringComparison.OrdinalIgnoreCase))
            {
                if (_lookup.TryGetValue("A1_MARTA_001", out line)) return true;
            }

            return _lookup.TryGetValue(id, out line);
        }

        public VoiceLine GetLine(string id)
        {
            TryGetLine(id, out var line);
            return line;
        }

        public void SetLines(IEnumerable<VoiceLine> lines)
        {
            allLines.Clear();
            if (lines != null)
            {
                allLines.AddRange(lines);
            }
            InitializeLookup();
        }

        public void AddLine(VoiceLine line)
        {
            if (line == null || string.IsNullOrEmpty(line.LineId)) return;
            if (!allLines.Contains(line))
            {
                allLines.Add(line);
            }
            _lookup[line.LineId] = line;
        }

        public List<VoiceLine> GetLinesBySpeaker(SpeakerId speaker)
        {
            var result = new List<VoiceLine>();
            foreach (var line in allLines)
            {
                if (line != null && line.Speaker == speaker)
                {
                    result.Add(line);
                }
            }
            return result;
        }

        public List<VoiceLine> GetLinesByAct(int actNumber)
        {
            var prefix = $"A{actNumber}_";
            var result = new List<VoiceLine>();
            foreach (var line in allLines)
            {
                if (line != null && !string.IsNullOrEmpty(line.LineId) && line.LineId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(line);
                }
            }
            return result;
        }
    }
}
