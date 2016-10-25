using PGT.Core;
using UnityEngine;

namespace McHorseface.LawnDart
{
    public class KillCounter : PGT.Core.Behaviour
    {

        public static KillCounter instance = null;

        public int killCount = 0;



        void Start()
        {
            if(instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            AddEventListener(MiiAnimationController.MII_HIT, () =>
            {
                killCount++;
                Debug.Log("KillCount: " + killCount);
            }, true);
            AddEventListener("pacifist", () =>
            {
                killCount--;
                Debug.Log("KillCount: " + killCount);
            }, true);
        }


        
    }
}
