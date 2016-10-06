using UnityEngine;
using UnityEngine.SceneManagement;
using PGT.Core;

namespace McHorseface.LawnDart
{
    public class WarpTarget : MonoBehaviour
    {
        [SerializeField]
        bool SceneChange = true;
        [SerializeField]
        string NextScene = "GameScene";

        int dartlayer = -1;

        void Start ()
        {
            dartlayer = LayerMask.NameToLayer("LawnDart");
        }


        void OnTriggerEnter (Collider other)
        {
            if (other.gameObject.layer == dartlayer)
            {
                if (SceneChange)
                {
                    SceneManager.LoadScene(NextScene);
                }else
                {
                    Player.instance.teleport(transform.position);
                }
            }
        }
    }
}