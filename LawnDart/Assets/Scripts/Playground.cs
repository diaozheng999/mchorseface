using UnityEngine;
using PGT.Core;

namespace McHorseface.LawnDart
{
    class Playground : MonoBehaviour
    {
        public void KillAll()
        {
            EventRegistry.instance.Invoke("killall");
        }
    }
}
