namespace PGT.Core
{
    using UnityEngine;

    class FramerateLimiter : MonoBehaviour
    {
        public bool limitFramerate = false;
        public int targetFramerate = 15;

        void Awake()
        {
            if (limitFramerate)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = targetFramerate;
            }
        }
    }
}
