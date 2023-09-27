using System.Runtime.InteropServices;
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
            if (q.w == 0) return 0;
            return Vector3.Angle(q * Vector3.up, Vector3.up);
        }

        /// <summary>
        /// 获取刚体姿态
        /// </summary>
        /// <param name="id">索引</param>
        /// <param name="pose">姿态</param>
        /// <returns>是否存在目标刚体</returns>
        public static bool GetTrackerPose(int id, out UnityEngine.Pose pose)
        {
            KmaxVector3 pos;
            KmaxVector4 rot;
            int ret = GetTrackerPose(id, out pos, out rot);
            pose.position = pos.ToVector3();
            pose.rotation = rot.ToQuaternion();
            return ret != 0;
        }

        public const int RESULT_SUCCESS = 0;

        internal static void SetTextureFormat(RenderTextureFormat format)
        {
            uint formatU = GetDXGIFormatForRenderTextureFormat(format, false);
            SetTextureFormat(formatU);
        }

        public static uint GetDXGIFormatForRenderTextureFormat(
            RenderTextureFormat renderTextureFormat, bool isSRGBFormat)
        {
            // If the DXGI format corresponding to a Unity render texture
            // format is not know, the returned DXGI format will be 0, which
            // corresponds to DXGI_FORMAT_UNKNOWN.
            uint dxgiFormat = 0u;

            switch (renderTextureFormat)
            {
                case RenderTextureFormat.ARGB32:
                    dxgiFormat =
                        isSRGBFormat ?
                            // DXGI_FORMAT_R8G8B8A8_UNORM_SRGB
                            29u :
                            // DXGI_FORMAT_R8G8B8A8_UNORM
                            28u;
                    break;

                case RenderTextureFormat.DefaultHDR:
                    dxgiFormat =
                            // DXGI_FORMAT_R16G16B16A16_FLOAT
                            10u;
                    break;

                case RenderTextureFormat.ARGB64:
                    dxgiFormat =
                        isSRGBFormat ?
                            // No corresponding sRGB DXGI format. Use
                            // DXGI_FORMAT_UNKNOWN.
                            0u :
                            // DXGI_FORMAT_R16G16B16A16_UNORM
                            11u;
                    break;
            }

            return dxgiFormat;
        }

    }
}

