using System;
using System.Collections.Generic;
using System.Text;

namespace MoonshineSim.UI
{
    /// <summary>
    /// Plain-C# state for the in-game clipboard: an ordered checklist plus a
    /// set of live data rows. No Unity dependency (the ProofingUtility pattern)
    /// so it stays testable and whatever renders it is free to change.
    ///
    /// Items tick themselves from game events for now; manual ticking can be
    /// layered on later without touching this class.
    /// </summary>
    public class ClipboardModel
    {
        public sealed class Entry
        {
            public string Id;
            public string Label;
            public bool Done;
            public bool Available = true;
        }

        private readonly List<Entry> _checklist = new();
        private readonly List<string> _dataOrder = new();
        private readonly Dictionary<string, string> _data = new();
        private string _noteHeader;
        private string _noteBody;

        /// <summary>Raised whenever the checklist or data rows change.</summary>
        public event Action Changed;

        public IReadOnlyList<Entry> Checklist => _checklist;
        public int DoneCount => _checklist.FindAll(e => e.Done).Count;
        public int AvailableCount => _checklist.FindAll(e => e.Available).Count;

        public void AddItem(string id, string label, bool available = true)
        {
            _checklist.Add(new Entry { Id = id, Label = label, Available = available });
            Changed?.Invoke();
        }

        public void SetDone(string id, bool done = true)
        {
            var e = _checklist.Find(x => x.Id == id);
            if (e == null || e.Done == done) return;
            e.Done = done;
            if (done) e.Available = true;
            Changed?.Invoke();
        }

        public void SetAvailable(string id, bool available)
        {
            var e = _checklist.Find(x => x.Id == id);
            if (e == null || e.Available == available) return;
            e.Available = available;
            Changed?.Invoke();
        }

        public void SetData(string key, string value)
        {
            if (_data.TryGetValue(key, out var current) && current == value) return;
            if (!_data.ContainsKey(key)) _dataOrder.Add(key);
            _data[key] = value;
            Changed?.Invoke();
        }

        public void ClearData()
        {
            if (_data.Count == 0) return;
            _data.Clear();
            _dataOrder.Clear();
            Changed?.Invoke();
        }

        /// <summary>A free-text block rendered after the data rows (e.g. the buyer list).</summary>
        public void SetNote(string header, string body)
        {
            if (_noteHeader == header && _noteBody == body) return;
            _noteHeader = header;
            _noteBody = body;
            Changed?.Invoke();
        }

        /// <summary>Flat text rendering for the prototype clipboard panel.</summary>
        public string Render()
        {
            var sb = new StringBuilder();
            sb.Append("CHECKLIST  ").Append(DoneCount).Append('/').Append(AvailableCount).Append('\n');
            foreach (var e in _checklist)
            {
                sb.Append(e.Done ? "[x] " : "[ ] ").Append(e.Label);
                if (!e.Available) sb.Append("   (later)");
                sb.Append('\n');
            }

            if (_dataOrder.Count > 0)
            {
                sb.Append("\nCURRENT BATCH\n");
                foreach (var key in _dataOrder)
                {
                    sb.Append(key).Append(": ").Append(_data[key]).Append('\n');
                }
            }

            if (!string.IsNullOrEmpty(_noteBody))
            {
                sb.Append('\n');
                if (!string.IsNullOrEmpty(_noteHeader)) sb.Append(_noteHeader).Append('\n');
                sb.Append(_noteBody);
                if (!_noteBody.EndsWith("\n")) sb.Append('\n');
            }
            return sb.ToString();
        }
    }
}
