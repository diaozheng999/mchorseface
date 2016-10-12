using UnityEngine;
using PGT.Core;
using PGT.Core.Func;
using System.Collections.Generic;

namespace McHorseface.SlideManager
{
    class SlideShow : MonoBehaviour
    {
        [SerializeField]
        Slide[] slides;
        int currentSlide;
        bool inTransition;

        [SerializeField]
        bool queueWhenBusy = false;
        bool crossFade = false;

        Queue<Lambda> actionQueue;
        

        void Start()
        {
            currentSlide = 0;
            inTransition = false;
            actionQueue = new Queue<Lambda>();
        }
        
        public void NextSlide()
        {
            if (inTransition)
            {
                if (queueWhenBusy) actionQueue.Enqueue(NextSlide);
                return;
            }
            
            if(currentSlide < slides.Length - 1)
            {
                SwapSlides(currentSlide, ++currentSlide);
            }
        }

        public void PrevSlide()
        {
            if(inTransition)
            {
                if (queueWhenBusy) actionQueue.Enqueue(NextSlide);
                return;
            }
            if(currentSlide > 0)
            {
                SwapSlides(currentSlide, --currentSlide);
            }
        }

        void HideSlide(int i)
        {
            inTransition = true;
            slides[i].Hide(OnTransitionFinish);
        }

        void ShowSlide(int i)
        {
            inTransition = true;
            slides[i].Show(OnTransitionFinish);
        }

        

        void SwapSlides(int i, int j)
        {
            inTransition = true;
            if (crossFade)
            {
                bool fadeIn = false;
                bool fadeOut = false;
                slides[i].Hide(() =>
                {
                    fadeOut = true;
                    if (fadeIn) OnTransitionFinish();
                });
                slides[j].Show(() =>
                {
                    fadeIn = true;
                    if (fadeOut) OnTransitionFinish();
                });
            }
            else
            {
                slides[i].Hide(() => slides[j].Show(OnTransitionFinish));
            }
        }


        void OnTransitionFinish()
        {
            inTransition = false;
            if (actionQueue != null)
            {
                actionQueue.Dequeue()();
            }
        }
    }
}
