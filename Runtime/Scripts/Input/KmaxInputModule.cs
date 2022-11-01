using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KmaxXR {
    public class KmaxInputModule : StandaloneInputModule
    {
        public override void Process()
        {
            base.Process();
            ProcessStylusEvent();
        }

        private bool ProcessStylusEvent()
        {
            foreach (var p in KmaxPointer.Pointers)
            {
                var data = GetStylusEventData(p);
                ProcessStylusPress(data, p.State);
                ProcessStylusMove(data);
                ProcessStylusDrag(data);
            }
            return KmaxPointer.Enable;
        }

        private PointerEventData GetStylusEventData(KmaxPointer pointer)
        {
            PointerEventData centerData;
            bool created = GetPointerData(pointer.Id, out centerData, true);
            centerData.Reset();
            if (created) centerData.position = pointer.ScreenPosition;
            pointer.UpdateState();
            pointer.Raycast(centerData);
            return centerData;
        }

        protected void ProcessStylusPress(PointerEventData data, PointerEventData.FramePressState state)
        {
            var currentObj = data.pointerCurrentRaycast.gameObject;
            if (currentObj == null) return;
            if (state == PointerEventData.FramePressState.Pressed)
            {
                data.eligibleForClick = true;
                data.delta = Vector2.zero;
                data.dragging = false;
                data.useDragThreshold = true;
                data.pressPosition = data.position;
                data.pointerPressRaycast = data.pointerCurrentRaycast;

                DeselectIfSelectionChanged(currentObj, data);
                // Debug.Log("point down");
                PointerEventData pointerEvent = data;
                var newPressed = ExecuteEvents.ExecuteHierarchy(currentObj, pointerEvent, ExecuteEvents.pointerDownHandler);

                // didnt find a press handler... search for a click handler
                if (newPressed == null)
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObj);

                // Debug.Log("Pressed: " + newPressed);

                float time = Time.unscaledTime;

                if (newPressed == pointerEvent.lastPress)
                {
                    var diffTime = time - pointerEvent.clickTime;
                    if (diffTime < 0.3f)
                        ++pointerEvent.clickCount;
                    else
                        pointerEvent.clickCount = 1;

                    pointerEvent.clickTime = time;
                }
                else
                {
                    pointerEvent.clickCount = 1;
                }

                pointerEvent.pointerPress = newPressed;
                pointerEvent.rawPointerPress = currentObj;

                pointerEvent.clickTime = time;

                // Save the drag handler as well
                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentObj);

                if (pointerEvent.pointerDrag != null)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
            }
            else if (state == PointerEventData.FramePressState.Released)
            {
                // Debug.Log("point up");
                PointerEventData pointerEvent = data;
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentObj);

                // PointerClick and Drop events
                if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
                }
                else if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                {
                    ExecuteEvents.ExecuteHierarchy(currentObj, pointerEvent, ExecuteEvents.dropHandler);
                }

                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;

                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

                pointerEvent.dragging = false;
                pointerEvent.pointerDrag = null;

                // redo pointer enter / exit to refresh state
                // so that if we moused over something that ignored it before
                // due to having pressed on something else
                // it now gets it.
                if (currentObj != pointerEvent.pointerEnter)
                {
                    HandlePointerExitAndEnter(pointerEvent, null);
                    HandlePointerExitAndEnter(pointerEvent, currentObj);
                }
            }
        }

        protected void ProcessStylusMove(PointerEventData data)
        {
            HandlePointerExitAndEnter(data, data.pointerCurrentRaycast.gameObject);
        }

        protected void ProcessStylusDrag(PointerEventData data)
        {
            ProcessDrag(data);
        }
    }
}