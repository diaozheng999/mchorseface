﻿/**
 * WiimoteController.cs
 * Simon Diao
 * 
 * Interfaces accelerometer and controls from LawnDart_Android.
 **/

using UnityEngine;
using UnityEngine.UI;
using UnityCoroutine = System.Collections.IEnumerator;
using PGT.Core;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace McHorseface.LawnDart
{
    public class YieldWhen : CustomYieldInstruction
    {
        public bool wait = true;
        public override bool keepWaiting {  get { return wait; } }
    }

    public class LDController : MonoBehaviour {

        const byte POS_UPDATE = 0x01;
        const byte BTN_ON = 0x02;
        const byte BTN_OFF = 0x03;
        const byte ACCEL_UPDATE = 0x04;
        const byte VIBRA = 0x05;

        public const string BUTTON_OFF = "ld_btn_off";
        public const string BUTTON_ON = "ld_btn_on";

        public static LDController instance = null;

        [SerializeField]
        InputField IPAddressInput;
        NetworkStream stream;

        [SerializeField]
        GameObject Post;

        [SerializeField]
        GameObject Internal;

        [SerializeField]
        float ScaleFactor = 3f;
        
        Quaternion rot;
        Vector3 accel;

        Quaternion calibratedRotation;

        // we're not locking cos rot and accel are not updated at all
        public Quaternion Rot { get {
                return rot;
            }
        }
        public Vector3 Accel { get {
                return accel;
            }
        }

        bool terminated = false;
        byte[] buffer;
        void Start()
        {
            enabled = false;
            buffer = new byte[29];

            if(instance != null)
            {
                Debug.Log("Another copy of LDController is present.");
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            instance = this;
            
        }

        public void Connect()
        {
            var client = new TcpClient();
            Debug.Log(IPAddressInput.GetComponentInChildren<Text>().text);
            try
            {
                client.Connect(IPAddressInput.GetComponentInChildren<Text>().text, 53451);
                stream = client.GetStream();
                enabled = true;
                (new Thread(NetworkListener)).Start();
            }
            catch
            {
                Debug.Log("Connection Error.");
            }
        }
       
        public void Calibrate()
        {
            transform.rotation = Quaternion.FromToRotation(Post.transform.forward, Vector3.forward);
        }

        public void Vibrate()
        {
            if (stream != null && stream.CanWrite)
            {
                stream.WriteByte(VIBRA);
                stream.Flush();
            }
        }

        void LogBytes(byte[] buffer)
        {
            string buf = "";
            foreach(byte b in buffer)
            {
                buf += b.ToString("X2")+" ";
            }

            Debug.Log("Printing buffer "+buf);
        }

        void NetworkListener()
        {
            byte[] buffer = new byte[sizeof(float) * 7 + 1];
            while (!terminated)
            {
                stream.Read(buffer, 0, 1);
                switch (buffer[0])
                {
                    case POS_UPDATE:
                        for (var i = 1; i < 29; i += stream.Read(buffer, i, 29 - i));

                        accel.x = BitConverter.ToSingle(buffer, 1);
                        accel.y = BitConverter.ToSingle(buffer, 5);
                        accel.z = BitConverter.ToSingle(buffer, 9);
                        rot.x = BitConverter.ToSingle(buffer, 13);
                        rot.y = BitConverter.ToSingle(buffer, 17);
                        rot.z = BitConverter.ToSingle(buffer, 21);
                        rot.w = BitConverter.ToSingle(buffer, 25);
 
                        break;

                    case BTN_OFF:
                        // Event library is currently NOT thread-safe
                        UnityExecutionThread.instance.ExecuteInMainThread(() =>
                        {
                            EventRegistry.instance.Invoke(BUTTON_OFF);
                        });
                        break;

                    case BTN_ON:
                        UnityExecutionThread.instance.ExecuteInMainThread(() =>
                        {
                            EventRegistry.instance.Invoke(BUTTON_ON);
                        });
                        break;

                    default:
                        Debug.LogError("Packets out of sync.");
                        break;
                }
            }
        }

        public Quaternion GetCalibratedRotation()
        {
            return Post.transform.rotation;
        }

        void OnDestroy()
        {
            terminated = true;
        }

	    // Update is called once per frame
	    void Update () {
            Internal.transform.localRotation = rot;
	    }
    }
}