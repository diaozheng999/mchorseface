using UnityEngine;
using UnityEngine.UI;
using UnityCoroutine = System.Collections.IEnumerator;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;

/**
 * Adjusted from PGT's NetCode.cs - Sudoku Shuffle
 */

namespace McHorseface.LawnDartController
{
    class YieldWhen : CustomYieldInstruction
    {
        bool wait = true;
        public override bool keepWaiting { get { return wait; } }
        public void resolve()
        {
            wait = false;
        }
    }

    public class TCPStreamer : MonoBehaviour
    {
        const byte PACKET_HEADER = 0x53;
        const byte ACCEL_UPDATE = 0x04;
        const byte POS_UPDATE = 0x01;
        const byte BTN_ON = 0x02;
        const byte BTN_OFF = 0x03;
        const byte VIBRA = 0x05;

        // listeners
        private TcpListener[] listeners;
        private List<IPAddress> ip;
        NetworkStream stream;
        byte[] buffer;

        [SerializeField]
        Text ipAddress;

        // Data section
        [SerializeField] Text roll_val;
        [SerializeField] Text pitch_val;
        [SerializeField]
        Text yaw_val;
        [SerializeField]
        Text pos_x_val;
        [SerializeField]
        Text pos_y_val;
        [SerializeField]
        Text pos_z_val;
        [SerializeField]
        Text accel_x_val;
        [SerializeField]
        Text accel_y_val;
        [SerializeField]
        Text accel_z_val;
        [SerializeField]
        GameObject button_indicator;

        bool pressed = false;

        // Use this for initialization
        void Start()
        {
            enabled = false;
            ip = new List<IPAddress>();
            var host = Dns.GetHostEntry(Dns.GetHostName());
            button_indicator.SetActive(false);
            buffer = new byte[sizeof(float) * 7 + 2];

            foreach(var entry in host.AddressList)
            {
                if(entry.AddressFamily == AddressFamily.InterNetwork)
                {
                    ip.Add(entry);
                }
            }
            ipAddress.text = "Connect to:";

            listeners = new TcpListener[ip.Count];

            for(int i=0; i<ip.Count; i++)
            {
                try
                {
                    var ep = new IPEndPoint(ip[i], 53451);
                    listeners[i] = new TcpListener(ep);
                    listeners[i].Start();
                    listeners[i].BeginAcceptTcpClient(new AsyncCallback(Transition), listeners[i]);
                    ipAddress.text += "\n" + ip[i].ToString();
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

            StartCoroutine(ListenForVibration());
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

        bool update;

        void Update()
        {
            var rot = transform.eulerAngles;
            var pos = transform.position;
            var accel = Input.acceleration;

            roll_val.text = rot.z.ToString();
            pitch_val.text = rot.x.ToString();
            yaw_val.text = rot.y.ToString();

            pos_x_val.text = pos.x.ToString();
            pos_y_val.text = pos.y.ToString();
            pos_z_val.text = pos.z.ToString();

            accel_x_val.text = accel.x.ToString();
            accel_y_val.text = accel.y.ToString();
            accel_z_val.text = accel.z.ToString();
        }

        UnityCoroutine ListenForVibration()
        {
            byte[] buf = new byte[2];
            while (stream.CanRead)
            {
                var yield_instruction = new YieldWhen();
                stream.BeginRead(buf, 0, 1, (IAsyncResult r) =>
                {
                    yield_instruction.resolve();
                    stream.EndRead(r);
                }, null);
                yield return yield_instruction;
                if(buf[0] == VIBRA)
                {
                    Handheld.Vibrate();
                }
            }
        }

        void FixedUpdate()
        {

            // handle buttons
            if(Input.touchCount > 0 && !pressed)
            {
                pressed = true;
                stream.WriteByte(BTN_ON);
                button_indicator.SetActive(true);
                //Handheld.Vibrate();
            }else if(Input.touchCount == 0 && pressed)
            {
                pressed = false;
                button_indicator.SetActive(false);
                stream.WriteByte(BTN_OFF);
                Handheld.Vibrate();
            }

            var pos = transform.position;
            var rot = transform.rotation;
            var accel = Input.acceleration;

            buffer[0] = 0x01;
            Array.Copy(BitConverter.GetBytes(accel.x), 0, buffer, 1, 4);
            Array.Copy(BitConverter.GetBytes(accel.y), 0, buffer, 5, 4);
            Array.Copy(BitConverter.GetBytes(accel.z), 0, buffer, 9, 4);
            Array.Copy(BitConverter.GetBytes(rot.x), 0, buffer, 13, 4);
            Array.Copy(BitConverter.GetBytes(rot.y), 0, buffer, 17, 4);
            Array.Copy(BitConverter.GetBytes(rot.z), 0, buffer, 21, 4);
            Array.Copy(BitConverter.GetBytes(rot.w), 0, buffer, 25, 4);

            stream.Write(buffer, 0, 29);
            


            stream.Flush();
        }
    }
}

