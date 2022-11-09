using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
namespace KmaxXR
{
    [RequireComponent(typeof(Canvas))]
    public class UIFacade : MonoBehaviour
    {
        private int width => resolution.x;
        private int height => resolution.y;
        [SerializeField, Tooltip("pixel size")]
        Vector2Int resolution = new Vector2Int(1920, 1080);
        [SerializeField] bool syncOrientation = true;
        private Canvas canvas;
        /// <summary>
        /// 当前对象上的画布组件
        /// </summary>
        public Canvas ThisCanvas { get {
            if (canvas == null) canvas = GetComponent<Canvas>();
            return  canvas;
        } }
        /// <summary>
        /// 初始化的所有图形
        /// </summary>
        private static List<Canvas> canvasList = new List<Canvas>();
        public static IEnumerable<Canvas> Canvases => canvasList;
        /// <summary>
        /// 选中最靠近镜头的图形
        /// </summary>
        public static Graphic SelectedGraphics = null;
        /// <summary>
        /// 射线穿过的所有图形
        /// </summary>
        public static List<Graphic> perSelectedGraphics = new List<Graphic>();

        private static Vector2 ray2dPoint;
        public static Vector2 Ray2dPoint { get => ray2dPoint; set => ray2dPoint = value; }
        private static Vector3 ray3dPoint;
        public static Vector3 Ray3dPoint { get => ray3dPoint; set => ray3dPoint = value; }
        /// <summary>
        /// 是否跟随屏幕旋转
        /// </summary>
        public bool SyncOrientation { get => syncOrientation; set => syncOrientation = value; }

        void OnValidate()
        {
            if (width <= 0 || height <= 0)
            resolution = new Vector2Int(1920, 1080);
        }

        void OnEnable()
        {
            canvasList.Add(ThisCanvas);
            GlobalEntity.Instance.AddListener<Pose>(KmaxVREvent.PoseChanged, FixPose);
            GlobalEntity.Instance.AddListener<Vector2>(KmaxVREvent.ScreenSize, FixSize);
        }

        void OnDisable()
        {
            canvasList.Remove(ThisCanvas);
            GlobalEntity.Instance.RemoveListener<Pose>(KmaxVREvent.PoseChanged, FixPose);
            GlobalEntity.Instance.RemoveListener<Vector2>(KmaxVREvent.ScreenSize, FixSize);
        }

        public void FixSize(Vector2 size)
        {
            RectTransform rt = transform as RectTransform;
            OnValidate();
            var scacleX = size.x / width;
            var scacleY = size.y / height;
            var scaleValue = Mathf.Min(scacleX, scacleY);
            rt.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
            rt.sizeDelta = new Vector2(width, height);
        }

        public void FixPose(UnityEngine.Pose p)
        {
            if (!syncOrientation)
            {
                //ignore
                return;
            }
            RectTransform rt = transform as RectTransform;
            rt.position = p.position;
            rt.rotation = p.rotation;
        }
    }
}
