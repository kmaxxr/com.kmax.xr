using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KmaxXR
{
    public class KmaxVR : MonoBehaviour
    {

        private static KmaxVR kmaxVR;
        public static KmaxVR Instance
        {
            get
            {
                if (kmaxVR == null)
                    kmaxVR = GameObject.FindObjectOfType<KmaxVR>();

                return kmaxVR;
            }
        }

        private Vector2 windowSize;
        private Vector2 screenSize, screenOffset;
        private float displayBorderBottom = 0f;
        public Vector2 ScreenSize
        {
            get
            {
                if (screenSize == Vector2.zero)
                {
                    KmaxPlugin.GetScreenPhysical(ref screenOffset.x, ref screenOffset.y, ref screenSize.x, ref screenSize.y);
                }
                return screenSize;
            }
        }
        public Vector3 ScreenCenter => transform.position + transform.up * screenOffset.y;
        private delegate void CSharpDelegate();
        CSharpDelegate csharpDelegate;

        private Transform visualScreen;
        /// <summary>
        /// 虚拟屏幕
        /// </summary>
        public Transform VisualScreen
        {
            get
            {
                if (visualScreen == null)
                {
                    var go = new GameObject(nameof(visualScreen));
                    visualScreen = go.transform;
                    visualScreen.SetParent(transform);
                    visualScreen.localPosition = screenOffset;
                    visualScreen.localRotation = Quaternion.identity;
                }
                return visualScreen;
            }
        }
        public KmaxTracker kmaxTracker;
        [Range(0, 0.08f)]
        private float Ipd;
        [SerializeField]
        private bool syncOrientation = true;
        public bool EnableOrientation { get => syncOrientation; set => syncOrientation = value; }

        private void Start()
        {
#if UNITY_2020_1_OR_NEWER && !UNITY_EDITOR
            KmaxPlugin.ActiveLog();
            OpenStereoScopic();
#endif
            KmaxPlugin.InitXR();
            if (kmaxTracker == null) kmaxTracker = FindObjectOfType<KmaxTracker>();
            var ss = ScreenSize;
            if (syncOrientation) SetOrientation(KmaxPlugin.GetOrientation());
            GlobalEntity.Instance.Dispatch(KmaxVREvent.ScreenSize, ScreenSize);
        }

        private void OnValidate()
        {
            KmaxPlugin.ActiveLog();
            var ss = ScreenSize;
        }

        void Update()
        {
            var anglex = KmaxPlugin.GetOrientation();
            if (syncOrientation && Mathf.Abs(VisualPC_X - anglex) > 0.1f)
            {
                SetOrientation(anglex, 0.1f);
            }
        }

        private float VisualPC_X = 0;
        void SetOrientation(float anglex, float t = 1f)
        {
            VisualPC_X = Mathf.Lerp(VisualPC_X, anglex, t);
            transform.localEulerAngles = Vector3.right * VisualPC_X;
            GlobalEntity.Instance.Dispatch(KmaxVREvent.PoseChanged, new UnityEngine.Pose(ScreenCenter, transform.rotation));
        }

        public bool ShowGizmos = true;
        private Color ViewSreenBorder = Color.green;
        private Color ViewWindowBorder = Color.gray;
        private readonly Vector3[] corners = new Vector3[4];
        private readonly Vector3[] cor_near = new Vector3[4];
        private readonly Vector3[] cor_far = new Vector3[4];
        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (!ShowGizmos) return;
            SetKmaxMatrix();

            KmaxRect rect = KmaxPlugin.GetViewRect(ViewArea.Window);
            // Handles.color = ViewWindowBorder;
            // Handles.DrawPolyLine(rect.lt.ToVector3(), rect.rt.ToVector3(), rect.rb.ToVector3(), rect.lb.ToVector3(), rect.lt.ToVector3());

            rect = KmaxPlugin.GetViewRect(ViewArea.Screen);
            // Handles.color = ViewSreenBorder;
            // Handles.DrawPolyLine(rect.lt.ToVector3(), rect.rt.ToVector3(), rect.rb.ToVector3(), rect.lb.ToVector3(), rect.lt.ToVector3());

            corners[0].Set(-screenSize.x / 2, screenSize.y / 2, 0);
            corners[1].Set(-screenSize.x / 2, -screenSize.y / 2, 0);
            corners[2].Set(screenSize.x / 2, -screenSize.y / 2, 0);
            corners[3].Set(screenSize.x / 2, screenSize.y / 2, 0);
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] += (Vector3)(screenOffset);
            }
            Handles.matrix = transform.localToWorldMatrix;
            Handles.DrawSolidRectangleWithOutline(corners, Color.clear, ViewSreenBorder);
            kmaxTracker.GetFrustumCorners(-0.13f, cor_near);
            kmaxTracker.GetFrustumCorners(0.3f, cor_far);
            void DrawComfortZone(Vector3[] startCorners, Vector3[] endCorners)
            {
                var lineColor = ViewWindowBorder;
                Handles.DrawSolidRectangleWithOutline(startCorners, Color.clear, lineColor);
                Handles.DrawSolidRectangleWithOutline(endCorners, Color.clear, lineColor);
                Handles.color = lineColor;
                for (int i = 0; i < startCorners.Length; ++i)
                {
                    Handles.DrawLine(startCorners[i], endCorners[i]);
                }
            }
            DrawComfortZone(cor_near, cor_far);
#endif

        }

        /// <summary>
        /// 打开立体显示
        /// </summary>
        public void OpenStereoScopic()
        {
            var gameHwnd = KmaxPlugin.GetFocus();
            UpdateWnd();
            if (csharpDelegate == null) csharpDelegate = new CSharpDelegate(UpdateWnd);
            KmaxPlugin.CallBackFromUnity(Marshal.GetFunctionPointerForDelegate(csharpDelegate));
            if (KmaxPlugin.OpenStereoDisplay(gameHwnd) == KmaxPlugin.RESULT_SUCCESS)
                StartCoroutine("CallPluginAtEndOfFrames");
            else
                Debug.LogError("OpenStereoScopic Failed");
        }

        public void SetKmaxMatrix()
        {
            KmaxMatrix km = new KmaxMatrix(this.transform.localToWorldMatrix);
            KmaxPlugin.SetKmaxMatrix(km);
        }

        private void UpdateWnd()
        {
            float width = 0;
            float height = 0;
            KmaxPlugin.GetGameWindowSize(ref width, ref height);
            windowSize = new Vector2(width, height);
            if (kmaxTracker.ChangeRT(width, height))
                KmaxPlugin.SetTextureFromUnity(kmaxTracker.RenderTexture_l.GetNativeTexturePtr(), kmaxTracker.RenderTexture_r.GetNativeTexturePtr());
        }


        private IEnumerator CallPluginAtEndOfFrames()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                GL.IssuePluginEvent(KmaxPlugin.GetRenderEventFunc(), 1);
            }
        }

        private void OnDestroy()
        {
            KmaxPlugin.UninitXR();
            csharpDelegate = null;
            KmaxPlugin.ResetTracker();
        }

    }

    public enum KmaxVREvent { PoseChanged, ScreenSize }

}
