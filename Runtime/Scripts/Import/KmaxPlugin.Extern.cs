using System.Runtime.InteropServices;
using UnityEngine;

namespace KmaxXR
{
    public static partial class KmaxPlugin
    {
        public delegate void LogCallBack(int level, System.IntPtr msg, int len);
        internal static LogCallBack logger = new LogCallBack(LogFunc);
        private static void LogFunc(int level, System.IntPtr msg, int len)
        {
            string format = $"<color=green>KmaxXR</color> {Marshal.PtrToStringAnsi(msg, len)}";
            switch (level)
            {
                case 0:
                    Debug.Log(format);
                    break;
                case 1:
                    Debug.LogWarning(format);
                    break;
                case 2:
                    Debug.LogError(format);
                    break;
                default:
                    break;
            }
        }

        public static void ActiveLog(bool enable = true)
        {
            SetLogger(enable ? logger : null);
        }

        public static KmaxRect GetViewRect(ViewArea type)
        {
            KmaxVector3 lt;
            KmaxVector3 rt;
            KmaxVector3 lb;
            KmaxVector3 rb;
            KmaxPlugin.GetViewWindow(type, out lt, out rt, out lb, out rb);
            return new KmaxRect(lt, lb, rt, rb);
        }
    }
}

