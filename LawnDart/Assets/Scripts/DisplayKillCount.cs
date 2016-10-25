using UnityEngine;
using UnityEngine.UI;

namespace McHorseface.LawnDart
{
    public class DisplayKillCount : MonoBehaviour
    {

        Text text;

        void Start()
        {
            text = GetComponent<Text>();
        }

        void Update()
        {
            text.text = KillCounter.instance.killCount.ToString();
        }
    }
}