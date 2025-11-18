using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ObjectExplorer
{
    public class ObjectExplorer : EditorWindow
    {
        private enum TargetType
        {
            StaticInstance, //C# static Instance
            GameObject, //unity game object
        }

        private float refreshTimer;
        private Vector2 scrollPos;

        private TargetType targetType;
        //
        private string targetStaticInstancePath;
        private object targetStaticInstance;
        private Type targetStaticType;
        //
        private string targetGoPath;
        private GameObject targetGo;
        private Component targetComponent;
        private string[] componentMenus;
        private int componentIndex;
        private Rect buttonRect;

        private static bool showUnityMembers = true;
        private static Dictionary<string, bool> foldoutDict = new Dictionary<string, bool>();
        public static MemberPool<FieldMember> fieldMemberPool = new MemberPool<FieldMember>();
        public static MemberPool<PropertyMember> propertyMemberPool = new MemberPool<PropertyMember>();
        public static MemberPool<ListMember> listMemberPool = new MemberPool<ListMember>();
        public static MemberPool<DictionaryMember> dictMemberPool = new MemberPool<DictionaryMember>();
        public static ListPool listPool = new ListPool();

        private float forceRefreshTimer = 0;
        private string staticInstancePathHistory;

        private bool ForceRefresh
        {
            get => forceRefreshTimer < 0 && forceRefreshTimer > -10f;
            set => forceRefreshTimer = value ? 0.5f : -100f;
        }

        [MenuItem("Window/ObjectExplorer", false, 900)]
        public static void ShowWindow()
        {
            GetWindow<ObjectExplorer>("ObjectExplorer");
        }

        private void OnEnable()
        {
            ForceRefresh = true;
            LoadSettings();
        }

        private void Update()
        {
            forceRefreshTimer -= Time.deltaTime;
            refreshTimer += Time.deltaTime;
            if(refreshTimer > 0.3f)
            {
                refreshTimer = 0f;
                Repaint();
            }
        }

        private void OnGUI()
        {
            using(new EditorGUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.ExpandWidth(true)))
            {
                GUIToolBar();
            }

            EditorGUIUtility.hierarchyMode = true;
            EditorGUIUtility.wideMode = true;
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, EditorStyles.inspectorDefaultMargins);
            {
                object target;
                Type type;
                if(targetType == TargetType.GameObject)
                {
                    if (targetComponent == null)
                    {
                        targetComponent = null;//如果不设置为null，后面转为System.object时判断又不是null，取值就报错了
                    }
                    target = targetComponent;
                    type = targetComponent?.GetType();
                }
                else
                {
                    target = targetStaticInstance;
                    type = targetStaticType;
                }
                GUIObject(target, type, "");
            }
            EditorGUILayout.EndScrollView();
            EditorGUIUtility.hierarchyMode = false;
        }

        private void GUIToolBar()
        {
            var tt = (TargetType)EditorGUILayout.EnumPopup(targetType, EditorStyles.toolbarDropDown, GUILayout.Width(120));
            if (tt != targetType)
            {
                targetType = tt;
                SaveSettings();
            }

            if(targetType == TargetType.StaticInstance)
            {
                var p = EditorGUILayout.TextField(targetStaticInstancePath);
                if(p != targetStaticInstancePath || ForceRefresh)
                {
                    targetStaticInstancePath = p;
                    RefreshTargetStaticInstance();
                    ForceRefresh = false;
                }

                if (EditorGUILayout.DropdownButton(new GUIContent("History"), FocusType.Keyboard, EditorStyles.toolbarDropDown, GUILayout.ExpandWidth(false)))
                {
                    var c = new ObjectExplorerHistory();
                    c.Init(staticInstancePathHistory, 
                        hs =>
                        {
                            staticInstancePathHistory = hs;
                            SaveSettings();
                        },
                        pt =>
                        {
                            targetStaticInstancePath = pt;
                            ForceRefresh = true;
                        });
                    buttonRect.x -= 170;
                    PopupWindow.Show(buttonRect, c);
                }

                if (Event.current.type == EventType.Repaint)
                {
                    buttonRect = GUILayoutUtility.GetLastRect();
                }
                if (GUILayout.Button("?", GUILayout.ExpandWidth(false)))
                {
                    EditorUtility.DisplayDialog("?", "NameSpace.ClassA>Instance. e.g. UnityEngine.Vector3", "ok");
                }
            }
            else
            {
                var go = EditorGUILayout.ObjectField(targetGo, typeof(GameObject), true, GUILayout.ExpandWidth(false), GUILayout.MinWidth(100)) as GameObject;
                if(go == null && !string.IsNullOrEmpty(targetGoPath) && Application.isPlaying || ForceRefresh)
                {
                    go = GetGameObjectByPath(targetGoPath);
                    ForceRefresh = false;
                }

                if(go != targetGo)
                {
                    targetGoPath = go != null ? GetTransformPath(go.transform) : "";
                    componentMenus = null;
                    targetComponent = null;
                    if (targetGo != null)
                    {
                        componentIndex = -1;
                    }
                    targetGo = go;
                    RefreshComponentMenu();
                }

                if (EditorGUILayout.DropdownButton(new GUIContent(targetComponent != null ? targetComponent.GetType().Name : ""), 
                    FocusType.Keyboard, EditorStyles.toolbarDropDown, GUILayout.Width(150)))
                {
                    RefreshComponentMenu();
                    var contens = new GUIContent[componentMenus.Length];
                    for (var i = 0; i < componentMenus.Length; i++)
                    {
                        var s = componentMenus[i];
                        contens[i] = new GUIContent(s);
                    }
                    Vector2 mousePos = Event.current.mousePosition;
                    EditorUtility.DisplayCustomMenu(new Rect(mousePos.x - 120, mousePos.y, 0, 0), contens, componentIndex,
                        (data, options, selected) =>
                        {
                            if (selected != componentIndex || targetComponent == null)
                            {
                                componentIndex = selected;
                                RefreshComponent();
                            }
                        }, null);
                }

                if (targetGo != null && targetComponent == null)
                {
                    RefreshComponent();
                }

                var clr = GUI.color;
                if (showUnityMembers)
                {
                    GUI.color = Color.yellow;
                }
                if (GUILayout.Button("ShowUnity", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                {
                    showUnityMembers = !showUnityMembers;
                    SaveSettings();
                }
                GUI.color = clr;

                if (targetType == TargetType.GameObject)
                {
                    GUILayout.Label(targetGoPath);
                }
            }
        }
        
        public static void GUIObject(object obj, Type type, string path)
        {
            if(obj == null && type == null)
            {
                return;
            }

            var tempListSF = listPool.Get();
            var tempListF = listPool.Get();
            var tempListSP = listPool.Get();
            var tempListP = listPool.Get();
            var memberList = listPool.Get();
            GetMembers(obj, type, path, memberList);
            foreach (var member in memberList)
            {
                if (member.IsField)
                {
                    if (member.IsStatic)
                    {
                        tempListSF.Add(member);
                    }
                    else
                    {
                        tempListF.Add(member);
                    }
                }
                else
                {
                    if (member.IsStatic)
                    {
                        tempListSP.Add(member);
                    }
                    else
                    {
                        tempListP.Add(member);
                    }
                }
            }
            GUIMemberList(tempListSF, "[Static Field]");
            GUIMemberList(tempListF, "[Field]");
            GUIMemberList(tempListSP, "[Static Property]");
            GUIMemberList(tempListP, "[Property]");
            tempListSF.Clear();
            tempListF.Clear();
            tempListSP.Clear();
            tempListP.Clear();
            listPool.Recycle(tempListSF);
            listPool.Recycle(tempListF);
            listPool.Recycle(tempListSP);
            listPool.Recycle(tempListP);
            listPool.Recycle(memberList);
        }

        private static void GUIMemberList(List<Member> list, string title)
        {
            if (list.Count <= 0)
            {
                return;
            }

            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            foreach (var member in list)
            {
                member.OnGUI();
            }
        }

        public static bool GUIFoldout(string namePath, string name)
        {
            var b = false;
            var path = namePath + "." + name;
            foldoutDict.TryGetValue(path, out b);
            var nb = EditorGUILayout.Foldout(b, name, true);
            if(b ^ nb)
            {
                foldoutDict[path] = nb;
            }

            return nb;
        }

        private void LoadSettings()
        {
            staticInstancePathHistory = PlayerPrefs.GetString("ObjectExplorer.hs", string.Empty);
            targetType = (TargetType)PlayerPrefs.GetInt("ObjectExplorer.tt", 0);
            showUnityMembers = PlayerPrefs.GetInt("ObjectExplorer.sum", 1) > 0;
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetString("ObjectExplorer.hs", staticInstancePathHistory);
            PlayerPrefs.SetInt("ObjectExplorer.tt", (int)targetType);
            PlayerPrefs.SetInt("ObjectExplorer.sum", showUnityMembers ? 1 : 0);
        }

        void RefreshComponentMenu()
        {
            if (targetGo == null)
            {
                return;
            }

            var cs = targetGo.GetComponents<Component>();
            componentMenus = new string[cs.Length];
            for (int i = 0; i < cs.Length; i++)
            {
                componentMenus[i] = cs[i].GetType().Name;
            }
            Array.Sort(componentMenus);
        }

        void RefreshComponent()
        {
            targetComponent = null;
            if(componentMenus != null && targetGo != null && componentIndex < componentMenus.Length && componentIndex >= 0)
            {
                Debug.Log("RefreshComponent");
                var cs = targetGo.GetComponents<Component>(); //Component不行
                var name = componentMenus[componentIndex];
                foreach(var t in cs)
                {
                    if(t.GetType().Name == name)
                    {
                        targetComponent = t;
                        break;
                    }
                }
            }
        }

        void RefreshTargetStaticInstance()
        {
            if(targetType != TargetType.StaticInstance || targetStaticInstancePath == null)
            {
                return;
            }

            Debug.Log("RefreshTargetStaticInstance");
            var paths = targetStaticInstancePath.Split(new char[]{'>'});
            targetStaticType = null;
            targetStaticInstance = null;
            var ar = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var assembly in ar)
            {
                var t = assembly.GetType(paths[0]);
                if(t != null)
                {
                    targetStaticType = t;
                    break;
                }
            }

            if(targetStaticType == null)
            {
                return;
            }

            if(paths.Length <= 1)
            {
                AddHistory(targetStaticInstancePath);
                return;
            }

            targetStaticInstance = InvodeFieldOrProperty(null, targetStaticType, paths[1]);
            for(int i = 2; i < paths.Length && targetStaticInstance != null; i++)
            {
                if(!string.IsNullOrEmpty(paths[i]))
                {
                    targetStaticInstance = InvodeFieldOrProperty(targetStaticInstance, targetStaticType, paths[1]);
                }
            }

            if (targetStaticInstance != null)
            {
                AddHistory(targetStaticInstancePath);
            }
        }

        public static object InvodeFieldOrProperty(object obj, Type type, string name)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic;
            if (obj == null)
            {
                flags |= BindingFlags.Static;
            }
            var fieldInfo = type.GetField(name, flags);
            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(obj);
            }


            var propertyInfo = type.GetProperty(name, flags);
            if(propertyInfo != null && propertyInfo.GetMethod != null)
            {
                return propertyInfo.GetMethod.Invoke(null, null);
            }

            return null;
        }

        public static bool IsDerivedFromUnityObject(Type type)
        {
            return type.IsSubclassOf(typeof(UnityEngine.Object));
        }

        public static bool IsUnityObject(Type type)
        {
            return type.Namespace != null && (type.IsSubclassOf(typeof(UnityEngine.Object)) && type.Namespace.StartsWith("Unity"));
        }

        private string FieldName(string str)
        {
            var index = str.LastIndexOf("k__BackingField");
            if(index > 0)
            {
                return str.Substring(1, index - 2);
            }
            return str;
        }

        private void AddHistory(string path)
        {
            if (!staticInstancePathHistory.Contains(path))
            {
                staticInstancePathHistory += path + "\n";
                SaveSettings();
            }
        }

        private string GetTransformPath(Transform trans)
        {
            var rt = trans.name;
            while(trans.parent != null)
            {
                trans = trans.parent;
                rt = trans.name + "/" + rt;
            }
            return rt;
        }

        private GameObject GetGameObjectByPath(string path)
        {
            if(string.IsNullOrEmpty(targetGoPath))
            {
                return null;
            }

            var i = targetGoPath.IndexOf('/');
            if(i < 0)
            {
                i = targetGoPath.Length;
            }

            Transform trans;
            var name = targetGoPath.Substring(0, i);
            var go = GameObject.Find(name);
            if(go == null)
            {
                return null;
            }

            if(i >= targetGoPath.Length - 1)
            {
                return go;
            }

            name = targetGoPath.Substring(i + 1, path.Length - i - 1);
            trans = go.transform.Find(name);
            return trans?.gameObject;
        }

        public static void GetMembers(object obj, Type type, string path, List<Member> list)
        {
            var flag = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            if (showUnityMembers || !IsDerivedFromUnityObject(type))
            {
                GetMembersInner(obj, type, path, list, flag);
            }
            else
            {
                flag |= BindingFlags.DeclaredOnly;
                while (type != typeof(System.Object) && type != typeof(System.ValueType) && !IsUnityObject(type))
                {
                    GetMembersInner(obj, type, path, list, flag);
                    type = type.BaseType;
                }
            }
            
            list.Sort((a, b) =>
            {
                if (a.IsStatic ^ b.IsStatic)
                {
                    return a.IsStatic & !b.IsStatic ? -1 : 1;
                }

                if (a.IsField ^ b.IsField)
                {
                    return a.IsField & !b.IsField ? -1 : 1;
                }

                return String.Compare(a.Name, b.Name, StringComparison.Ordinal);
            });
        }

        private static void GetMembersInner(object obj, Type type, string path, List<Member> list, BindingFlags flag)
        {
            var fields = type.GetFields(flag);
            foreach (var field in fields)
            {
                if (!field.Name.EndsWith("k__BackingField") && field.GetCustomAttribute(typeof(ObsoleteAttribute)) == null && (field.IsStatic || obj != null))
                {
                    var m = fieldMemberPool.Get();
                    m.Init(obj, type, field, path + field.Name);
                    list.Add(m);
                }
            }

            var properties = type.GetProperties(flag);
            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.GetIndexParameters().Length == 0 &&
                    propertyInfo.GetCustomAttribute(typeof(ObsoleteAttribute)) == null &&
                    ((propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsStatic ||
                      propertyInfo.SetMethod != null && propertyInfo.SetMethod.IsStatic) ||
                     obj != null))
                {
                    var m = propertyMemberPool.Get();
                    m.Init(obj, type, propertyInfo, path + propertyInfo.Name);
                    list.Add(m);
                }
            }
        }
    }
}
