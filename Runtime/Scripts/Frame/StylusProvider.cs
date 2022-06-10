using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KmaxXR
{
    public abstract class StylusProvider : ScriptableObject, IStylusButtonProvider
    {
        public const string MenuPrefix = "Kmax/Stylus/";
        public virtual bool GetKey(StylusKey key)
        {
            return false;
        }

        public virtual bool GetKeyDown(StylusKey key)
        {
            return false;
        }

        public virtual bool GetKeyUp(StylusKey key)
        {
            return false;
        }

        public virtual void Open() {}
        public virtual void Process() {}
        public virtual void Close() {}
    }
}
