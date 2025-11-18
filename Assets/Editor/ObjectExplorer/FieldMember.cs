using System;
using System.Reflection;
using UnityEngine;

namespace ObjectExplorer
{
    public class FieldMember : Member
    {
        private object obj;
        private Type objType;
        private FieldInfo fieldInfo;

        public override bool IsField => true;

        public void Init(object obj, Type objType, FieldInfo fieldInfo, string path)
        {
            base.Init(path);
            this.obj = obj;
            this.objType = objType;
            this.fieldInfo = fieldInfo;
            valueType = fieldInfo.FieldType;
            Name = fieldInfo.Name;
            IsStatic = fieldInfo.IsStatic;
            IsMonoBehaviour = objType.IsSubclassOf(typeof(MonoBehaviour)) && typeof(MonoBehaviour).GetField(fieldInfo.Name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) != null;
        }

        public override void Recycle()
        {
            obj = null;
            objType = null;
            fieldInfo = null;
            valueType = null;
            ObjectExplorer.fieldMemberPool.Recycle(this);
        }

        public override object GetValue()
        {
            return fieldInfo.GetValue(obj);
        }

        public override void SetValue(object obj)
        {
            fieldInfo.SetValue(this.obj, obj);
        }
    }
}