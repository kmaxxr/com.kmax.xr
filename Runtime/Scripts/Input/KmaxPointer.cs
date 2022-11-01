using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KmaxXR
{
    public abstract class KmaxPointer : MonoBehaviour
    {
        [SerializeField] protected Camera eventCamera;
        private static List<KmaxPointer> pointers = new List<KmaxPointer>();

        public static IEnumerable<KmaxPointer> Pointers => pointers;
        public static KmaxPointer PointerById(int id) => pointers.Find(p => p.Id == id);
        public static bool Enable => pointers.Count > 0;
        public abstract int Id { get; }
        public abstract Vector2 ScreenPosition { get; }
        public abstract Pose EndpointPose { get; }
        public bool Hit3D { get; protected set; }
        public GameObject GrabObject { get; set; }
        protected bool press, lastpress;
        public PointerEventData.FramePressState State {
            get
            {
                if (press != lastpress)
                    return press ? PointerEventData.FramePressState.Pressed : PointerEventData.FramePressState.Released;
                
                return PointerEventData.FramePressState.NotChanged;
            }
        }
        
        protected void OnEnable()
        {
            pointers.Add(this);
            if (eventCamera == null) eventCamera = Camera.main;
        }
        protected void OnDisable()
        {
            pointers.Remove(this);
        }

        public abstract void UpdateState();
        [System.NonSerialized]
        protected List<RaycastResult> raycastResultCache = new List<RaycastResult>();
        public abstract void Raycast(PointerEventData eventData);

        protected static RaycastResult FindFirstRaycast(List<RaycastResult> candidates)
        {
            for (var i = 0; i < candidates.Count; ++i)
            {
                if (candidates[i].gameObject == null)
                    continue;

                return candidates[i];
            }
            return new RaycastResult();
        }
    }
}