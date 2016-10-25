using UnityEngine;
using UnityEngine.UI;

namespace McHorseface.LawnDart
{
    public class DistanceIndicator : MonoBehaviour
    {

        Camera cam;

        [SerializeField]
        float scaleFactor;

        [SerializeField]
        Text distanceIndicator;

        [SerializeField]
        bool imperial = false;

        [SerializeField]
        bool scales = true;

        const float M_TO_FT = 0.3048f;

        // Use this for initialization
        void Start()
        {
            cam = Camera.main;
        }

        // Update is called once per frame
        void Update()
        {
            transform.LookAt(cam.transform);
            var dist = Vector3.Distance(cam.transform.position, transform.position);
            var distStr = "";
            if (!imperial)
            {
                distStr += dist.ToString("N1") + "m";
            }
            else
            {
                var ft = dist * M_TO_FT;
                distStr += Mathf.Floor(ft) + "'";

                var inch = (ft - Mathf.Floor(ft)) * 12;
                distStr += Mathf.Round(inch) + "\"";
            }

            distanceIndicator.text = distStr;

            if(scales) transform.localScale = scaleFactor * dist * Vector3.one;
        }
    }
}

