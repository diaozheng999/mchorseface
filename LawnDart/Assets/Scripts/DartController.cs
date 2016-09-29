using UnityEngine;
using System.Collections;

public class DartController : MonoBehaviour {
    private Rigidbody rb;

	// Use this for initialization
	void Start () {
        rb = gameObject.GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        gameObject.transform.LookAt(transform.position + rb.velocity);
        //gameObject.transform.eulerAngles = rb.velocity;
	}

    void OnTriggerEnter(Collider other)
    {
        gameObject.BroadcastMessage("BecomeKinematic");
    }
    void BecomeKinematic()
    {
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
    }
}
