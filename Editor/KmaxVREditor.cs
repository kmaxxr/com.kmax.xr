
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
namespace KmaxXR
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(KmaxVR))]
    public class KmaxVREditor : Editor
    {
        public const int ViewWidth = 1920;
        public const int ViewHeight = 1080;
        private KmaxVR script;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (script == null)
                script = target as KmaxVR;
            DrawDefaultInspector();

            GUI.skin.GetStyle("HelpBox").richText = true;
            if (VRInEditor.Enable) EditorGUILayout.HelpBox("<size=12>Deselect Menu \"<color=green>Kmax -> Enable Stereo Display</color>\" to close Stereo Display</size>", MessageType.Info);
            else EditorGUILayout.HelpBox("<size=12>Select Menu \"<color=green>Kmax -> Enable Stereo Display</color>\" to open Stereo Display</size>", MessageType.Info);
            string v3 = script.ScreenCenter.ToString("F3");
            EditorGUILayout.HelpBox($"Screen Center: {v3}", MessageType.None);
            CheckPlatform();
            CheckIsStereo();

            serializedObject.ApplyModifiedProperties();
        }

        void OnSceneGUI()
        {
            if (script == null) return;
            var transform = script.transform;
            float len = 0.05f;
            string label = script.ScreenSize.ToString("F3");
            Handles.Label(transform.position, $"{nameof(KmaxVR)}\n{label}");
            Handles.DrawLine(transform.position, transform.position + transform.up * len);
            var apos = transform.position + Vector3.forward * len;
            Handles.DrawLine(transform.position, apos);
            var angle = 90 - transform.eulerAngles.x;
            Handles.Label(apos, angle.ToString("F2"));
            Handles.DrawWireArc(transform.position, transform.right,
                transform.up, angle, len);
        }

        private void CheckPlatform()
        {
            ApiCompatibilityLevel api = PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone);
            if (api != ApiCompatibilityLevel.NET_4_6)
            {
                EditorGUILayout.HelpBox("<b><color=#ff0000ff><size=15>" +
                   "Build Settings Error,please perform the following steps:</size></color></b>\n\n" +
                   "<color=#00ffffff><b>1.</b> Go to Edit > Project Settings... > Player > Other Settings\n\n" +
                   "<b>2.</b> Api CompatibilityLevel*\n\n" +
                   "<b>3.</b> Architecture Switch .NET 4.x\n\n" +
                   "</color>", MessageType.Warning);
            }
        }
        private void CheckIsStereo()
        {
#if UNITY_2019
            string[] vrSdks = PlayerSettings.GetVirtualRealitySDKs(
                BuildTargetGroup.Standalone);

            // Check if the Desktop Stereo virtual reality SDK is enabled.
            if (!PlayerSettings.virtualRealitySupported || !vrSdks.Contains("stereo"))
            {
                EditorGUILayout.HelpBox("<b><color=#ff0000ff><size=15>" +
                    "Error: To enable stereoscopic 3D,please perform the following steps:</size></color></b>\n\n" +
                    "<color=#00ffffff><b>1.</b> Go to Edit > Project Settings... > Player > " +
                    "XR Settings\n\n" +
                    "<b>2.</b> Enable Virtual Reality Supported\n\n" +
                    "<b>3.</b> Add Stereo Display (non " +
                    "head-mounted) to the list of Virtual Reality " +
                    "SDKs</color>", MessageType.Warning);
            }
#endif
        }

    }

    [InitializeOnLoad]
    public static class VRInEditor
    {
        static VRInEditor()
        {
            EditorApplication.playModeStateChanged += PlayModeStateHandler;
            KmaxPlugin.ActiveLog();
        }

        private static void PlayModeStateHandler(PlayModeStateChange state)
        {
            // Debug.Log($"VRInEditor.PlayModeStateHandler {state}");
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    Enable = true;
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    Enable = false;
                    break;
                default: break;
            }
        }

        public static bool Enable
        {
            get { return EditorPrefs.GetBool(KmaxMenu.XR_DISPLAY_ENABLE); }
            set
            {
                if (!EditorPrefs.GetBool(KmaxMenu.XR_DISPLAY_ENABLE)) return;
                if (value) OpenStereoDisplay();
                else CloseStereoDisplay();
            }
        }

        private static void OpenStereoDisplay()
        {
            Assembly assembly = typeof(EditorWindow).Assembly;
            var type = assembly.GetType("UnityEditor.GameView");
            var window = EditorWindow.GetWindow(type, true);
            window.Focus();
            window.position = new Rect(0, 0, KmaxVREditor.ViewWidth, KmaxVREditor.ViewHeight);

            if (Application.isPlaying)
            {
                KmaxVR.Instance.OpenStereoScopic();
            }

        }

        private static void CloseStereoDisplay()
        {

        }
    }
}
