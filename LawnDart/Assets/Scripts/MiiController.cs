using UnityEngine;
using System.Collections;


namespace McHorseface.LawnDart
{
    
    public class MiiController : MonoBehaviour {

        [SerializeField]
        GameObject distanceIndicator;
        Camera cam;

        [SerializeField]
        float xViewRange = 0.1f;

        [SerializeField]
        float yViewRange = 0.1f;

        Vector3 p = new Vector3(0.5f, 0.5f, 0);

	    // Use this for initialization
	    void Start () {
            cam = Camera.main;
	    }
	
	    // Update is called once per frame
	    void Update () {
            //compute whether the mii is in camera view

            Vector3 point = cam.WorldToViewportPoint(transform.position) - p;
            

            if(Mathf.Abs(point.x) < xViewRange && Mathf.Abs(point.y) < yViewRange)
            {
                if (!distanceIndicator.activeSelf)
                {
                    distanceIndicator.SetActive(true);
                }
            }else if(distanceIndicator.activeSelf)
            {
                distanceIndicator.SetActive(false);
            }

	    }
    }
}
