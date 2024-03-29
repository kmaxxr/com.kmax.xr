﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KmaxXR
{
    [ExecuteAlways]
    public class KmaxTracker : MonoBehaviour
    {
        [SerializeField]
        private Camera[] cams = new Camera[3];

        [SerializeField]
        private Transform pen;
        /// <summary>
        /// 超过3秒没有检测到眼部将触发眼部位置回归
        /// </summary>
        [SerializeField, Range(0, 60)]
        private float WaitForEyeScends = 3f;
        /// <summary>
        /// 眼部追踪丢失回归的速度
        /// </summary>
        [SerializeField, Range(0, 60)]
        private float missBackSpeed = 2f;

        private Transform viewAnchor;
        private KmaxMatrix kmaxMatrix;

        private RenderTexture renderTexture_r;
        public RenderTexture RenderTexture_r
        {
            get { return renderTexture_r; }
            set { renderTexture_r = value; }
        }

        private RenderTexture renderTexture_l;
        public RenderTexture RenderTexture_l
        {
            get { return renderTexture_l; }
            set { renderTexture_l = value; }
        }

        void Start()
        {
            kmaxMatrix = new KmaxMatrix(Matrix4x4.zero);
            // float width = 1920;
            // float height = 1080;
            // KmaxPlugin.GetGameWindowSize(ref width, ref height);
            // ChangeRT(width, height);
        }

        void OnDestroy()
        {

        }

        bool validate => cams.Length >= 3 && pen != null;


        void Update()
        {
#if UNITY_EDITOR
            if (!validate) return;
#endif
            UpdateProjection();
            Track();

            cams[0].projectionMatrix = GetProjection(cams[0]);
#if UNITY_EDITOR || UNITY_2020_1_OR_NEWER
            cams[1].projectionMatrix = GetProjection(cams[1]);
            cams[2].projectionMatrix = GetProjection(cams[2]);
            cams[1].Render();
            cams[2].Render();
#endif

        }

        float loseTimer = 0f;
        void Track()
        {
            // 设置相机参数
            KmaxPlugin.SetIpd(cams[0].stereoSeparation);
            KmaxPlugin.SetCameraParameter(cams[0].nearClipPlane, cams[0].farClipPlane);

            KmaxVector3 pos;
            KmaxVector4 rot;
            KmaxPlugin.TrackerHandle(Time.deltaTime);

            KmaxPlugin.GetEyePose(StereoscopicEye.main, out pos, out rot);
            cams[0].transform.localPosition = pos.ToVector3();
            if (KmaxPlugin.GetTrackerVisible(0))
            {
                KmaxPlugin.GetEyePose(StereoscopicEye.left, out pos, out rot);
                cams[1].transform.localPosition = pos.ToVector3();
                // cams[1].transform.localRotation = rot.ToQuaternion();
                KmaxPlugin.GetEyePose(StereoscopicEye.right, out pos, out rot);
                cams[2].transform.localPosition = pos.ToVector3();
                // cams[2].transform.localRotation = rot.ToQuaternion();
                loseTimer = WaitForEyeScends;
            }
            else if (loseTimer > 0)
            {
                loseTimer -= Time.deltaTime;
            }
            else
            {
                float f = Time.deltaTime * missBackSpeed;
                cams[1].transform.localPosition = Vector3.Lerp(
                    cams[1].transform.localPosition,
                    Vector3.zero, f);
                cams[2].transform.localPosition = Vector3.Lerp(
                    cams[2].transform.localPosition,
                    Vector3.zero, f);
            }
            KmaxPlugin.GetPenPose(out pos, out rot);
            pen.transform.localPosition = pos.ToVector3();
            pen.transform.localRotation = rot.ToQuaternion();

        }

        private void UpdateProjection()
        {
            KmaxVR.Instance.SetKmaxMatrix();
            cams[0].SetStereoViewMatrix(Camera.StereoscopicEye.Left, cams[1].worldToCameraMatrix);
            cams[0].SetStereoViewMatrix(Camera.StereoscopicEye.Right, cams[2].worldToCameraMatrix);
            cams[0].SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, GetProjection(cams[1]));
            cams[0].SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, GetProjection(cams[2]));
        }

        Matrix4x4 GetProjection(Camera cam)
        {
            KmaxMatrix camM = new KmaxMatrix(cam.worldToCameraMatrix);
            KmaxPlugin.GetProjection(camM, out kmaxMatrix);

            return kmaxMatrix.ToMatrix4x4();
        }

        public bool ChangeRT(float width, float height)
        {
            return ChangeRT(width, height, out var format);
        }

        public bool ChangeRT(float width, float height, out RenderTextureFormat format)
        {
            if (width <= 0 || height <= 0)
            {
                format = default;
                return false;
            }

            bool hdr = false;
            foreach (var item in cams)
            {
                hdr = hdr || item.allowHDR;
            }
            // Debug.Log($"HDR enable {hdr}");
            format = hdr ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.ARGB32;
            renderTexture_l = new RenderTexture((int)width, (int)height, 24, format);
            cams[1].targetTexture = renderTexture_l;
            renderTexture_r = new RenderTexture((int)width, (int)height, 24, format);
            cams[2].targetTexture = renderTexture_r;
            return true;
        }

        private readonly Rect Viewport = new Rect(0, 0, 1, 1);
        public void GetFrustumCorners(float offset, Vector3[] outVS)
        {
            float dis = cams[0].transform.localPosition.z;
            cams[0].CalculateFrustumCorners(
                Viewport,
                Mathf.Abs(dis) + offset,
                Camera.MonoOrStereoscopicEye.Mono,
                outVS);
            for (int i = 0; i < outVS.Length; i++)
            {
                outVS[i] += transform.localPosition;
            }
        }

        private void OnDrawGizmos()
        {
            if (viewAnchor != null)
            {
                float dist = Vector3.Dot(transform.forward, viewAnchor.transform.position - transform.position);
                Debug.DrawLine(transform.position, transform.position + transform.forward * dist, Color.yellow);
            }

        }
    }
}
