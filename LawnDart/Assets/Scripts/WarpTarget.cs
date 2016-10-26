using UnityEngine;
using UnityEngine.SceneManagement;
using PGT.Core;

namespace McHorseface.LawnDart
{
    public class WarpTarget : MonoBehaviour
    {
        public const string WARP = "warp";

		public static bool loading = false;

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
			loading = false;
        }

		//128.237.215.182

        void OnTriggerEnter (Collider other)
        {
            Debug.Log("Target Collided!");
            var dartController = other.GetComponent<DartController>();
            Debug.Log(dartController);
			if (dartController != null) {
				Debug.Log ("target: dartTriggerEnter:" + dartController.dartTriggerEnter + ", targetTriggerEnter:" + dartController.targetTriggerEnter);
			}	
            if (other.gameObject.layer == dartlayer && dartController!=null && (
				(dartController.dartTriggerEnter && !dartController.targetTriggerEnter) ||
				(dartController.targetTriggerEnter && !dartController.dartTriggerEnter)
			))
            {
                Debug.Log(other.gameObject.name);
                if (SceneChange)
                {
					Player.instance.Teleport(transform.position);
					if(!loading)
                    	SceneManager.LoadSceneAsync(NextScene);
					GetComponent<Collider> ().enabled = false;
					loading = true;
                }else
                {
                    Player.instance.Teleport(transform.position);
                    EventRegistry.instance.Invoke(WARP, instance);
                }

                gameObject.SetActive(false);


				dartController.targetTriggerEnter = true;

            }
        }

        void OnDestroy()
        {
            EventRegistry.instance.RemoveEventListener(WARP, targetListener);
        }
    }
}