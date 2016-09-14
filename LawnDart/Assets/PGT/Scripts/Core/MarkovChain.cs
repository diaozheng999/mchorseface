namespace PGT.Core
{
    using System.Collections.Generic;
    using Markov;
    using System;

    namespace Markov
    {
        interface IState
        {
            void OnEnter(IState from=null);
            void OnLeave(IState to=null);
        } 

        class DimensionMismatchException : Exception { }
    }

    class MarkovChain
    {
        Dictionary<int, string> names;
        IState[] states;
        float[,] delta;
        int size;
        int currState;

        public MarkovChain(int size, int currState)
        {
            states = new IState[size];
            delta = new float[size,size];
            this.size = size;
            this.currState = currState;
        }

        public void SetState(int i, IState state)
        {
            states[i] = state;
        }

        public void SetTransitionProb(int i, int j, float prob)
        {
            delta[i, j] = prob;
        }

        public void Begin()
        {
            states[currState].OnEnter(null);
        }

        public void Transition()
        {
            float trans = UnityEngine.Random.value;
            for(int i=0;i< size; i++)
            {
                if(delta[currState, i] > trans)
                {
                    HardTransition(i);
                    break;
                }
                trans -= delta[currState, i];
            }
        }

        public void HardTransition(int toState)
        {
            int _currState = currState;
            currState = toState;
            states[_currState].OnLeave(states[toState]);
            states[toState].OnEnter(states[_currState]);
        }
    }
}
