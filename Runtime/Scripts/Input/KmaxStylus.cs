using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Pose = UnityEngine.Pose;

namespace KmaxXR
{
    public class KmaxStylus : KmaxPointer
    {
        [SerializeField] StylusProvider provider;
        [SerializeField] GameObject point;
        [SerializeField] float rayLength = 1f;
        public const int StylusButtnCenter = 1 << 4;
        public const int StylusButtnLeft = StylusButtnCenter + 1;
        public const int StylusButtnRigth = StylusButtnCenter + 2;
        public override int Id => StylusButtnCenter;
        
        [System.Serializable]
        public class RayCastEvent : UnityEvent<GameObject> {}
        [System.Serializable]
        public class IntEvent : UnityEvent<int> {}
        [Header("Events")]
        /// <summary>
        /// Event dispatched when the pointer enters an object.
        /// </summary>
        [Tooltip("Event dispatched when the pointer enters an object.")]
        public RayCastEvent OnObjectEntered = new RayCastEvent();

        /// <summary>
        /// Event dispatched when the pointer exits an object.
        /// </summary>
        [Tooltip("Event dispatched when the pointer exits an object.")]
        public RayCastEvent OnObjectExited = new RayCastEvent();

        /// <summary>
        /// Event dispatched when a pointer button becomes pressed.
        /// </summary>
        [Tooltip("Event dispatched when a pointer button becomes pressed.")]
        public IntEvent OnButtonPressed = new IntEvent();

        /// <summary>
        /// Event dispatched when a pointer button becomes released.
        /// </summary>
        [Tooltip("Event dispatched when a pointer button becomes released.")]
        public IntEvent OnButtonReleased = new IntEvent();
        private LineRenderer line;
        void Start()
        {
            point.transform.localPosition = rayLength * Vector3.forward;
            line = GetComponentInChildren<LineRenderer>();
            if (line != null)
            {
                line.useWorldSpace = false;
                line.SetPosition(1, rayLength * Vector3.forward);
            }
            provider.Open();
        }

        public override void UpdateState()
        {
            provider.Process();
            lastpress = press;
            press = provider.GetKey(StylusKey.Mid);
        }

        readonly List<Graphic> sortedGraphics = new List<Graphic>();
        private void Raycast(Canvas canvas, List<Graphic> results)
        {
            if (canvas == null)
                return;
            
            var foundGraphics = GraphicRegistry.GetGraphicsForCanvas(canvas);
            if (foundGraphics == null || foundGraphics.Count == 0) return;
            var pointerPosition = point.transform.position;
            // Necessary for the event system
            int totalCount = foundGraphics.Count;
            for (int i = 0; i < totalCount; ++i)
            {
                Graphic graphic = foundGraphics[i];

                // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
                if (graphic.depth == -1 || !graphic.raycastTarget || graphic.canvasRenderer.cull)
                    continue;

                if (!RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPosition, eventCamera))
                    continue;

                if (eventCamera != null && eventCamera.WorldToScreenPoint(graphic.rectTransform.position).z > eventCamera.farClipPlane)
                    continue;

                if (graphic.Raycast(pointerPosition, eventCamera))
                {
                    sortedGraphics.Add(graphic);
                }
            }

            sortedGraphics.Sort((g1, g2) => g2.depth.CompareTo(g1.depth));
            totalCount = sortedGraphics.Count;
            for (int i = 0; i < totalCount; ++i)
                results.Add(sortedGraphics[i]);

            sortedGraphics.Clear();
        }

        private RaycastResult result3D, result2D;

        public override Vector2 ScreenPosition => eventCamera.WorldToScreenPoint(point.transform.position);

        // public override Pose EndpointPose => new Pose(point.transform.position, point.transform.rotation);
        public override Pose EndpointPose => new Pose(transform.position, point.transform.rotation);

        public override void Raycast(PointerEventData eventData)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            // 3D
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, rayLength))
            {
                result3D = new RaycastResult
                {
                    gameObject = hit.collider.gameObject,
                    distance = hit.distance,
                    worldPosition = hit.point,
                    worldNormal = hit.normal,
                    screenPosition = eventCamera.WorldToScreenPoint(hit.point),
                    index = 0,
                    sortingLayer = 0,
                    sortingOrder = 0
                };
            }
            else
            {
                point.SetActive(false);
                result3D = default(RaycastResult);
            }

            // UI
            foreach (var item in KmaxUIFacade.UIList)
            {
                item.Raycast(ray, raycastResultCache, rayLength, ~0);
            }
            result2D = FindFirstRaycast(raycastResultCache);
            raycastResultCache.Clear();

            // set event data
            if (result3D.gameObject == null)
            {
                eventData.pointerCurrentRaycast = result2D;
                Hit3D = false;
            }
            else if (result2D.gameObject == null)
            {
                eventData.pointerCurrentRaycast = result3D;
                Hit3D = true;
            }
            else
            {
                eventData.pointerCurrentRaycast = result2D.distance > result3D.distance ? result3D : result2D;
                Hit3D = result2D.distance > result3D.distance;
            }

            point.SetActive(eventData.pointerCurrentRaycast.gameObject != null);
            if (result3D.gameObject == null && result2D.gameObject == null) // nothing cast
            {
                point.transform.localPosition = rayLength * Vector3.forward;
                Vector2 curPosition = eventCamera.WorldToScreenPoint(point.transform.position);
                eventData.delta = curPosition - eventData.position;
                eventData.position = curPosition;
            }
            else
            {
                point.transform.position = eventData.pointerCurrentRaycast.worldPosition;
                eventData.delta = eventData.pointerCurrentRaycast.screenPosition - eventData.position;
                eventData.position = eventData.pointerCurrentRaycast.screenPosition;
            }
            if (line != null) line.SetPosition(1, point.transform.localPosition - line.transform.localPosition);
        }

    }
}
