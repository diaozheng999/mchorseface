using PGT.Core.Func;

namespace McHorseface.SlideManager
{
    abstract class BasicSlide : Slide
    {

        public override void Show(Lambda continuation)
        {
            gameObject.SetActive(true);
            continuation();
        }
        public override void Hide(Lambda continuation)
        {
            gameObject.SetActive(false);
            continuation();
        }
    }
}
