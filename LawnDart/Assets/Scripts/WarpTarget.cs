using UnityEngine;
using UnityEngine.SceneManagement;
using PGT.Core;

namespace McHorseface.LawnDart
{
    public class WarpTarget : MonoBehaviour
    {
        public const string WARP = "warp";

        [SerializeField]
        bool SceneChange = true;
        [SerializeField]
        string NextScene = "GameScene";

        static long _instance = 0;

        [SerializeField]
        Material sceneChangeTexture;

        [SerializeField]
        Material teleportTexture;

        int dartlayer = -1;

        int targetListener;
        long instance;

        void Start ()
        {
            dartlayer = LayerMask.NameToLayer("LawnDart");
            
            foreach (var m in GetComponentsInChildren<MeshRenderer>())
            {
                m.material = SceneChange ? sceneChangeTexture : teleportTexture;
            }
            instance = ++_instance;
            targetListener = EventRegistry.instance.AddEventListener(WARP,
                (e) =>
                {
                    if (instance != (long)e)
                    {
                        gameObject.SetActive(true);
                    }
                }, true);
        }

        void OnTriggerEnter (Collider other)
        {
            Debug.Log("Collided!");
            var dartController = other.GetComponent<DartController>();
            Debug.Log(dartController);
            if (other.gameObject.layer == dartlayer && dartController!=null && (dartController.hitWarpTarget || !dartController.hitGround))
            {
                Debug.Log(other.gameObject.name);
                if (SceneChange)
                {
                    SceneManager.LoadScene(NextScene);
                }else
                {
                    Player.instance.Teleport(transform.position);
                    EventRegistry.instance.Invoke(WARP, instance);
                }

                gameObject.SetActive(false);

                if (dartController.hitWarpTarget)
                {
                    dartController.hitWarpTarget = false;
                }
            }
        }

        void OnDestroy()
        {
            EventRegistry.instance.RemoveEventListener(WARP, targetListener);
        }
    }
}