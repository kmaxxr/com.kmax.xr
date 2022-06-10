using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace KmaxXR
{
    public class KmaxAR : MonoBehaviour
    {
        private static KmaxAR kmaxAR;
        public static KmaxAR GetInstance()
        {
            if (kmaxAR == null)
                kmaxAR = GameObject.FindObjectOfType<KmaxAR>();

            return kmaxAR;
        }

        private bool ARState { get; set; }
        private int frame;
        private ARMedium client;
        private Queue<ARMessage> MsgQueue = new Queue<ARMessage>();

        public RenderTexture rt;
        public ARCamera arCam;
        [SerializeField]
        private string host = "localhost";
        // Start is called before the first frame update
        void Start()
        {
            client = new ARMedium(host);
            client.OnProcessMessage += OnProcessMesaageCallBack;
        }

        public void ConnectAR()
        {
            client.ConnectAR();
        }

        private void OnProcessMesaageCallBack(AR_Action action, string data)
        {
            ARMessage msg = new ARMessage();
            msg.action = action;
            msg.body = data;
            MsgQueue.Enqueue(msg);
        }


        private void HandleAction(ARMessage msg)
        {
            switch (msg.action)
            {
                case AR_Action.GetPose:
                    Pose p = JsonUtility.FromJson<Pose>(msg.body);
                    arCam.PoseCallback(p);
                    break;
            }
        }

        // Update is called once per frame
        private void Update()
        {
            if (MsgQueue.Count > 0)
            {
                ARMessage msg = MsgQueue.Dequeue();
                HandleAction(msg);
            }

            if (frame == 5)
            {
                if (ARState)
                    KmaxPlugin.SendARTexture(rt.GetNativeTexturePtr(), rt.width, rt.height);
                frame = 0;
            }
            frame++;

        }

        public void OpenAR()
        {
            KmaxPlugin.StartAR();
            ARState = true;
        }

        private void OnDestroy()
        {
            if (ARState)
            {
                Debug.Log(KmaxPlugin.CloseAR());
            }
            client?.Dispose();
        }
    }
}
