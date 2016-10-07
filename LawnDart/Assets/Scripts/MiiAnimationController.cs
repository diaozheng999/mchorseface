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

        protected Animator anim;
        string[] waves = { "DoWave0", "DoWave1", "DoWave2" };

        [SerializeField]
        protected float waveChance = 0.02f;

        protected bool doWave = true;

        public bool alive = true;

        GameObject activeBody;

        [SerializeField]
        protected Rigidbody collapsed;
        [SerializeField]
        Collider collapsedCollider;
        

        [SerializeField]
        protected float blinkChance = 0.2f;

        [SerializeField]
        protected MeshRenderer head;

        public static string MII_HIT = "mii_ouch";

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

            anim.SetFloat("IdleSpeed", 2 +  (Random.value - 0.5f));

            StartCoroutine(DoWave());

            EventRegistry.instance.AddEventListener("killall", () => Fragment(Vector3.zero), false);
	    }
	
        protected virtual UnityCoroutine DoWave ()
        {
            yield return new WaitForEndOfFrame();
            while (doWave)
            {
                if (Random.value < waveChance)
                {
                    anim.SetTrigger(waves[Mathf.FloorToInt(Random.value * 3)]);
                }
                yield return new WaitForSeconds(Random.value);
            }

            doWave = false;
        }

        public virtual void Fragment(Vector3 position)
        {
            if (!alive) return;
            alive = false;
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
        }
        

        void OnEnable()
        {
            if (!doWave)
            {
                doWave = true;
                StartCoroutine(DoWave());
            }
        }
    }
}

