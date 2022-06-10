﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KmaxXR
{
    // [ExecuteAlways]
    public class KmaxTracker : MonoBehaviour
    {
        [SerializeField]
        private Camera[] cams = new Camera[3];

        [SerializeField]
        private Transform pen;


        private Transform viewAnchor;
        private KmaxMatrix kmaxMatrix;
        private float VisualPC_X = 0;
        private PoseChangedNotify poseChanged;

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

        void Awake()
        {
            poseChanged = PoseChangedNotify.AddNotify(PoseChangedNotify.DisplayOriention);
        }

        // Start is called before the first frame update
        void Start()
        {
            viewAnchor = KmaxVR.Instance.View;
            kmaxMatrix = new KmaxMatrix(Matrix4x4.zero);
        }

        void OnDestroy()
        {
            poseChanged?.Destroy();
            poseChanged = null;
        }

        bool validate => cams.Length >= 3 && pen != null;

        void Update()
        {
#if UNITY_EDITOR
            if (!validate) return;
            KmaxVR.Instance.SetKmaxMatrix();
#endif
            Tracker();
#if UNITY_EDITOR

            cams[0].projectionMatrix = UpdateProjection(cams[0]);
            cams[1].projectionMatrix = UpdateProjection(cams[1]);
            cams[2].projectionMatrix = UpdateProjection(cams[2]);
            cams[1].Render();
            cams[2].Render();

#endif

        }

        void Tracker()
        {

            KmaxVector3 pos;
            KmaxVector4 rot;
            KmaxPlugin.TrackerHandle(Time.deltaTime);

            KmaxPlugin.GetEyePose(StereoscopicEye.main, out pos, out rot);
            transform.localPosition = pos.ToVector3();
            transform.localRotation = rot.ToQuaternion();
            KmaxPlugin.GetEyePose(StereoscopicEye.left, out pos, out rot);
            cams[1].transform.localPosition = pos.ToVector3();
            cams[1].transform.localRotation = rot.ToQuaternion();
            KmaxPlugin.GetEyePose(StereoscopicEye.right, out pos, out rot);
            cams[2].transform.localPosition = pos.ToVector3();
            cams[2].transform.localRotation = rot.ToQuaternion();
            KmaxPlugin.GetPenPose(out pos, out rot);
            pen.transform.localPosition = pos.ToVector3();
            pen.transform.localRotation = rot.ToQuaternion();
            pen.transform.Rotate(Vector3.right * 90);

            Quaternion q = new Quaternion(
                KmaxPlugin.Pen_GetGyroValue1(2),
                -KmaxPlugin.Pen_GetGyroValue1(3),
                -KmaxPlugin.Pen_GetGyroValue1(1),
                KmaxPlugin.Pen_GetGyroValue1(0)
            );
            if (transform.parent != null && Mathf.Abs(VisualPC_X - q.eulerAngles.x) > 0.1f)
            {
                VisualPC_X = q.eulerAngles.x;
                viewAnchor.localEulerAngles = Vector3.right * (VisualPC_X - 360 + 90);
                poseChanged.Notify(new UnityEngine.Pose(viewAnchor.position, viewAnchor.rotation));
            }
        }

        private void OnPreRender()
        {

#if !UNITY_EDITOR
            KmaxVR.Instance.SetKmaxMatrix();
            cams[0].SetStereoViewMatrix(Camera.StereoscopicEye.Left, cams[1].worldToCameraMatrix);
            cams[0].SetStereoViewMatrix(Camera.StereoscopicEye.Right, cams[2].worldToCameraMatrix);
            cams[0].SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, UpdateProjection(cams[1]));
            cams[0].SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, UpdateProjection(cams[2]));
#endif

        }

        Matrix4x4 UpdateProjection(Camera cam)
        {
            KmaxMatrix camM = new KmaxMatrix(cam.worldToCameraMatrix);
            KmaxPlugin.GetProjection(camM, out kmaxMatrix);

            return kmaxMatrix.ToMatrix4x4();
        }


        public bool ChangeRT(float width, float height)
        {
            if (width <= 0 || height <= 0) return false;
            renderTexture_l = new RenderTexture((int)width, (int)height, 16, RenderTextureFormat.ARGB32);
            cams[1].targetTexture = renderTexture_l;
            renderTexture_r = new RenderTexture((int)width, (int)height, 16, RenderTextureFormat.ARGB32);
            cams[2].targetTexture = renderTexture_r;
            return true;
        }


        private void OnDrawGizmos()
        {
            if (viewAnchor != null)
            {
                float dist = Vector3.Dot(transform.forward, (viewAnchor.transform.position - transform.position));
                Debug.DrawLine(transform.position, transform.position + transform.forward * dist, Color.yellow);
            }

        }
    }
}