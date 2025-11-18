using System;
using System.Reflection;

namespace ObjectExplorer
{
    public class PropertyMember : Member
    {
        private object obj;
        private Type objType;
        private PropertyInfo propertyInfo;

        public override bool IsProperty => true;
        public MethodInfo GetMethod { get; private set; }
        public MethodInfo SetMethod { get; private set; }

        public override bool CanGet => GetMethod != null;
        public override bool CanSet => SetMethod != null;

        public void Init(object obj, Type objType, PropertyInfo propertyInfo, string path)
        {
            base.Init(path);
            this.obj = obj;
            this.objType = objType;
            this.propertyInfo = propertyInfo;
            valueType = propertyInfo.PropertyType;
            Name = propertyInfo.Name;
            GetMethod = propertyInfo.GetMethod;
            SetMethod = propertyInfo.SetMethod;
            IsStatic = GetMethod != null ? GetMethod.IsStatic : SetMethod.IsStatic;
        }

        public override void Recycle()
        {
            obj = null;
            objType = null;
            propertyInfo = null;
            valueType = null;
            GetMethod = null;
            SetMethod = null;
            ObjectExplorer.propertyMemberPool.Recycle(this);
        }

        public override object GetValue()
        {
            return propertyInfo.GetValue(obj);
        }

        public override void SetValue(object obj)
        {
            propertyInfo.SetValue(this.obj, obj);
        }
    }
}