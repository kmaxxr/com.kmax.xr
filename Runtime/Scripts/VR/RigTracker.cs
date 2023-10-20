using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KmaxXR
{
    /// <summary>
    /// 通用追踪组件
    /// </summary>
    [DefaultExecutionOrder(ExecOrder)]
    public class RigTracker : MonoBehaviour
    {
        public const int ExecOrder = -100;
        /// <summary>
        /// 索引，标识
        /// </summary>
        [SerializeField]
        int id;
        int status;
        [SerializeField]
        Transform target;
        public bool HideOnInvisible = true;
        /// <summary>
        /// 是否可见
        /// </summary>
        public bool Visible => status > 0;

        void Start()
        {
            if (target == null)
                target = transform;
        }

        void OnValidate()
        {
            if (target == null || target == transform)
            {
                HideOnInvisible = false;
            }
        }

        Pose pose;
        void Update()
        {
            status = KmaxPlugin.GetTrackerPose(id, out pose) ? 1 : 0;
            target.localPosition = pose.position;
            target.localRotation = pose.rotation;
            if (HideOnInvisible)
            {
                target.gameObject.SetActive(Visible);
            }
        }
    }
}
