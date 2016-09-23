/**
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

        public const string BUTTON_OFF = "ld_btn_off";
        public const string BUTTON_ON = "ld_btn_on";

        [SerializeField]
        InputField IPAddressInput;
        NetworkStream stream;

        [SerializeField]
        GameObject Dart;

        [SerializeField]
        GameObject Internal;

        [SerializeField]
        float ScaleFactor = 3f;

        Vector3 pos;
        Quaternion rot;
        Vector3 accel;
        Semaphore sem;

        bool terminated = false;
        byte[] buffer;
        void Start()
        {
            enabled = false;
            buffer = new byte[29];
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

            EventRegistry.instance.AddEventListener(BUTTON_OFF, () =>
            {
                var dup = Instantiate(Dart);
                var rb = dup.GetComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.transform.rotation = Internal.transform.rotation;
                rb.transform.position = transform.position;

                Vector3 gravity = rb.transform.InverseTransformVector(Vector3.down);

                //rb.AddRelativeForce(force * rb.transform.forward, ForceMode.Impulse);

                rb.AddRelativeForce(ScaleFactor * (accel - gravity), ForceMode.Impulse);
                Debug.Log(accel);

                EventRegistry.instance.SetTimeout(20f, () =>
                {
                    Destroy(dup);
                });
            }, true);
            
        }
        /*
        UnityCoroutine NetworkListener()
        {
            while (!terminated)
            {
                var yield_instr = new YieldWhen();
                stream.BeginRead(buffer, 0, 1, (IAsyncResult s) =>
                {
                    yield_instr.wait = false;
                    stream.EndRead(s);
                }, null);

                yield return yield_instr;


                switch (buffer[0])
                {
                    case POS_UPDATE:
                        Debug.Log("Received POS_UPDATE");
                        yield_instr = new YieldWhen();

                        stream.BeginRead(buffer, 1, 28, (IAsyncResult s) =>
                        {
                            yield_instr.wait = false;
                            stream.EndRead(s);
                        }, null);
                        yield return yield_instr;
                        LogBytes(buffer);
                        pos.x = BitConverter.ToSingle(buffer, 1);
                        pos.y = BitConverter.ToSingle(buffer, 5);
                        pos.z = BitConverter.ToSingle(buffer, 9);
                        rot.x = BitConverter.ToSingle(buffer, 13);
                        rot.y = BitConverter.ToSingle(buffer, 17);
                        rot.z = BitConverter.ToSingle(buffer, 21);
                        rot.w = BitConverter.ToSingle(buffer, 25);
                        break;

                    case ACCEL_UPDATE:
                        Debug.Log("Received ACCEL_UPDATE");
                        yield_instr = new YieldWhen();

                        stream.BeginRead(buffer, 1, 12, (IAsyncResult s) =>
                        {
                            yield_instr.wait = false;
                            stream.EndRead(s);
                        }, null);
                        yield return yield_instr;
                        LogBytes(buffer);
                        accel.x = BitConverter.ToSingle(buffer, 1);
                        accel.y = BitConverter.ToSingle(buffer, 5);
                        accel.z = BitConverter.ToSingle(buffer, 9);
                        break;

                    case BTN_OFF:
                        Debug.Log("Received BTN_OFF");
                        EventRegistry.instance.Invoke(BUTTON_OFF);
                        break;

                    case BTN_ON:
                        Debug.Log("Received BTN_ON");
                        EventRegistry.instance.Invoke(BUTTON_ON);
                        break;

                    default:
                        Debug.Log("Unknown byte");
                        break;
                }
                
            }
        }*/

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
                        pos.x = BitConverter.ToSingle(buffer, 1);
                        pos.y = BitConverter.ToSingle(buffer, 5);
                        pos.z = BitConverter.ToSingle(buffer, 9);
                        rot.x = BitConverter.ToSingle(buffer, 13);
                        rot.y = BitConverter.ToSingle(buffer, 17);
                        rot.z = BitConverter.ToSingle(buffer, 21);
                        rot.w = BitConverter.ToSingle(buffer, 25);
                        break;

                    case ACCEL_UPDATE:
                        for (var i = 1; i < 13; i += stream.Read(buffer, i, 13 - i));
                        accel.x = BitConverter.ToSingle(buffer, 1);
                        accel.y = BitConverter.ToSingle(buffer, 5);
                        accel.z = BitConverter.ToSingle(buffer, 9);
                        break;

                    case BTN_OFF:
                        EventRegistry.instance.Invoke(BUTTON_OFF);
                        break;

                    case BTN_ON:
                        EventRegistry.instance.Invoke(BUTTON_ON);
                        break;

                    default:
                        Debug.LogError("Packets out of sync.");
                        break;
                }
            }
        }


        void OnDestroy()
        {
            terminated = true;
        }

	    // Update is called once per frame
	    void Update () {
            transform.rotation = rot;
	    }
    }
}

