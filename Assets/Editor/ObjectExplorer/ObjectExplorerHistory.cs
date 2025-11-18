using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ObjectExplorer
{
    public class ObjectExplorerHistory : PopupWindowContent
    {
        private List<string> lines = new List<string>();
        private Action<string> onSave;
        private Action<string> onSelect;
        private Vector2 scrollPos;
        private bool removed;

        public void Init(string hs, Action<string> onSave, Action<string> onSelect)
        {
            foreach (var s in hs.Split(new char[]{'\n'}))
            {
                if (!string.IsNullOrEmpty(s))
                {
                    lines.Add(s);
                }
            }
            lines.Sort();
            this.onSave = onSave;
            this.onSelect= onSelect;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(250, 200);
        }

        public override void OnGUI(Rect rect)
        {
            int remove = -1;
            using (var s = new GUILayout.ScrollViewScope(scrollPos))
            {
                scrollPos = s.scrollPosition;
                for (var i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(line))
                        {
                            onSelect(line);
                            editorWindow.Close();
                        }

                        if (GUILayout.Button("X", GUILayout.ExpandWidth(false)))
                        {
                            remove = i;
                        }
                    }
                }
            }

            if (remove >= 0)
            {
                lines.RemoveAt(remove);
                removed = true;
            }
        }

        public override void OnOpen()
        {
        }

        public override void OnClose()
        {
            if (removed)
            {
                StringBuilder sb = new StringBuilder("");
                foreach (var line in lines)
                {
                    sb.Append(line);
                    sb.Append("\n");
                }
                onSave(sb.ToString());
            }
        }
    }
}