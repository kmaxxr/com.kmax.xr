using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KmaxXR
{
    /// <summary>
    /// 姿态变化通知源
    /// </summary>
    public class PoseChangedNotify
    {
        public const string DisplayOriention = "DisplayOriention";
        public delegate void PoseChangedEvent(UnityEngine.Pose pose);
        private event PoseChangedEvent OnPoseChanged;
        private string name;
        private PoseChangedNotify(string id)
        {
            name = id;
        }
        public void Notify(UnityEngine.Pose pose) => OnPoseChanged?.Invoke(pose);
        public void Destroy() => RemoveNotify(name);

        private static Dictionary<string, PoseChangedNotify> notifiers = new Dictionary<string, PoseChangedNotify>();
        public static PoseChangedNotify AddNotify(string name)
        {
            if (notifiers.ContainsKey(name))
            {
                Debug.LogError($"Already exist a PoseChangedNotify named {name}");
                return null;
            }
            return notifiers[name] = new PoseChangedNotify(name);
        }

        private static void RemoveNotify(string name)
        {
            if (notifiers.ContainsKey(name))
            {
                notifiers.Remove(name);
            }
        }

        public static bool Subscribe(string name, PoseChangedEvent e)
        {
            PoseChangedNotify notify;
            if (notifiers.TryGetValue(name, out notify))
            {
                notify.OnPoseChanged += e;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Unsubscribe(string name, PoseChangedEvent e)
        {
            PoseChangedNotify notify;
            if (notifiers.TryGetValue(name, out notify))
            {
                notify.OnPoseChanged -= e;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

}