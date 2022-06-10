using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace KmaxXR
{
    public class GoFacade
    {

        private static GoFacade instance;
        public static GoFacade Instance
        {
            get
            {
                if (instance == null) instance = new GoFacade();
                return instance;
            }
        }

        private int stylusState = 0;
        public int StylusState
        {
            get { return stylusState; }
            set { stylusState = value; }
        }
        private GameObject curHitGameObject;
        public GameObject CurHitGameObject
        {
            get { return curHitGameObject; }
            set { curHitGameObject = value; }
        }

        private GameObject line;
        public GameObject Line
        {
            get { return line; }
            set { line = value; }

        }

        private float stoGoLength;
        public float StoGoLength
        {
            get { return stoGoLength; }

            set { stoGoLength = value; }
        }

        private StylusPose stylusPos;
        public StylusPose StylusPose
        {
            get
            {
                if (stylusPos == null)
                {
                    stylusPos = new StylusPose();
                }
                return stylusPos;

            }
            set { stylusPos = value; }
        }


        private bool buttonPress;
        public bool ButtonPress
        {
            get
            {
                return buttonPress;
            }
            set
            {
                buttonPress = value;
            }
        }

        private bool buttonScalePress;
        public bool ButtonScalePress { get => buttonScalePress; set => buttonScalePress = value; }

        public float HandDistance { get => handDistance; set => handDistance = value; }
        public Transform Hand { get => hand; set => hand = value; }

        private float handDistance;
        private Transform hand;

    }

    public class StylusPose
    {
        public Vector3 Direction;
        public Vector3 Pos;
        public Quaternion Rot;
    }
}
