using UnityEngine;
using UnityCoroutine = System.Collections.IEnumerator;
using PGT.Core;

namespace McHorseface.LawnDart
{
    public class CalibrationMiiController : MiiAnimationController
    {
        [SerializeField]
        float CalibrationAnimProb = 0.5f;

        [SerializeField]
        Texture normalFace;

        [SerializeField]
        Texture blinkFace;

        [SerializeField]
        Texture aimFace;

        bool isBlinking = false;
        bool isAiming = false;

        void Start ()
        {
            anim = GetComponent<Animator>();

            anim.SetFloat("IdleSpeed", 2 + (Random.value - 0.5f));

            StartCoroutine(DoWave());

            EventRegistry.instance.AddEventListener("killall", () => Fragment(Vector3.zero), false);
        }

        protected override UnityCoroutine DoWave()
        {
            yield return new WaitForEndOfFrame();
            while (doWave)
            {
                if(Random.value < waveChance)
                {
                    if(Random.value < CalibrationAnimProb)
                    {
                        anim.SetTrigger("DoCalib");
                    }else
                    {
                        anim.SetTrigger("DoWave2");
                    }
                }
                yield return new WaitForSeconds(Random.value);
                
                if(Random.value < blinkChance)
                {
                    head.material.mainTexture = blinkFace;
                    yield return new WaitForSeconds(0.2f * Random.value);
                    head.material.mainTexture = isAiming? aimFace : normalFace;
                }
            }
        }

        public void ChangeHeadTexture()
        {
            head.material.mainTexture = aimFace;
            isAiming = true;
        }

        public void ChangeHeadTextureBack()
        {
            head.material.mainTexture = normalFace;
            isAiming = false;
        }
    }
}

