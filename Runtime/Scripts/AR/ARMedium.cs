using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Linq;

namespace KmaxXR
{
    [Serializable]
    public enum AR_Action
    {
        Connect,
        GetPose,
        Start,
    }

    internal class ARMedium : IDisposable
    {
        private UdpClient client;
        private string host;
        internal const int DefaultPort = 11000;
        private int port;
        public Action<AR_Action, string> OnProcessMessage;
        public ARMedium(string host = "localhost", int port = DefaultPort)
        {
            this.host = host;
            this.port = port;
        }

        public void ConnectAR()
        {
            client = new UdpClient();
            IPEndPoint end = new IPEndPoint(IPAddress.Any, port);
            client.Connect(host, port);
            client.BeginReceive(ReceiveBack, end);
            OnSubmit(AR_Action.Connect, "connect");
        }

        public void OnSubmit(AR_Action action, string msg)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(msg);
            byte[] actionBytes = BitConverter.GetBytes((int)action);
            byte[] finalBytes = actionBytes.Concat(dataBytes).ToArray<byte>();

            client.BeginSend(finalBytes, finalBytes.Length, SendCallback, null);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                client.EndSend(ar);
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
            }

        }

        void ReceiveBack(IAsyncResult ir)
        {
            IPEndPoint end = ir.AsyncState as IPEndPoint;
            byte[] recBytes = client.EndReceive(ir, ref end);
            if (recBytes.Length > 4)
            {
                AR_Action actionType = (AR_Action)BitConverter.ToInt32(recBytes, 0);
                string result = Encoding.UTF8.GetString(recBytes, 4, recBytes.Length - 4);
                OnProcessMessage?.Invoke(actionType, result);
                // Debug.Log($"receive msg {result}");
            }
            client.BeginReceive(ReceiveBack, end);
        }

        public void Dispose()
        {
            client?.Close();
            client?.Dispose();
        }
    }
}