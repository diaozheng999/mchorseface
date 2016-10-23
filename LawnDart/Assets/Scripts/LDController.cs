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
using PGT.Core.Func;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

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
        const byte BTN_2_ON = 0x07;
        const byte BTN_2_OFF = 0x08;
        const byte BTN_3_ON = 0x09;
        const byte BTN_3_OFF = 0x0A;
        const byte BTN_4_ON = 0x0B;
        const byte BTN_4_OFF = 0x0C;

        const byte IP_ADDR = 0x06;

        public const string BUTTON_OFF = "ld_btn_off";
        public const string BUTTON_ON = "ld_btn_on";
        public const string BUTTON_2_ON = "ld_btn_2_on";
        public const string BUTTON_2_OFF = "ld_btn_2_off";
        public const string BUTTON_3_ON = "ld_btn_3_on";
        public const string BUTTON_3_OFF = "ld_btn_3_off";
        public const string BUTTON_4_ON = "ld_btn_4_on";
        public const string BUTTON_4_OFF = "ld_btn_4_off";

        public static LDController instance = null;
        

        public string nextScene = "GameScene";

        [SerializeField]
        InputField IPAddressInput;
        NetworkStream stream;

        UdpClient udpClient;
        TcpClient tcpClient;

        [SerializeField]
        GameObject Post;

        [SerializeField]
        GameObject Sprite;

        [SerializeField]
        GameObject Internal;

        [SerializeField]
        GameObject Callie;

        [SerializeField]
        float ScaleFactor = 3f;
        
        Quaternion rot;
        Vector3 accel;

        Quaternion calibratedRotation;
        string ipaddr;

        bool quatblocked = false;
        
        public bool enableControls = false;

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

            if(instance != null)
            {
                Debug.Log("Another copy of LDController is present.");
                Destroy(gameObject);
                return;
            }

            buffer = new byte[29];

            DontDestroyOnLoad(gameObject);
            instance = this;

            EventRegistry.instance.AddEventListener(LDCalibrator.CALIB_TRYOUT, () =>
            {
                Sprite.SetActive(false);
            }, true);
            
        }

        public void ShowSprite()
        {
            Sprite.SetActive(true);
        }


        public void Connect()
        {
            var client = new TcpClient();
            Debug.Log(IPAddressInput.GetComponentInChildren<Text>().text);
            try
            {
                client.Connect(IPAddressInput.GetComponentInChildren<Text>().text, 53451);
                
                stream = client.GetStream();
                tcpClient = client;
                StartCoroutine(SendIpAddrAsync(client));

                udpClient = new UdpClient(53471);

                enabled = true;
                (new Thread(UDPNetworkListener)).Start();
                (new Thread(NetworkListener)).Start();
            }
            catch (Exception e)
            {
                Debug.Log("Connection Error: "+e.ToString());
            }
        }
       
        UnityCoroutine SendIpAddrAsync(TcpClient client)
        {
            yield return new WaitForEndOfFrame();
            
            byte[] charbuf = Encoding.ASCII.GetBytes(client.Client.LocalEndPoint.ToString().ToCharArray());

            stream.WriteByte(IP_ADDR);
            stream.WriteByte((byte)charbuf.Length);
            stream.Write(charbuf, 0, charbuf.Length);
            stream.Flush();
        }

        public void Calibrate(Vector3 fwd)
        {
            //aesthetics
            var mat = Sprite.GetComponentInChildren<MeshRenderer>().material;
            mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, 1);

            var delta = Quaternion.FromToRotation(fwd, Vector3.forward);


            transform.rotation = Quaternion.identity;

            transform.rotation = Quaternion.FromToRotation(delta * Post.transform.forward, Vector3.forward);

            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
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

        void UDPNetworkListener()
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 53471);
            while (!terminated)
            {

                Vector3 accel2 = new Vector3(0, 0, 0);
                Quaternion rot2 = new Quaternion(0, 0, 0, 0);
                byte[] buffer = udpClient.Receive(ref ipep);

                switch (buffer[0])
                {
                    case POS_UPDATE:
                        accel.x = BitConverter.ToSingle(buffer, 1);
                        accel.y = BitConverter.ToSingle(buffer, 5);
                        accel.z = BitConverter.ToSingle(buffer, 9);
                        rot.x = BitConverter.ToSingle(buffer, 13);
                        rot.y = BitConverter.ToSingle(buffer, 17);
                        rot.z = BitConverter.ToSingle(buffer, 21);
                        rot.w = BitConverter.ToSingle(buffer, 25);
                        break;
                    case 0x13:
                        accel2.x = BitConverter.ToSingle(buffer, 1);
                        accel2.y = BitConverter.ToSingle(buffer, 5);
                        accel2.z = BitConverter.ToSingle(buffer, 9);

                        rot2.x = BitConverter.ToSingle(buffer, 13);
                        rot2.y = BitConverter.ToSingle(buffer, 17);
                        rot2.z = BitConverter.ToSingle(buffer, 21);
                        rot2.w = BitConverter.ToSingle(buffer, 25);
                        var info = new Tuple<Vector3, Quaternion>(new Vector3(accel2.x, accel2.y, accel2.z), new Quaternion(rot2.x, rot2.y, rot2.z, rot2.w));
                        if (enableControls)
                            UnityExecutionThread.instance.ExecuteInMainThread(() =>
                            {
                                EventRegistry.instance.Invoke("FIRE", info);
                            });
                        break;
                }
                
            }
        }

        void NetworkListener()
        {
            byte[] buffer = new byte[sizeof(float) * 7 + 1];
            while (!terminated)
            {
                if (stream.Read(buffer, 0, 1) < 1) continue;

                Vector3 accel2 = new Vector3(0, 0, 0);
                Quaternion rot2 = new Quaternion(0, 0, 0, 0);
                switch (buffer[0])
                {
                    case POS_UPDATE:
                        for (var i = 1; i < 29; i += stream.Read(buffer, i, 29 - i));
                        accel2.x = BitConverter.ToSingle(buffer, 1);
                        accel2.y = BitConverter.ToSingle(buffer, 5);
                        accel2.z = BitConverter.ToSingle(buffer, 9);
                        rot2.x = BitConverter.ToSingle(buffer, 13);
                        rot2.y = BitConverter.ToSingle(buffer, 17);
                        rot2.z = BitConverter.ToSingle(buffer, 21);
                        rot2.w = BitConverter.ToSingle(buffer, 25);
                        break;
                    case 0x13:

                        for (var i = 1; i < 29; i += stream.Read(buffer, i, 29 - i)) ;
                        accel2.x = BitConverter.ToSingle(buffer, 1);
                        accel2.y = BitConverter.ToSingle(buffer, 5);
                        accel2.z = BitConverter.ToSingle(buffer, 9);

                        rot2.x = BitConverter.ToSingle(buffer, 13);
                        rot2.y = BitConverter.ToSingle(buffer, 17);
                        rot2.z = BitConverter.ToSingle(buffer, 21);
                        rot2.w = BitConverter.ToSingle(buffer, 25);
                        var info = new Tuple<Vector3, Quaternion>(new Vector3(accel2.x, accel2.y, accel2.z), new Quaternion(rot2.x, rot2.y, rot2.z, rot2.w));
                        if(enableControls)
                        UnityExecutionThread.instance.ExecuteInMainThread(() =>
                        {
                                EventRegistry.instance.Invoke("FIRE", info);
                        });
                        break;

                    case BTN_OFF:
                        // Event library is currently NOT thread-safe
                        Debug.Log("1 finger off");
                        if (enableControls)
                        UnityExecutionThread.instance.ExecuteInMainThread(() =>
                        {
                                EventRegistry.instance.Invoke(BUTTON_OFF);
                        });
                        break;

                    case BTN_ON:
                        Debug.Log("1 finger on");
                        if (enableControls)
                        UnityExecutionThread.instance.ExecuteInMainThread(() =>
                        {
                                EventRegistry.instance.Invoke(BUTTON_ON);
                        });
                        break;

                    case BTN_2_OFF:
                        Debug.Log("2 fingers off");
                        // Event library is currently NOT thread-safe
                        if (enableControls)
                        UnityExecutionThread.instance.ExecuteInMainThread(() =>
                        {
                                EventRegistry.instance.Invoke(BUTTON_2_OFF);
                        });
                        break;

                    case BTN_2_ON:
                        Debug.Log("2 fingers on");
                        if (enableControls)
                        UnityExecutionThread.instance.ExecuteInMainThread(() =>
                        {
                                EventRegistry.instance.Invoke(BUTTON_2_ON);
                        });
                        break;

                    case BTN_3_OFF:
                        Debug.Log("3 fingers off");
                        // Event library is currently NOT thread-safe
                        if (enableControls)
                        UnityExecutionThread.instance.ExecuteInMainThread(() =>
                        {
                                EventRegistry.instance.Invoke(BUTTON_3_OFF);
                        });
                        break;

                    case BTN_3_ON:
                        Debug.Log("3 fingers on");
                        if (enableControls)
                        UnityExecutionThread.instance.ExecuteInMainThread(() =>
                        {
                                EventRegistry.instance.Invoke(BUTTON_3_ON);
                        });
                        break;

                    case BTN_4_OFF:
                        Debug.Log("4 fingers off");
                        // Event library is currently NOT thread-safe
                        if (enableControls)
                        UnityExecutionThread.instance.ExecuteInMainThread(() =>
                        {
                                EventRegistry.instance.Invoke(BUTTON_4_OFF);
                        });
                        break;

                    case BTN_4_ON:
                        Debug.Log("4 fingers on");
                        if (enableControls)
                        UnityExecutionThread.instance.ExecuteInMainThread(() =>
                        {
                                EventRegistry.instance.Invoke(BUTTON_4_ON);
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

        public Quaternion GetRawRotation()
        {
            return Internal.transform.localRotation;
        }

        public Quaternion GetCalibratedRotation(Quaternion info)
        {
            Internal.transform.localRotation = info;
            return GetCalibratedRotation();
        }

        public Quaternion GetInnerRotation()
        {
            return Post.transform.localRotation;
        }

        void OnDestroy()
        {
            terminated = true;
            if(udpClient!=null) udpClient.Close();
            if(tcpClient!=null) tcpClient.Close();
        }

        public void SetOrientationForce(Quaternion rot)
        {
            StartCoroutine(_setOrientation(rot));
        }

        UnityCoroutine _setOrientation(Quaternion rot)
        {
            Internal.transform.localRotation = rot;
            quatblocked = true;
            yield return new WaitForSeconds(0.1f);

            EventRegistry.instance.Invoke("QSet");

            yield return new WaitForSeconds(0.1f);
            quatblocked = false;
        }

        public void EnableControls()
        {
            enableControls = true;
            Sprite.SetActive(false);
        }

        // Update is called once per frame
        void Update () {
            if (quatblocked) return;
            Internal.transform.localRotation = rot;
	    }
    }
}