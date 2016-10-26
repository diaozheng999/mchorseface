using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using PGT.Core;

public class LoadGame : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void loadGame()
    {
        SceneManager.LoadScene("Level1");
    }

    public void loadGame2()
    {
        SceneManager.LoadScene("GameScene");
    }

	public void loadGame3(){
		SceneManager.LoadScene ("PortalTest");
	}


	public void KILLL(){
		EventRegistry.instance.Invoke ("killall");
	}
}
