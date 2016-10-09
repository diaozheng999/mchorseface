using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PGT.Core;


namespace McHorseface.LawnDart
{
    public class Player : MonoBehaviour
    {
        public static Player instance;
        [SerializeField]
        GameObject black;

        [SerializeField]
        bool tryout = false;

        [SerializeField]
        GameObject Callie;


        Queue<Vector3> tp_dest;

        int callie_listener;


        void Awake()
        {
            instance = this;
            tp_dest = new Queue<Vector3>();
        }

        void Start()
        {
            callie_listener = EventRegistry.instance.AddEventListener(CalibrationMiiController.CALLIE_HIT, () =>
            {
                var dup = Instantiate(Callie);
                dup.transform.position = transform.position + 3f * (Camera.main.transform.forward + Camera.main.transform.right) + 5f * Vector3.up;
            }, true);
        }

        void OnDestroy()
        {
            EventRegistry.instance.RemoveEventListener(CalibrationMiiController.CALLIE_HIT, callie_listener);
        }

        public void Teleport(Vector3 pos)
        {
            Debug.Log("Player: Teleporting");
            tp_dest.Enqueue(pos);
            StartCoroutine(teleport());
        }

        IEnumerator teleport()
        {
            while(tp_dest.Count > 0)
            {
                var pos = tp_dest.Dequeue();
                black.SetActive(true);
                yield return new WaitForSeconds(0.5f);
                transform.position = pos;
                black.SetActive(false);
                CalibrationMiiController.instance.Reposition(pos);
                yield return new WaitForSeconds(1f);
            }
        }
    }

}