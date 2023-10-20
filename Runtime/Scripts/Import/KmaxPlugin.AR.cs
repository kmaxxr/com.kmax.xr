using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace KmaxXR
{
    public static partial class KmaxPlugin
    {
        const string ARDllName = "KMaxUnity";
        [DllImport(ARDllName, CallingConvention = CallingConvention.StdCall)]
        internal static extern int SendARTexture(System.IntPtr texture, int width, int height);
        [DllImport(ARDllName)]
        internal static extern int StartAR();
        [DllImport(ARDllName)]
        internal static extern int CloseAR();
    }
}
