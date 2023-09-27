using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Reflection;
using System;

namespace KmaxXR
{

    public static partial class KmaxPlugin
    {
        const string dllName = "KMaxUnity";

        [DllImport(dllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern void SetTimeFromUnity(float t);

        [DllImport(dllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern int SetTextureFromUnity(System.IntPtr texture_l, System.IntPtr texture_r);

        [DllImport(dllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern void SetTextureFormat(uint format);

        [DllImport(dllName)]
        internal static extern IntPtr GetRenderEventFunc();

        [DllImport(dllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern int OpenStereoDisplay(IntPtr hwnd);
        [DllImport(dllName)]
        internal static extern int InitXR();
        [DllImport(dllName)]
        internal static extern int UninitXR();
        [DllImport(dllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern int GetGameWindowSize(ref float width, ref float height);

        [DllImport(dllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern void CallBackFromUnity(IntPtr callback);
        [DllImport(dllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern void GetProjection(KmaxMatrix camM, out KmaxMatrix km);
        [DllImport(dllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern void SetIpd(float ipd);
        [DllImport(dllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern void SetCameraParameter(float zNear, float zFar);
        [DllImport(dllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern void SetKmaxMatrix(KmaxMatrix km);
        [DllImport(dllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern void GetScreenPhysical(ref float x, ref float y, ref float width, ref float height);
        [DllImport(dllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern void GetViewWindow(ViewArea type,out KmaxVector3 lt, out KmaxVector3 rt, out KmaxVector3 lb, out KmaxVector3 rb);
       
        [DllImport(dllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern int SendARTexture(IntPtr texture, int width, int height);
        [DllImport(dllName)]
        internal static extern int StartAR();
        [DllImport(dllName)]
        internal static extern int CloseAR();
        [DllImport(dllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern void TrackerHandle(float detaTime);
        [DllImport(dllName)]
        internal static extern void GetEyePose(StereoscopicEye eye,out KmaxVector3 pos, out KmaxVector4 rot);
        [DllImport(dllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern void GetPenPose(out KmaxVector3 pos, out KmaxVector4 rot);
        [DllImport(dllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern int GetTrackerPose(int id, out KmaxVector3 pos, out KmaxVector4 rot);
        [DllImport(dllName)]
        internal static extern void ResetTracker();
        [DllImport(dllName, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetLogger(LogCallBack logger);
        [DllImport(dllName)]
        public static extern bool Pen_Connected();
        [DllImport(dllName)]
        internal static extern bool Pen_Open();
        [DllImport(dllName)]
        internal static extern bool Pen_Close();
        [DllImport(dllName)]
        public static extern bool Pen_GetButtonDown(int id);
        [DllImport(dllName)]
        internal static extern void Pen_Shake(int time, int value);
        [DllImport(dllName)]
        internal static extern void Pen_Light(int time, int value);
        [DllImport(dllName)]
        internal static extern float Pen_GetGyroValue1(uint id);
        [DllImport("user32.dll")]
        internal static extern IntPtr GetFocus();
        
    }
}
