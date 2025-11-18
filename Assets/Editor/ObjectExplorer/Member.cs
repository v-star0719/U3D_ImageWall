using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace ObjectExplorer
{
    public abstract class Member
    {
        protected Type valueType;
        protected string path;
        public string Name { get; protected set; }
        public bool IsStatic { get; protected set; }
        public virtual bool IsField => false;
        public virtual bool IsProperty => false;
        public virtual bool IsListItem => false;
        public virtual bool CanGet => true;
        public virtual bool CanSet => true;
        public virtual bool IsMonoBehaviour { get; protected set; }

        protected List<Member> listMembers;

        protected void Init(string path)
        {
            this.path = path;
        }

        public virtual void Recycle()
        {
            IsStatic = false;
            if (listMembers != null)
            {
                ObjectExplorer.listPool.Recycle(listMembers);
                listMembers = null;
            }
        }
        public virtual void OnGUI()
        {
            try
            {
                if (!CanGet)
                {
                    return;
                }

                bool e = GUI.enabled;
                GUI.enabled = CanSet;
                Type type = valueType;
                if (type == typeof(int))
                {
                    GUIInt();
                }
                else if (type == typeof(float))
                {
                    GUIFloat();
                }
                else if (type == typeof(string))
                {
                    GUIText();
                }
                else if (type == typeof(bool))
                {
                    GUIToggle();
                }
                else if (type == typeof(Color))
                {
                    GUIColor();
                }
                else if (type == typeof(Vector2))
                {
                    GUIVector2();
                }
                else if (type == typeof(Vector2Int))
                {
                    GUIVector2Int();
                }
                else if (type == typeof(Vector3))
                {
                    GUIVector3();
                }
                else if (type == typeof(Vector3Int))
                {
                    GUIVector3Int();
                }
                else if (type == typeof(Vector4))
                {
                    GUIVector4();
                }
                else if (type == typeof(Quaternion))
                {
                    GUIQuaternion();
                }
                else if (type.IsEnum)
                {
                    GUIEnum();
                }
                else if (type.IsSubclassOf(typeof(Delegate)))
                {
                    GUIDelegate();
                }
                else if (typeof(IList).IsAssignableFrom(type))
                {
                    GUIList();
                }
                else if (typeof(IDictionary).IsAssignableFrom(type))
                {
                    GUIDictionary();
                }
                else if (IsUnityObject(type))
                {
                    GUIUnityObject();
                }
                else
                {
                    GUIObject();
                }
                GUI.enabled = e;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public virtual object GetValue()
        {
            return null;
        }

        public virtual void SetValue(object obj)
        {
        }
        
        private void GUIInt()
        {
            var v = (int)GetValue();
            var nv = EditorGUILayout.IntField(Name, v);
            if(v != nv)
            {
                SetValue(nv);
            }
        }

        private void GUILong()
        {
            var v = (int)GetValue();
            var nv = EditorGUILayout.LongField(Name, v);
            if (v != nv)
            {
                SetValue(nv);
            }
        }

        private void GUIVector2()
        {
            var v = (Vector2)GetValue();
            var nv = EditorGUILayout.Vector2Field(Name, v);
            if (!v.Equals(nv))
            {
                SetValue(nv);
            }
        }

        private void GUIVector2Int()
        {
            var v = (Vector2Int)GetValue();
            var nv = EditorGUILayout.Vector2IntField(Name, v);
            if (!v.Equals(nv))
            {
                SetValue(nv);
            }
        }

        private void GUIVector3()
        {
            var v = (Vector3)GetValue();
            var nv = EditorGUILayout.Vector3Field(new GUIContent(Name), v);
            if (!v.Equals(nv))
            {
                SetValue(nv);
            }
        }

        private void GUIVector3Int()
        {
            var v = (Vector3Int)GetValue();
            var nv = EditorGUILayout.Vector3IntField(Name, v);
            if (!v.Equals(nv))
            {
                SetValue(nv);
            }
        }

        private void GUIVector4()
        {
            var v = (Vector4)GetValue();
            var nv = EditorGUILayout.Vector4Field(Name, v);
            if (!v.Equals(nv))
            {
                SetValue(nv);
            }
        }

        private void GUIQuaternion()
        {
            if (!ObjectExplorer.GUIFoldout(path, Name))
            {
                return;
            }
            var v = (Quaternion)GetValue();

            EditorGUI.indentLevel++;

            var eulerAngles = v.eulerAngles;
            var nEulerAngles = EditorGUILayout.Vector3Field("eulerAngles", v.eulerAngles);
            if (!nEulerAngles.Equals(eulerAngles))
            {
                SetValue(Quaternion.Euler(nEulerAngles));
            }

            var nv = v;
            nv.x = EditorGUILayout.FloatField("x", nv.x);
            nv.y = EditorGUILayout.FloatField("y", nv.x);
            nv.z = EditorGUILayout.FloatField("z", nv.x);
            nv.w = EditorGUILayout.FloatField("w", nv.x);
            if (!v.Equals(nv))
            {
                SetValue(nv);
            }

            EditorGUI.indentLevel--;
        }

        private void GUIFloat()
        {
            var v = (float)GetValue();
            var nv = EditorGUILayout.FloatField(Name, v);
            if(Math.Abs(v - nv) > 0.0001f)
            {
                SetValue(nv);
            }
        }

        private void GUIText()
        {
            var v = (string)GetValue();
            var nv = EditorGUILayout.TextField(Name, v);
            if(v != nv)
            {
                SetValue(nv);
            }
        }

        private void GUIToggle()
        {
            var v = (bool)GetValue();
            var nv = EditorGUILayout.Toggle(Name, v);
            if(v != nv)
            {
                SetValue(nv);
            }
        }

        private void GUIColor()
        {
            var v = (Color)GetValue();
            var nv = EditorGUILayout.ColorField(Name, v);
            if(v != nv)
            {
                SetValue(nv);
            }
        }

        private void GUIEnum()
        {
            var v = (Enum)GetValue();
            var nv = EditorGUILayout.EnumPopup(Name, v);
            if(!Equals(v, nv))
            {
                SetValue(nv);
            }
        }

        private void GUIDelegate()
        {
            var v = (Delegate)GetValue();
            EditorGUILayout.LabelField(Name + " [Delegate] " + (v?.GetInvocationList().Length ?? 0));
        }

        private void GUIUnityObject()
        {
            var v = (UnityEngine.Object)GetValue();
            var nv = EditorGUILayout.ObjectField(Name, v, valueType, true);
            if(v != nv)
            {
                SetValue(nv);
            }
        }

        private void GUIObject()
        {
            var v = GetValue();
            if(v == null)
            {
                EditorGUILayout.LabelField(Name, "null");
                return;
            }

            if(!ObjectExplorer.GUIFoldout(path, Name))
            {
                return;
            }

            EditorGUI.indentLevel++;

            ObjectExplorer.GUIObject(v, valueType, path);

            EditorGUI.indentLevel--;
        }

        private void GUIList()
        {
            var listObj = GetValue();
            if (listObj == null)
            {
                EditorGUILayout.LabelField(Name, "null");
                return;
            }

            //foreach (var info in valueType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            //{
            //    EditorGUILayout.LabelField(info.Name);
            //}

            IList iList = listObj as IList;
            var count = iList.Count;
            if (!ObjectExplorer.GUIFoldout(path, Name + " " + count))
            {
                return;
            }

            EditorGUI.indentLevel++;

            var elementType = valueType.GetElementType();
            if (elementType == null)
            {
                var itemProperty = valueType.GetProperty("Item", BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance);
                if (itemProperty != null)
                {
                    var getIndexer = itemProperty.GetGetMethod();
                    elementType = getIndexer.ReturnType;
                }
            }

            for (int i = 0; i < count; i++)
            {
                var m = ObjectExplorer.listMemberPool.Get();
                m.Init(iList, i, elementType, path + "." + i);
                m.OnGUI();
                m.Recycle();
            }

            EditorGUI.indentLevel--;
        }

        private void GUIDictionary()
        {
            var dictObj = GetValue();
            if (dictObj == null)
            {
                EditorGUILayout.LabelField(Name, "null");
                return;
            }

            //foreach (var info in valueType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            //{
            //    EditorGUILayout.LabelField(info.Name);
            //}

            var iDict = dictObj as IDictionary;
            var count = iDict.Count;
            if (!ObjectExplorer.GUIFoldout(path, Name + " " + count))
            {
                return;
            }

            EditorGUI.indentLevel++;

            var elementType = valueType.GetElementType();
            if (elementType == null)
            {
                var itemProperty = valueType.GetProperty("Item", BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance);
                if (itemProperty != null)
                {
                    var getIndexer = itemProperty.GetGetMethod();
                    elementType = getIndexer.ReturnType;
                }
            }

            for (int i = 0; i < count; i++)
            {
                var m = ObjectExplorer.dictMemberPool.Get();
                m.Init(iDict, i, elementType, path + "." + i);
                m.OnGUI();
                m.Recycle();
            }
            
            EditorGUI.indentLevel--;
        }

        public bool IsUnityObject(Type type)
        {
            return type.Namespace != null && (type.IsSubclassOf(typeof(UnityEngine.Object)) && type.Namespace.StartsWith("Unity"));
        }
    }

    public class MemberPool<T> where T : Member, new()
    {
        private Stack<T> stack = new Stack<T>();

        public T Get()
        {
            if (stack.Count > 0)
            {
                return stack.Pop();
            }
            return new T();
        }

        public void Recycle(T t)
        {
            stack.Push(t);
        }
    }

    public class ListPool
    {
        private Stack<List<Member>> stack = new Stack<List<Member>>();
        public List<Member> Get()
        {
            if(stack.Count > 0)
            {
                return stack.Pop();
            }
            return new List<Member>(16);
        }

        public void Recycle(List<Member> list)
        {
            foreach (var member in list)
            {
                member.Recycle();
            }
            list.Clear();
            stack.Push(list);
        }
    }
}
