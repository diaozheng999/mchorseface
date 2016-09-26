using UnityEngine;
using System.Collections;

public class DontDestroy : MonoBehaviour {

    public GameObject dart;

    void Awake()
    {
        DontDestroyOnLoad(dart);
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
