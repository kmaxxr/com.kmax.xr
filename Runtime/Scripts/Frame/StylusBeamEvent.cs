using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
namespace KmaxXR
{
    internal static class StylusBeamEvent
    {
        /// <summary>
        /// 触笔按钮按下回调
        /// </summary>
        public static Action<GameObject> StylusBeamClickDown;
        /// <summary>
        /// 触笔按钮抬起回调
        /// </summary>
        public static Action<GameObject> StylusBeamClickUp;
        /// <summary>
        /// 触笔点击物体回调
        /// </summary>
        public static Action<GameObject> StylusBeamClick;
        /// <summary>
        /// 点击空物体回调
        /// </summary>
        public static Action StylusBeamClickNull;
    }
    /// <summary>
    /// 射线笔按钮状态接口
    /// </summary>
    public interface IStylusButtonProvider
    {
        bool GetKey(StylusKey key);
        bool GetKeyDown(StylusKey key);
        bool GetKeyUp(StylusKey key);
    }
    /// <summary>
    /// 射线笔按钮事件接口
    /// </summary>
    public interface IStylusButtonEvent
    {
        event Action OnMidBtnDown;
        event Action OnMidBtnUp;
        event Action OnLeftBtnDown;
        event Action OnLeftBtnUp;
        event Action OnRightBtnDown;
        event Action OnRightBtnUp;
    }
    /// <summary>
    /// 射线笔按钮键值
    /// </summary>
    public enum StylusKey { Mid, Left, Right }
    /// <summary>
    /// 射线笔按钮事件参数
    /// </summary>
    public struct StylusButtonArgs
    {
        public bool down;
        public StylusKey key;
        public GameObject hitObj;
    }
}
