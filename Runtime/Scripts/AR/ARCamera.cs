using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace KmaxXR
{

    public class ARCamera : MonoBehaviour
    {
        private Material mat;
        private Transform window;
        private Camera cam;
        public Camera depthCam;
        public RenderTexture depthRt;
        public RenderTexture arRt;
        [SerializeField] Shader clipShader;
        [SerializeField] Shader depthShader;
        public float ImageW = 640;
        public float ImageH = 480;


        // Start is called before the first frame update
        void Start()
        {
            cam = this.GetComponent<Camera>();
            window = KmaxVR.Instance.VisualScreen;
            mat = new Material(clipShader);
        }

        private void OnPreRender()
        {
            depthCam.backgroundColor = Color.black;
            depthCam.clearFlags = CameraClearFlags.Color;
            depthCam.targetTexture = depthRt;
            depthCam.RenderWithShader(depthShader, string.Empty);

            KmaxRect rect = KmaxPlugin.GetViewRect(ViewArea.Window);
            mat.SetMatrix("_WindowMatrix", window.worldToLocalMatrix);
            mat.SetMatrix("_InvProjectionMatrix", cam.projectionMatrix.inverse);
            mat.SetTexture("_CustomDepthTexture", depthRt);
            mat.SetMatrix("_ViewToWorld", cam.cameraToWorldMatrix);
            mat.SetVector("_LeftTop_uv", GetUV(rect.lt.ToVector3()));
            mat.SetVector("_RightTop_uv", GetUV(rect.rt.ToVector3()));
            mat.SetVector("_LeftBottom_uv", GetUV(rect.lb.ToVector3()));
            mat.SetVector("_RightBottom_uv", GetUV(rect.rb.ToVector3()));
            mat.SetVector("_LeftTop", new Vector4(rect.lt.x, rect.lt.y, rect.lt.z, 1));
            mat.SetVector("_RightBottom", new Vector4(rect.rb.x, rect.rb.y, rect.rb.z, 1));

        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, mat);
        }

        public Vector4 GetUV(Vector3 pos)
        {
            Vector4 uv = Vector4.zero;
            Vector4 clip = cam.projectionMatrix * cam.worldToCameraMatrix * new Vector4(pos.x, pos.y, pos.z, 1);
            Vector3 ndc = new Vector3(clip.x / clip.w, clip.y / clip.w, clip.z / clip.w);
            uv = new Vector4(ndc.x * 0.5f + 0.5f, ndc.y * 0.5f + 0.5f, 1, 1);
            return uv;
        }

        public void PoseCallback(KmaxAR.Pose p)
        {
            double[] tvecArr = new double[3];
            double[] cam = new double[9];
            double[] dis = new double[5];
            double[] rotMatArr = new double[12];
            tvecArr = p.pos;
            rotMatArr = p.rot;
            cam = p.cam;
            dis = p.dist;
            Matrix4x4 camP = CalculateProjectionMatrixFromCameraMatrixValues((float)cam[0], (float)cam[4], (float)cam[2], (float)cam[5], ImageW, ImageH, 0.01f, 1000f);

            this.cam.projectionMatrix = camP;
            depthCam.projectionMatrix = camP;

            Matrix4x4 transformationM = new Matrix4x4(); // from OpenCV
            transformationM.SetRow(0, new Vector4((float)rotMatArr[0], (float)rotMatArr[1], (float)rotMatArr[2], (float)tvecArr[0]));
            transformationM.SetRow(1, new Vector4((float)rotMatArr[3], (float)rotMatArr[4], (float)rotMatArr[5], (float)tvecArr[1]));
            transformationM.SetRow(2, new Vector4((float)rotMatArr[6], (float)rotMatArr[7], (float)rotMatArr[8], (float)tvecArr[2]));
            transformationM.SetRow(3, new Vector4(0, 0, 0, 1));
            // Debug.Log("transformationM " + transformationM.ToString());

            Matrix4x4 invertZM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, -1));
            // Debug.Log("invertZM " + invertZM.ToString());

            Matrix4x4 invertYM = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));
            // Debug.Log("invertYM " + invertYM.ToString());

            // right-handed coordinates system (OpenCV) to left-handed one (Unity)
            Matrix4x4 ARM = invertYM * transformationM;

            // Apply Z-axis inverted matrix.
            ARM = ARM * invertZM;

            ARM = window.localToWorldMatrix * ARM.inverse;
            // Debug.Log("ARM " + ARM.ToString());

            SetTransformFromMatrix(this.cam.transform, ref ARM);

        }


        public Matrix4x4 CalculateProjectionMatrixFromCameraMatrixValues(float fx, float fy, float cx, float cy, float width, float height, float near, float far)
        {
            Matrix4x4 projectionMatrix = new Matrix4x4();
            projectionMatrix.m00 = 2.0f * fx / width;
            projectionMatrix.m02 = 1.0f - 2.0f * cx / width;
            projectionMatrix.m11 = 2.0f * fy / height;
            projectionMatrix.m12 = -1.0f + 2.0f * cy / height;
            projectionMatrix.m22 = -(far + near) / (far - near);
            projectionMatrix.m23 = -2.0f * far * near / (far - near);
            projectionMatrix.m32 = -1.0f;

            return projectionMatrix;
        }

        private void LoadCameraMarix(string filename, ref double[] cam, ref double[] dis)
        {
            string[] strs = File.ReadAllLines(filename);

            for (int i = 2; i < strs.Length; i++)
            {
                if (i <= 10)
                {
                    cam[i - 2] = double.Parse(strs[i]);
                }
                else if (i > 12)
                {
                    dis[i - 13] = double.Parse(strs[i]);
                }
            }
        }

        public void SetTransformFromMatrix(Transform transform, ref Matrix4x4 matrix)
        {
            transform.localPosition = ExtractTranslationFromMatrix(ref matrix);
            transform.localRotation = ExtractRotationFromMatrix(ref matrix);
            transform.localScale = ExtractScaleFromMatrix(ref matrix);
        }


        public Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 translate;
            translate.x = matrix.m03;
            translate.y = matrix.m13;
            translate.z = matrix.m23;
            return translate;
        }

        public Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 forward;
            forward.x = matrix.m02;
            forward.y = matrix.m12;
            forward.z = matrix.m22;

            Vector3 upwards;
            upwards.x = matrix.m01;
            upwards.y = matrix.m11;
            upwards.z = matrix.m21;

            return Quaternion.LookRotation(forward, upwards);
        }


        public Vector3 ExtractScaleFromMatrix(ref Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
            scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
            scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
            return scale;
        }

    }
}
