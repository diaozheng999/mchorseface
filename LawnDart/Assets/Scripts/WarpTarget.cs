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

        [SerializeField]
        Material sceneChangeTexture;

        [SerializeField]
        Material teleportTexture;

        int dartlayer = -1;

        void Start ()
        {
            dartlayer = LayerMask.NameToLayer("LawnDart");
            
            foreach (var m in GetComponentsInChildren<MeshRenderer>())
            {
                m.material = SceneChange ? sceneChangeTexture : teleportTexture;
            }

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
                    Player.instance.Teleport(transform.position);
                }

                Destroy(gameObject);
            }
        }
    }
}