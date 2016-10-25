using PGT.Core;
using UnityEngine;

namespace McHorseface.LawnDart
{
    public class BlankBoard : MiiAnimationController
    {

        public GameObject go;


        void Start()
        {
            doBlood = false;
        }

        void Update()
        {

        }

        public override void Fragment(Vector3 position)
        {
            EventRegistry.instance.Invoke(MII_HIT);
            EventRegistry.instance.Invoke("pacifist");
            Destroy(go);
        }
    }
}