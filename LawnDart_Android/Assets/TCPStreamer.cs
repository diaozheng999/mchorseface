using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;

/**
 * Adjusted from PGT's NetCode.cs - Sudoku Shuffle
 */

namespace McHorseface.LawnDartController
{
    public class TCPStreamer : MonoBehaviour
    {
        const byte PACKET_HEADER = 0x53;
        const byte ACCEL_UPDATE = 0x04;
        const byte POS_UPDATE = 0x01;
        const byte BTN_ON = 0x02;
        const byte BTN_OFF = 0x03;

        // listeners
        private TcpListener[] listeners;
        private List<IPAddress> ip;
        NetworkStream stream;
        byte[] buffer;

        [SerializeField]
        Text ipAddress;
        bool pressed = false;

        // Use this for initialization
        void Start()
        {
            enabled = false;
            ip = new List<IPAddress>();
            var host = Dns.GetHostEntry(Dns.GetHostName());

            buffer = new byte[sizeof(float) * 7 + 2];

            foreach(var entry in host.AddressList)
            {
                if(entry.AddressFamily == AddressFamily.InterNetwork)
                {
                    ip.Add(entry);
                }
            }
            ipAddress.text = "";

            listeners = new TcpListener[ip.Count];

            for(int i=0; i<ip.Count; i++)
            {
                try
                {
                    var ep = new IPEndPoint(ip[i], 53451);
                    listeners[i] = new TcpListener(ep);
                    listeners[i].Start();
                    listeners[i].BeginAcceptTcpClient(new AsyncCallback(Transition), listeners[i]);
                    ipAddress.text += ip[i].ToString() + " /OR/ ";
                } catch {
                    listeners[i] = null;
                }
            }
        }

        protected void Transition(IAsyncResult res)
        {
            //stop all other listeners
            var listener = (TcpListener)res.AsyncState;
            var client = listener.EndAcceptTcpClient(res);

            StopListeners();

            stream = client.GetStream();
            enabled = true;

        }

        protected void StopListeners()
        {
            if (listeners == null) return;
            for (int i=0; i<ip.Count; i++)
            {
                if(listeners[i]!=null && !listeners[i].Server.Connected)
                {
                    listeners[i].Stop();
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            var pos = transform.position;
            var rot = transform.rotation;

            buffer[0] = 0x01;
            Array.Copy(BitConverter.GetBytes(pos.x), 0, buffer, 1, 4);
            Array.Copy(BitConverter.GetBytes(pos.y), 0, buffer, 5, 4);
            Array.Copy(BitConverter.GetBytes(pos.z), 0, buffer, 9, 4);
            Array.Copy(BitConverter.GetBytes(rot.x), 0, buffer, 13, 4);
            Array.Copy(BitConverter.GetBytes(rot.y), 0, buffer, 17, 4);
            Array.Copy(BitConverter.GetBytes(rot.z), 0, buffer, 21, 4);
            Array.Copy(BitConverter.GetBytes(rot.w), 0, buffer, 25, 4);

            stream.Write(buffer, 0, 29);
            
            if(Input.touchCount > 0 && !pressed)
            {
                pressed = true;
                stream.WriteByte(BTN_ON);
                Handheld.Vibrate();
            }else if(Input.touchCount == 0 && pressed)
            {
                pressed = false;
                stream.WriteByte(BTN_OFF);
                Handheld.Vibrate();
            }

            buffer[0] = ACCEL_UPDATE;
            var accel = Input.acceleration;
            Array.Copy(BitConverter.GetBytes(accel.x), 0, buffer, 1, 4);
            Array.Copy(BitConverter.GetBytes(accel.y), 0, buffer, 5, 4);
            Array.Copy(BitConverter.GetBytes(accel.z), 0, buffer, 9, 4);
            stream.Write(buffer, 0, 13);


            stream.Flush();
        }
    }
}

