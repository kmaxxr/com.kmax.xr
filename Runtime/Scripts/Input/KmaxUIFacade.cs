using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace KmaxXR
{
    [RequireComponent(typeof(GraphicRaycaster))]
    public class KmaxUIFacade : MonoBehaviour
    {
        private static List<KmaxUIFacade> uilist = new List<KmaxUIFacade>();
        public static IEnumerable<KmaxUIFacade> UIList => uilist;

        public static GraphicRaycaster RaycasterFor(Canvas c)
        {
            return c.GetComponent<GraphicRaycaster>();
        }

        private Canvas canvas;
        public Canvas Canvas
        {
            get {
                if (canvas == null) canvas = GetComponent<Canvas>();
                return canvas;
            }
        }

        private GraphicRaycaster raycaster;
        public GraphicRaycaster Raycaster
        {
            get {
                if (raycaster == null) raycaster = GetComponent<GraphicRaycaster>();
                return raycaster;
            }
        }
        private Camera eventCamera => Canvas.worldCamera;

        void OnEnable()
        {
            uilist.Add(this);
        }

        void OnDisable()
        {
            uilist.Remove(this);
        }

        public void Raycast(
            Ray ray,
            List<RaycastResult> resultAppendList,
            float maxDistance, 
            int layerMask)
        {
            
            // Retrieve the list of graphics associated with the canvas.
            IList<Graphic> graphics = 
                GraphicRegistry.GetGraphicsForCanvas(Canvas);
            if (graphics == null || graphics.Count == 0) return;

            // Iterate through each of graphics and perform hit tests.
            for (int i = 0; i < graphics.Count; ++i)
            {
                Graphic graphic = graphics[i];

                // Skip the graphic if it's not in the layer mask.
                if (((1 << graphic.gameObject.layer) & layerMask) == 0)
                {
                    continue;
                }

                // Perform a raycast against the graphic.
                RaycastResult result;
                if (Raycast(ray, graphic, out result, maxDistance))
                {
                    resultAppendList.Add(result);
                }
            }

            // Sort the results by depth.
            resultAppendList.Sort((x, y) => y.depth.CompareTo(x.depth));

            // Sort the results by sortingOrder.
            resultAppendList.Sort((x, y) =>
                y.sortingOrder.CompareTo(x.sortingOrder));
        }

        private bool Raycast(
            Ray ray,
            Graphic graphic,
            out RaycastResult result,
            float maxDistance)
        {
            result = default(RaycastResult);

            // Skip graphics that aren't raycast targets.
            // Skip graphics that aren't being drawn.
            if (graphic.depth == -1 || !graphic.raycastTarget || graphic.canvasRenderer.cull)
            {
                return false;
            }

            // Skip graphics that are reversed.
            if (Vector3.Dot(ray.direction, -graphic.transform.forward) > 0)
            {
                return false;
            }

            // Create a plane based on the graphic's transform.
            Plane plane = new Plane(
                -graphic.transform.forward, graphic.transform.position);

            // Skip graphics that failed the plane intersection test.
            float distance = 0.0f;
            if (!plane.Raycast(ray, out distance))
            {
                return false;
            }

            // Skip graphics that are further away than the max distance.
            if (distance > maxDistance)
            {
                return false;
            }

            Vector3 worldPosition = 
                ray.origin + (ray.direction * distance);

            Vector3 screenPosition = eventCamera.WorldToScreenPoint(worldPosition);            

            // Skip graphics that have failed the bounds test.
            if (!RectTransformUtility.RectangleContainsScreenPoint(
                    graphic.rectTransform, screenPosition, this.eventCamera))
            {
                return false;
            }

            // Skip graphics that fail the raycast test.
            // NOTE: This is necessary to ensure that raycasts against
            //       masked out areas of the graphic are correctly ignored.
            if (!graphic.Raycast(screenPosition, this.eventCamera))
            {
                return false;
            }

            result.depth = graphic.depth;
            result.distance = distance;
            result.worldPosition = worldPosition;
            result.worldNormal = plane.normal;
            result.screenPosition = screenPosition;
            result.gameObject = graphic.gameObject;
            result.sortingLayer = graphic.canvas.cachedSortingLayerValue;
            result.sortingOrder = graphic.canvas.sortingOrder;
            result.module = Raycaster;

            return true;
        }
    }
}