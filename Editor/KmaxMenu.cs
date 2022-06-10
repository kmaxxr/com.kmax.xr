using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;
using System.Reflection;
using UnityEngine.Rendering;

namespace KmaxXR
{

    public class KmaxMenu
    {
        public const string COMPANY_NAME = "Kmax";
        public const string XR_DISPLAY_ENABLE = "EnableStereoDisplay";
        public const string MENU_XR_ENABLE = COMPANY_NAME + "/Enable Stereo Display";
        const string GAMEOBJECT_EXT = "GameObject/" + COMPANY_NAME;

        [MenuItem(MENU_XR_ENABLE)]
        static void ToggleStereoDisplay()
        {
            bool enable = EditorPrefs.GetBool(XR_DISPLAY_ENABLE);
            EditorPrefs.SetBool(XR_DISPLAY_ENABLE, !enable);
            Menu.SetChecked(MENU_XR_ENABLE, !enable);
        }

        [MenuItem(MENU_XR_ENABLE, true)]
        static bool StereoDisplayValid()
        {
#if UNITY_EDITOR_WIN
            bool valid = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11;
            bool enable = EditorPrefs.GetBool(XR_DISPLAY_ENABLE);
            Menu.SetChecked(MENU_XR_ENABLE, enable && valid);
            return valid;
#else
            return false;
#endif
        }

        [MenuItem(COMPANY_NAME + "/Game View")]
        static void OpenGameView()
        {
            // Get existing open window or if none, make a new one:
            Assembly assembly = typeof(EditorWindow).Assembly;
            var type = assembly.GetType("UnityEditor.GameView");
            var window = EditorWindow.GetWindow(type);
            window.ShowUtility();
            window.position = new Rect(0, 0, 1920, 1080);
        }

        [MenuItem(GAMEOBJECT_EXT + "/Add KmaxVR", false, 10)]
        static void AddKmaxVR()
        {
            CreateGameObjectFromPrefab<KmaxVR>(nameof(KmaxVR));
        }

        [MenuItem(GAMEOBJECT_EXT + "/Add KmaxVR", true)]
        static bool AddKmaxVRValid()
        {
            return GameObject.FindObjectOfType<KmaxVR>() == null;
        }

        [MenuItem(GAMEOBJECT_EXT + "/Add KmaxAR", false, 11)]
        static void AddKmaxAR()
        {
            CreateGameObjectFromPrefab<KmaxAR>(nameof(KmaxAR));
        }

        [MenuItem(GAMEOBJECT_EXT + "/Add KmaxAR", true)]
        static bool AddKmaxARValid()
        {
            return GameObject.FindObjectOfType<KmaxAR>() == null;
        }

        [MenuItem(GAMEOBJECT_EXT + "/Convert to StylusInputModule", false)]
        static void ConvertToStylus()
        {
            var cs = Selection.activeGameObject.GetComponents<BaseInputModule>();
            foreach (var item in cs)
            {
                Undo.DestroyObjectImmediate(item);
            }
            Undo.AddComponent<StylusInputModule>(Selection.activeGameObject);
        }

        [MenuItem(GAMEOBJECT_EXT + "/Convert to StylusInputModule", true)]
        static bool ConvertToStylusValid()
        {
            return Selection.activeGameObject != null &&
                Selection.activeGameObject.GetComponent<EventSystem>() != null;
        }

        [MenuItem(GAMEOBJECT_EXT + "/Fix Canvas", false)]
        static void FixCanvas()
        {
            var fix = Selection.activeGameObject.GetComponent<UIFacade>();
            if (fix == null)
            {
                fix = Selection.activeGameObject.AddComponent<UIFacade>();
            }
            var vr = GameObject.FindObjectOfType<KmaxVR>();
            if (vr == null)
            {
                Debug.LogError("KmaxVR not found");
                return;
            }
            Undo.RegisterCompleteObjectUndo(Selection.activeTransform, "Fix Canvas");
            fix.FixSize(vr.ScreenSize);
            fix.FixPose(new UnityEngine.Pose(vr.ScreenCenter, vr.View.rotation));
            EditorUtility.SetDirty(Selection.activeTransform);
        }

        [MenuItem(GAMEOBJECT_EXT + "/Fix Canvas", true)]
        static bool FixCanvasValid()
        {
            if (Selection.activeGameObject == null) return false;
            var canvas = Selection.activeGameObject.GetComponent<Canvas>();
            if (canvas == null) return false;
            return true;
        }

        private static T CreateGameObject<T>(
            string name, bool setSelected = true, Transform parent = null)
            where T : Component
        {
            // Create the game object.
            GameObject gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent);
            gameObject.transform.SetAsLastSibling();

            // Register this operation with the Unity Editor's undo stack.
            Undo.RegisterCreatedObjectUndo(gameObject, $"Create {name}");

            // Determine whether to select the newly created Game Object
            // in the Unity Inspector window.
            if (setSelected)
            {
                Selection.activeGameObject = gameObject;
            }

            // Create the specified component.
            T component = gameObject.AddComponent<T>();

            return component;
        }

        const string PrefabAssetRelativePath = "Editor Resources";
        private static T CreateGameObjectFromPrefab<T>(
            string name, bool setSelected = true, Transform parent = null)
            where T : Component
        {
            // Attempt to find a reference to the prefab asset.
            GameObject prefab = FindAsset<GameObject>(
                $"{name} t:prefab", PrefabAssetRelativePath);

            if (prefab == null)
            {
                Debug.LogError(
                    $"Failed to create instance of {name}. " +
                    "Prefab not found.");

                return null;
            }

            // Create an instance of the prefab.
            GameObject gameObject = GameObject.Instantiate(prefab);
            gameObject.transform.SetParent(parent);
            gameObject.transform.SetAsLastSibling();
            gameObject.name = name;

            // Register the operation with the Unity Editor's undo stack.
            Undo.RegisterCreatedObjectUndo(gameObject, $"Create {name}");

            // Determine whether to select the newly created prefab instance
            // in the Unity Inspector window.
            if (setSelected)
            {
                Selection.activeGameObject = gameObject;
            }

            return gameObject.GetComponent<T>();
        }

        private static T FindAsset<T>(string filter, string relativePath = null)
            where T : Object
        {
            string[] guids = AssetDatabase.FindAssets(filter);

            for (int i = 0; i < guids.Length; ++i)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (string.IsNullOrEmpty(relativePath) ||
                    assetPath.Contains(relativePath))
                {
                    return (T)AssetDatabase.LoadAssetAtPath(assetPath, typeof(T));
                }
            }

            return null;
        }
    }

}