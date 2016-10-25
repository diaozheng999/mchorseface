using UnityEngine;
using System.Collections;
using UnityCoroutine = System.Collections.IEnumerator;
using PGT.Core;


namespace McHorseface.LawnDart
{
    enum MiiGender {
        Male, Female, Random
    }
    public class MiiAnimationController : MonoBehaviour {

        [SerializeField]
        MiiGender gender = MiiGender.Random;

        [SerializeField]
        GameObject[] maleBodies;

        [SerializeField]
        GameObject[] femaleBodies;

        [SerializeField]
        GameObject[] maleHair;

        [SerializeField]
        GameObject[] femaleHair;

		[SerializeField]
		AudioClip hitSound;

		[SerializeField]
		AudioClip[] woahSound;

		[SerializeField]
		AudioClip[] utterance;

        protected Animator anim;
        string[] waves = { "DoWave0", "DoWave1", "DoWave2" };

        [SerializeField]
        protected float waveChance = 0.02f;

		[SerializeField]
		protected float soundChance = 0.04f;

        protected bool doWave = true;

        public bool alive = true;

        GameObject activeBody;

        [SerializeField]
        protected Rigidbody collapsed;
        [SerializeField]
        Collider collapsedCollider;
        

		[SerializeField]
		float excitement = 1;

		float baseExcitement = 0;

        [SerializeField]
        protected float blinkChance = 0.2f;

        [SerializeField]
        protected MeshRenderer head;

        public static string MII_HIT = "mii_ouch";

		new protected AudioSource audio;

		int hitListener;

	    // Use this for initialization
	    void Start () {
            //disable all body and hair types
            foreach (var b in maleBodies) b.SetActive(false);
            foreach (var b in femaleBodies) b.SetActive(false);
            foreach (var b in maleHair) b.SetActive(false);
            foreach (var b in femaleHair) b.SetActive(false);

            foreach (var c in GetComponentsInChildren<Collider>())
            {
                c.enabled = false;
            }

            // do dice roll if gender== random
            if (gender == MiiGender.Random)
            {
                gender = Random.value > 0.5 ? MiiGender.Male : MiiGender.Female;
            }
            // select a body type
            int body_id, hair_id;
            switch (gender)
            {
                case MiiGender.Male:
                    body_id = Mathf.FloorToInt(Random.value * maleBodies.Length);
                    maleBodies[body_id].SetActive(true);
                    
                    hair_id = Mathf.FloorToInt(Random.value * maleHair.Length);
                    
                    maleHair[hair_id].SetActive(true);


                    // set a random body and hair colour
                    maleBodies[body_id].GetComponent<MeshRenderer>().material.color = Random.ColorHSV(0, 1, 0.7f, 1);
                    maleHair[hair_id].GetComponent<MeshRenderer>().material.color = Random.ColorHSV();


                    activeBody = maleBodies[body_id];
                    break;

                case MiiGender.Female:
                    body_id = Mathf.FloorToInt(Random.value * femaleBodies.Length);
                    femaleBodies[body_id].SetActive(true);


                    hair_id = Mathf.FloorToInt(Random.value * maleHair.Length);
                    femaleHair[hair_id].SetActive(true);

                    femaleBodies[body_id].GetComponent<MeshRenderer>().material.color = Random.ColorHSV(0, 1, 0.7f, 1);
                    femaleHair[hair_id].GetComponent<MeshRenderer>().material.color = Random.ColorHSV();

                    activeBody = femaleBodies[body_id];
                    break;
            }

            anim = GetComponent<Animator>();

			baseExcitement = Random.value;


            StartCoroutine(DoWave());

            EventRegistry.instance.AddEventListener("killall", () => Fragment(Vector3.zero), false);

			audio = GetComponent<AudioSource> ();
			audio.pitch += Random.value - 0.5f;

			hitListener = EventRegistry.instance.AddEventListener (LawnDartLauncher.DART_LAUNCH, () => {
				var accel = LDController.instance.Accel.sqrMagnitude / 10;
				this.SetTimeout(Random.value / 10f, () => {
					excitement += accel * (1+Random.value / 10);

					var woah_id = Mathf.FloorToInt(Random.value * woahSound.Length);
					audio.clip = woahSound[woah_id];

					audio.Play();
				});
			}, true);
	    }
	
        protected virtual UnityCoroutine DoWave ()
        {
            yield return new WaitForEndOfFrame();
            while (doWave && alive)
            {
				bool playSound = false;
                if (Random.value < waveChance)
                {
                    anim.SetTrigger(waves[Mathf.FloorToInt(Random.value * 3)]);
					playSound = true;
                }


				if (playSound || (Random.value < soundChance && !audio.isPlaying))
				{
					
					var sound_id = Mathf.FloorToInt (Random.value * utterance.Length);
					audio.clip = utterance[sound_id];
					audio.Play ();
				}

                yield return new WaitForSeconds(Random.value);
            }

            doWave = false;
        }

        public virtual void Fragment(Vector3 position)
        {
            if (!alive) return;
            alive = false;

			audio.clip = hitSound;
			audio.Play ();
			EventRegistry.instance.RemoveEventListener (LawnDartLauncher.DART_LAUNCH, hitListener);
            EventRegistry.instance.Invoke(MII_HIT);
            anim.Stop();
            collapsed.isKinematic = true;
            collapsedCollider.enabled = false;
            foreach (var c in GetComponentsInChildren<Collider>())
            {
                c.enabled = true;
            }
            foreach (var rb in GetComponentsInChildren<Rigidbody>())
            {
                rb.isKinematic = false;
                rb.AddExplosionForce(100f, position, 20, 10f, ForceMode.Acceleration);
            }

            this.SetTimeout(10f, () =>
            {
				
                Destroy(collapsed.gameObject);
            });
        }
        

		void Update(){
			anim.SetFloat("IdleSpeed", 2 +  (excitement - 0.5f));
			audio.volume = excitement / 2;
			excitement = Mathf.Lerp (excitement, baseExcitement, Time.deltaTime);
		}

        void OnEnable()
        {
            if (!doWave)
            {
                doWave = true;
                StartCoroutine(DoWave());
            }
        }

		void OnDestroy(){
			EventRegistry.instance.RemoveEventListener (LawnDartLauncher.DART_LAUNCH, hitListener);
		}
    }
}

