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

        public static event System.Action<UnityEngine.Pose> OrientationChanged;

        public Transform View => transform;
        public KmaxTracker kmaxTracker;
        [Range(0, 0.08f)]
        private float Ipd;
        [SerializeField]
        private bool syncOrientation = true;
        public bool EnableOrientation { get => syncOrientation; set => syncOrientation = value; }

        private void Start()
        {
            KmaxPlugin.InitXR();
            if (kmaxTracker == null) kmaxTracker = FindObjectOfType<KmaxTracker>();
            var ss = ScreenSize;
            if (syncOrientation) SetOrientation(KmaxPlugin.GetOrientation());
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
            transform.localEulerAngles = Vector3.right * (VisualPC_X - 360 + 90);
            OrientationChanged?.Invoke(new UnityEngine.Pose(ScreenCenter, transform.rotation));
        }

        public bool ShowGizmos = true;
        private Color ViewSreenBorder = Color.green;
        private Color ViewWindowBorder = Color.gray;
        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (!ShowGizmos) return;
            SetKmaxMatrix();

            KmaxRect rect = KmaxPlugin.GetViewRect(ViewArea.Window);
            Handles.color = ViewWindowBorder;
            Handles.DrawPolyLine(rect.lt.ToVector3(), rect.rt.ToVector3(), rect.rb.ToVector3(), rect.lb.ToVector3(), rect.lt.ToVector3());

            rect = KmaxPlugin.GetViewRect(ViewArea.Screen);
            Handles.color = ViewSreenBorder;
            Handles.DrawPolyLine(rect.lt.ToVector3(), rect.rt.ToVector3(), rect.rb.ToVector3(), rect.lb.ToVector3(), rect.lt.ToVector3());

            // rect = KmaxPlugin.GetViewRect(ViewArea.DisplayBorder);
            // Gizmos.DrawLine(rect.lt.ToVector3(), rect.rt.ToVector3());
            // Gizmos.DrawLine(rect.rt.ToVector3(), rect.rb.ToVector3());
            // Gizmos.DrawLine(rect.rb.ToVector3(), rect.lb.ToVector3());
            // Gizmos.DrawLine(rect.lb.ToVector3(), rect.lt.ToVector3());

            Vector3 lt, rt, lb, rb;
            lt = new Vector3(-screenSize.x / 2, screenSize.y / 2, 0);
            rt = new Vector3(screenSize.x / 2, screenSize.y / 2, 0);
            lb = new Vector3(-screenSize.x / 2, -screenSize.y / 2, 0);
            rb = new Vector3(screenSize.x / 2, -screenSize.y / 2, 0);
            // lt += (Vector3)(screenOffset);
            // rt += (Vector3)(screenOffset);
            // lb += (Vector3)(screenOffset);
            // rb += (Vector3)(screenOffset);
            // lt = transform.localToWorldMatrix.MultiplyPoint(lt);
            // rt = transform.localToWorldMatrix.MultiplyPoint(rt);
            // lb = transform.localToWorldMatrix.MultiplyPoint(lb);
            // rb = transform.localToWorldMatrix.MultiplyPoint(rb);
            // Handles.color = Color.red;
            // Handles.DrawPolyLine(lt, rt, rb, lb, lt);
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
            KmaxPlugin.OpenStereoDisplay(gameHwnd);
            StartCoroutine("CallPluginAtEndOfFrames");
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

}
