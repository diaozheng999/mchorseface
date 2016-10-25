using PGT.Core;
using UnityEngine;

namespace McHorseface.LawnDart
{
    public class BlankBoard : MiiAnimationController
    {

        public GameObject go;


        void Start()
        {
            // do nothing;
        }

        void Update()
        {

        }

        void Fragment()
        {
            EventRegistry.instance.Invoke(MiiAnimationController.MII_HIT);
            Destroy(go);
        }
    }
}