using System;
using UnityEngine;

namespace KmaxXR
{
    [CreateAssetMenu(menuName = StylusProvider.MenuPrefix + nameof(KmaxPenOne), fileName = nameof(KmaxPenOne), order = 100)]
    public class KmaxPenOne : StylusProvider
    {
        public bool LeftHand;
        [Tooltip("Simulate a pen with your mouse")]
        public bool MouseSimulate;
        public override void Open()
        {
            var ret = KmaxPlugin.Pen_Open();
            if (!ret)
            {
                Debug.LogWarning("Kmax Pen was not connected, please check your device.");
            }
        }

        /// <summary>
        /// 射线笔按钮状态(上一帧)
        /// </summary>
        private bool[] btnLastStates = { false, false, false };
        /// <summary>
        /// 射线笔按钮状态(当前帧)
        /// </summary>
        private bool[] btnStates = { false, false, false };

        public override void Process()
        {
            // 更新按钮状态
            for (int i = 0; i < btnLastStates.Length; i++)
            {
                btnLastStates[i] = btnStates[i];
                #if UNITY_EDITOR
                btnStates[i] = KmaxPlugin.Pen_GetButtonDown(i);
                if (MouseSimulate) btnStates[i] = btnStates[i] || Input.GetMouseButton(i);
                #else
                btnStates[i] = KmaxPlugin.Pen_GetButtonDown(i);
                #endif
            }
        }

        int KeyToIndex(StylusKey key)
        {
            switch (key)
            {
                case StylusKey.Mid: return 2;
                case StylusKey.Left: return LeftHand ? 1 : 0;
                case StylusKey.Right: return LeftHand ? 0 : 1;
                default: return 2;
            }
        }

        public override bool GetKey(StylusKey key) => btnStates[KeyToIndex(key)];

        public override bool GetKeyDown(StylusKey key)
        {
            int id = KeyToIndex(key);
            return btnStates[id] && !btnLastStates[id];
        }

        public override bool GetKeyUp(StylusKey key)
        {
            int id = KeyToIndex(key);
            return !btnStates[id] && btnLastStates[id];
        }
    }

}