using UnityEngine;
using PGT.Core.Func;

namespace McHorseface.SlideManager
{
    abstract class Slide : MonoBehaviour
    {
        protected void Start()
        {
            Hide(Function.noop);
        }

        public abstract void Show(Lambda continuation);
        public abstract void Hide(Lambda continuation);
    }
}
