using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectExplorer
{
    public class ListMember : Member
    {
        private IList iList;
        private int index;

        public override bool IsListItem => true;

        public void Init(IList iList, int index, Type valueType, string path)
        {
            base.Init(path);
            this.index = index;
            this.iList = iList;
            this.valueType = valueType;
            Name = index.ToString();
            IsStatic = false;
        }

        public override void Recycle()
        {
            iList = null;
            valueType = null;
            ObjectExplorer.listMemberPool.Recycle(this);
        }

        public override object GetValue()
        {
            return iList[index];
        }

        public override void SetValue(object obj)
        {
            iList[index] = obj;
        }
    }
}