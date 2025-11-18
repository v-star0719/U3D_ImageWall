using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectExplorer
{
    public class DictionaryMember : Member
    {
        private IDictionary iDict;
        private object key;

        public override bool IsListItem => true;

        public void Init(IDictionary iDict, int key, Type valueType, string path)
        {
            base.Init(path);
            this.key = key;
            this.iDict = iDict;
            this.valueType = valueType;
            Name = key.ToString();
            IsStatic = false;
        }

        public override void Recycle()
        {
            iDict = null;
            valueType = null;
            ObjectExplorer.dictMemberPool.Recycle(this);
        }

        public override object GetValue()
        {
            return iDict[key];
        }

        public override void SetValue(object obj)
        {
            iDict[key] = obj;
        }
    }
}