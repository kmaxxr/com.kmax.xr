
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
namespace KmaxXR
{

    public class StylusInputModule : StandaloneInputModule
    {
       
        /// <summary>
        /// Objects on layers specified in the exclude mask will not receive input events.  By
        /// default nothing is excluded.
        /// </summary>
        [Tooltip("Objects on layers specified in the exclude mask will not receive input events.")]
        public LayerMask LayerExcludeMask = 0;
      
        private PointerEventData _currentEventData = null;
        private float _uiHitDistance = float.MaxValue;
        private bool _uiOccluded = false;
        private bool _stylusButtonWasPressed = false;

        private const float PointerClickTimeThreshold = 0.3f;
        private PointerEventData _pointerEventOnPress = null;
        public float offsetX;
        public float offsetY;
        protected StylusInputModule()
        {
        }

        public override void UpdateModule()
        {
            base.UpdateModule();
        }

        public override bool IsModuleSupported()
        {
            return true;
        }


        public override bool ShouldActivateModule()
        {
            return base.ShouldActivateModule();
        }

        public override void ActivateModule()
        {
            // Try to find the event camera assigner, since that is currently
            // needed for raycasts to work effectively with Unity's UI system.


            base.ActivateModule();
        }

        public override void DeactivateModule()
        {
            base.DeactivateModule();
        }

        public override void Process()
        {

            SendUpdateEventToSelectedObject();

            ProcessStylusEvent();
        }

        /// <summary>
        /// Return the game object that is currently being interacted with.
        /// Returns null if there's nothing intersected.
        /// </summary>
        public GameObject GetCurrentIntersection()
        {
            return (this._currentEventData != null) ? this._currentEventData.pointerCurrentRaycast.gameObject : null;
            //return null;
        }

        /// <summary>
        /// Return distance from stylus tip to ui element being pointed to.  If no element is
        /// in line with the stylus ray, float.MaxValue is returned.
        /// </summary>
        public float GetStylusUiElementDistance()
        {
            PointerEventData stylusData = this._currentEventData;
            if (stylusData == null || stylusData.pointerCurrentRaycast.gameObject == null)
            {
                return float.MaxValue;
            }

            return this._uiHitDistance;
        }

        //////////////////////////////////////////////////////////////////
        // Private Methods
        //////////////////////////////////////////////////////////////////

        /// <summary>
        /// Process all mouse events.
        /// </summary>
        ///
        private void ProcessStylusEvent()
        {

            bool stylusButtonPressed = GoFacade.Instance.ButtonPress;
            bool buttonPressedThisFrame = (stylusButtonPressed && !this._stylusButtonWasPressed);
            bool buttonReleasedThisFrame = (!stylusButtonPressed && this._stylusButtonWasPressed);

            var stylusData = this.GetStylusPointerData(stylusButtonPressed);

            // If the stylus is not moving or is not pressed/released, we can process mouse events
            if (!this.UseStylus(buttonPressedThisFrame, buttonReleasedThisFrame, stylusData))
            {
                base.Process();
                return;
            }

            if (!this._uiOccluded)
            {
                // We don't want to process clicks if the UI is currently
                // occluded by a 3D object.
                this.ProcessStylusPress(stylusData, buttonPressedThisFrame, buttonReleasedThisFrame);
            }

            this.ProcessMove(stylusData);
            this.ProcessDrag(stylusData);

            // If we haven't done any events with the stylus this frame, we can process mouse events
            if (!this._stylusButtonWasPressed && !buttonPressedThisFrame && !buttonReleasedThisFrame)
            {
                base.Process();
            }

            this._stylusButtonWasPressed = stylusButtonPressed;
        }

        private PointerEventData GetStylusPointerData(bool stylusButtonPressed)
        {


            PointerEventData stylusData;
            var stylusPosition = new Vector2(0, 0);


            bool uiCurrentlyOccluded = false;

            stylusPosition = UIFacade.Ray2dPoint;
            if (this._currentEventData == null)
            {
                stylusData = new PointerEventData(eventSystem);
                stylusData.position = stylusPosition;
            }
            else
            {
                stylusData = this._currentEventData;
                stylusData.Reset();

            }


            var delta = stylusPosition - stylusData.position;

            if (Mathf.Abs(delta.y) > offsetY || Mathf.Abs(delta.x) > offsetX)
            {
                stylusData.delta = stylusPosition - stylusData.position;
            }
            else
            {
                stylusData.delta = new Vector2(0, 0);
            }
            //stylusData.delta = stylusPosition - stylusData.position;

            stylusData.position = stylusPosition;

            if (!this._stylusButtonWasPressed || !stylusButtonPressed)
            {   // Then we are definitely not dragging, so pick a new raycast target
                eventSystem.RaycastAll(stylusData, m_RaycastResultCache);
                stylusData.pointerCurrentRaycast = FindFirstRaycastWithMask(m_RaycastResultCache);

                m_RaycastResultCache.Clear();
            }   // If we aren't dragging, we want to maintain our same raycast target

            // If the UI has just become occluded, we want to pass in a null
            // pointerCurrentRaycast so that within ProcessMove(), a
            // pointer event exit event is triggered on the current object.
            if (uiCurrentlyOccluded && !this._uiOccluded)
            {
                var oldRaycast = stylusData.pointerCurrentRaycast;
                oldRaycast.gameObject = null;
                stylusData.pointerCurrentRaycast = oldRaycast;
            }
            this._uiOccluded = uiCurrentlyOccluded;

            this._currentEventData = stylusData;
            return stylusData;
        }

