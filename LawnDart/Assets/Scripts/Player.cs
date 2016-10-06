using UnityEngine;
using System.Collections;


namespace McHorseface.LawnDart
{
    public class Player : MonoBehaviour
    {
        public static Player instance;
        [SerializeField]
        GameObject black;

        [SerializeField]
        bool tryout = false;
        
        void Awake()
        {
            instance = this;
        }

        public IEnumerator teleport(Vector3 pos)
        {
            if (tryout) yield break;
            black.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            transform.position = pos;
            black.SetActive(false);
        }
    }

}