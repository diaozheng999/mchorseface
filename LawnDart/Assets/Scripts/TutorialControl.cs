using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TutorialControl : MonoBehaviour {

    public static bool spacePressed;
    public static bool firstPress;
    public static bool secondPress;
    public static bool thirdPress;
    public static bool welcomeOn;
    public static int timer;
    public static int timerStart;
    public static bool checkTimeDiff;
    public static bool finishTutorial;
    public static bool passed;

	// Use this for initialization
	void Start () {
	
	}

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyUp("space"))
        {
            checkTimeDiff = true;
            timerStart = timer;
            if (!secondPress & !thirdPress & !firstPress)
            {
                firstPress = true;
            }
            else if (firstPress & !thirdPress & !secondPress)
            {
                secondPress = true;
                firstPress = false;
            }
            else if (!firstPress & !thirdPress & secondPress)
            {
                secondPress = false;
                thirdPress = true;
            }
            else if (!firstPress & thirdPress & !secondPress)
            {
                finishTutorial = true;
                thirdPress = false;
            }
        }
        if (finishTutorial)
        {
            SceneManager.LoadScene(1);
        }

    }

    void OnGUI()
    {
        // bools cause I don't know how else to logic

        welcomeOn = true;
        GUIStyle textStyle = new GUIStyle();
        GUIStyle titleStyle = new GUIStyle();

        var text = "";

        if (welcomeOn)
        {
            text = "Welcome to Lawn Darts with Mii! \n\n Press the space bar to continue";
        }

        if (firstPress)
        {
            text = "Swing Wii Remote upward to Throw a Dart \n \n When you are comfortable throwing darts press Space to continue";
        }
        if (secondPress)
        {
            text = "Now try to get the lawn dart into the circle";
        }
        if (thirdPress)
        {
            text = "Great! You have finished the Tutorial! Press Space to go the real game.";
        }

        textStyle.fontSize = 20;
        textStyle.alignment = TextAnchor.UpperCenter;

        titleStyle.fontSize = 30;
        titleStyle.alignment = TextAnchor.UpperCenter;
     

        GUI.Label(new Rect(0, 10, Screen.width, 40), "TUTORIAL", titleStyle);
        GUI.Label(new Rect(0, 40, Screen.width, Screen.height), text, textStyle);

    }
}