        RaycastResult FindFirstRaycastWithMask(List<RaycastResult> candidates)
        {
            foreach (RaycastResult rr in candidates)
            {
                if (rr.gameObject != null && 0 == ((1 << rr.gameObject.layer) & LayerExcludeMask))
                {
                    return rr;
                }
            }

            return new RaycastResult();
        }

        private bool UseStylus(bool pressed, bool released, PointerEventData pointerData)
        {
            return (pressed || released || (pointerData != null && pointerData.IsPointerMoving()));
        }

        /// <summary>
        /// Process the current mouse press.
        /// </summary>
        private void ProcessStylusPress(PointerEventData pointerEvent, bool buttonPressedThisFrame, bool buttonReleasedThisFrame)
        {
            var currentRaycastTarget = pointerEvent.pointerCurrentRaycast.gameObject;

            // Bail if target object is on an excluded layer.
            if (currentRaycastTarget != null)
            {
                if (0 != (LayerExcludeMask & (1 << currentRaycastTarget.layer)))
                {
                    _currentEventData = null;
                    return;
                }
            }

            if (buttonPressedThisFrame)
            {
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;
                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

                DeselectIfSelectionChanged(currentRaycastTarget, pointerEvent);

                // search for the control that will receive the press
                // if we can't find a press handler set the press
                // handler to be what would receive a click.
                var newPressed = ExecuteEvents.ExecuteHierarchy(currentRaycastTarget, pointerEvent, ExecuteEvents.pointerDownHandler);

                // didnt find a press handler... search for a click handler
                if (newPressed == null)
                {
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentRaycastTarget);
                }

                float currentTime = Time.unscaledTime;

                if (newPressed == pointerEvent.lastPress)
                {
                    var timeDifference = currentTime - pointerEvent.clickTime;
                    pointerEvent.clickCount = (timeDifference < 0.3f) ? pointerEvent.clickCount + 1 : 1;

                    pointerEvent.clickTime = currentTime;
                }
                else
                {
                    pointerEvent.clickCount = 1;
                }

                pointerEvent.pointerPress = newPressed;
                pointerEvent.rawPointerPress = currentRaycastTarget;
                pointerEvent.clickTime = currentTime;

                // Save the drag handler as well
                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentRaycastTarget);

                if (pointerEvent.pointerDrag != null)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
                }

                this._pointerEventOnPress = pointerEvent;
            }

            // PointerUp notification
            if (buttonReleasedThisFrame)
            {
                float pressTime = (this._pointerEventOnPress != null) ?
                    this._pointerEventOnPress.clickTime : 0;

                float timeSinceLastPress = Time.unscaledTime - pressTime;

                if (this._pointerEventOnPress != null &&
                    timeSinceLastPress <= PointerClickTimeThreshold)
                {
                    ExecuteEvents.Execute(
                        this._pointerEventOnPress.pointerPress,
                        this._pointerEventOnPress,
                        ExecuteEvents.pointerUpHandler);

                    ExecuteEvents.Execute(
                        this._pointerEventOnPress.pointerPress,
                        this._pointerEventOnPress,
                        ExecuteEvents.pointerClickHandler);
                }
                else
                {
                    ExecuteEvents.Execute(
                        pointerEvent.pointerPress,
                        pointerEvent,
                        ExecuteEvents.pointerUpHandler);
                }

                // see if we button up on the same element that we clicked on...
                var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentRaycastTarget);

                // PointerClick and Drop events
                if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick)
                {

                    if (timeSinceLastPress > PointerClickTimeThreshold)
                    {
                        ExecuteEvents.Execute(
                            pointerEvent.pointerPress,
                            pointerEvent,
                            ExecuteEvents.pointerClickHandler);
                    }
                }
                else if (pointerEvent.pointerDrag != null)
                {
                    ExecuteEvents.ExecuteHierarchy(currentRaycastTarget, pointerEvent, ExecuteEvents.dropHandler);
                }

                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;

                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                {
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
                }

                pointerEvent.dragging = false;
                pointerEvent.pointerDrag = null;

                // redo pointer enter / exit to refresh state
                // so that if we moused over somethign that ignored it before
                // due to having pressed on something else
                // it now gets it.
                if (currentRaycastTarget != pointerEvent.pointerEnter)
                {
                    HandlePointerExitAndEnter(pointerEvent, null);
                    HandlePointerExitAndEnter(pointerEvent, currentRaycastTarget);
                }
            }
        }

     

    
    }
}
