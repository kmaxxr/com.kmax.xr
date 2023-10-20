using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KmaxXR
{
    public class StylusDragable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        private Vector3 _initialGrabOffset = Vector3.zero;
        private Quaternion _initialGrabRotation = Quaternion.identity;
        private bool _isKinematic = false;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsStylus(eventData)) return;
            // Debug.Log("Drag begin");

            var pointer = KmaxPointer.PointerById(eventData.pointerId);
            var pose = pointer.EndpointPose;
            // Cache the initial grab state.
            this._initialGrabOffset =
                Quaternion.Inverse(this.transform.rotation) *
                (this.transform.position - pose.position);

            this._initialGrabRotation =
                Quaternion.Inverse(pose.rotation) *
                this.transform.rotation;

            // If the grabbable object has a rigidbody component,
            // mark it as kinematic during the grab.
            var rigidbody = this.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                this._isKinematic = rigidbody.isKinematic;
                rigidbody.isKinematic = true;
            }

            pointer.GrabObject = gameObject;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsStylus(eventData)) return;
            // Debug.Log("Draging");
            var pointer = KmaxPointer.PointerById(eventData.pointerId);
            var pose = pointer.EndpointPose;

            // Update the grab object's rotation.
            this.transform.rotation =
                pose.rotation * this._initialGrabRotation;

            // Update the grab object's position.
            this.transform.position =
                pose.position +
                (this.transform.rotation * this._initialGrabOffset);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!IsStylus(eventData)) return;
            // Debug.Log("Drag end");
            var pointer = KmaxPointer.PointerById(eventData.pointerId);
            // If the grabbable object has a rigidbody component,
            // restore its original isKinematic state.
            var rigidbody = this.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.isKinematic = this._isKinematic;
            }
            pointer.GrabObject = null;
        }

        public static bool IsStylus(PointerEventData eventData)
        {
            return IsStylus(eventData.pointerId);
        }

        public static bool IsStylus(int pointerId)
        {
            return pointerId % KmaxStylus.StylusButtnCenter == 0;
        }

    }
}