﻿using System.Runtime.InteropServices;
using UnityEngine;

namespace KmaxXR
{
    public enum PenShakePower { Weak = 1, Medium, Strong }
    public enum PenLightColor { R = 1, G, B }
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

        /// <summary>
        /// 使笔震动
        /// </summary>
        /// <param name="time">时长，单位为秒</param>
        /// <param name="power">力度，可以是弱、中、强</param>
        public static void PenShake(float time, PenShakePower power)
        {
            int t = Mathf.FloorToInt(time * 10);
            int v = (int)power;
            Pen_Shake(t, v);
        }

        /// <summary>
        /// 使笔亮灯
        /// </summary>
        /// <param name="time">时长，单位为秒</param>
        /// <param name="c">颜色，可以是红、绿、蓝</param>
        public static void PenLight(float time, PenLightColor c)
        {
            int t = Mathf.FloorToInt(time);
            int v = (int)c;
            Pen_Light(t, v);
        }

        /// <summary>
        /// 获取机身旋转角度
        /// </summary>
        /// <returns>角度</returns>
        public static float GetOrientation()
        {
            Quaternion q = new Quaternion(
                KmaxPlugin.Pen_GetGyroValue1(2),
                -KmaxPlugin.Pen_GetGyroValue1(3),
                -KmaxPlugin.Pen_GetGyroValue1(1),
                KmaxPlugin.Pen_GetGyroValue1(0)
            );
            if (q.w == 0) return 270;
            return q.eulerAngles.x;
        }

    }
}

