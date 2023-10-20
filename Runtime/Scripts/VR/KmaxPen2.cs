using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KmaxXR
{
    [CreateAssetMenu(menuName = StylusProvider.MenuPrefix + nameof(KmaxPen2), fileName = nameof(KmaxPen2), order = 101)]
    public class KmaxPen2 : KmaxPen
    {
        public override void Process()
        {
            // 更新按钮状态
            for (int i = 0; i < btnLastStates.Length; i++)
            {
                btnLastStates[i] = btnStates[i];
                #if UNITY_EDITOR
                btnStates[i] = KmaxPlugin.Pen_GetButtonDown1(i);
                if (MouseSimulate) btnStates[i] = btnStates[i] || Input.GetMouseButton(i);
                #else
                btnStates[i] = KmaxPlugin.Pen_GetButtonDown1(i);
                #endif
            }
        }
    }
}
