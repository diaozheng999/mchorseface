namespace PGT.Core
{
    using System.Collections.Generic;
    using System.Threading;
    using UnityEngine;
    using System;
    using Func;

    public enum ReturnState { Done, Keep }

    public class WaitForEvent : CustomYieldInstruction
    {
        bool wait;
        public override bool keepWaiting { get
            {
                return wait;
            }
        }

        public WaitForEvent(string evt)
        {
            wait = true;
            EventRegistry.instance.AddEventListener(evt, () => wait = false);
        }
    }

    public class EventRegistry
    {

        //singleton
        private static EventRegistry _instance = null;

        public static EventRegistry instance
        {
            get
            {
                if (_instance == null) _instance = new EventRegistry();
                return _instance;
            }
        }

        public static void Reset()
        {
            _instance = null;
        }

        //singleton members
        public delegate ReturnState Callback(object t);
        private Dictionary<string, Dictionary<int, Callback>> registry;
        int counter;

        int gc_counter;
        int gc_max = 100;

        //bool flag;
        struct Event
        {
            public string evt;
            public object param;
        }
        Heap<float, Event> EventQueue;

        Queue<Tuple<string, object, bool>> InvokeSet;

        float time;

        Dictionary<string, object> lateEvents;

        //constructor
        private EventRegistry()
        {
            registry = new Dictionary<string, Dictionary<int, Callback>>();
            counter = 0;
            EventQueue = new Heap<float, Event>();
            //flag = false;
            lateEvents = new Dictionary<string, object>();
            InvokeSet = new Queue<Tuple<string, object, bool>>();
        }

        public int AddEventListener(string Event, Callback listener)
        {
            //check for late listeners
            if (lateEvents != null && lateEvents.ContainsKey(Event) &&
                listener.Invoke(lateEvents[Event]) == ReturnState.Done)
            {
                return -1;
            }
            if (!registry.ContainsKey(Event))
            {
                registry.Add(Event, new Dictionary<int, Callback>());
            }
            registry[Event].Add(this.counter, listener);
            int id = this.counter++;
            return id;
        }


        public int AddEventListener(string Event, Lambda listener, bool persistent = false)
        {
            ReturnState persistence = persistent ? ReturnState.Keep : ReturnState.Done;
            return AddEventListener(Event, (object _flag) =>
            {
                listener.Invoke();
                return persistence;
            });
        }


        public int AddEventListener(string Event, Action<object> listener, bool persistent = false)
        {
            ReturnState persistence = persistent ? ReturnState.Keep : ReturnState.Done;
            return AddEventListener(Event, (object obj) =>
            {
                listener.Invoke(obj);
                return persistence;
            });
        }

        public int AddEventListener(string Event, Future listener, bool persistent = false)
        {
            ReturnState persistence = persistent ? ReturnState.Keep : ReturnState.Done;
            return AddEventListener(Event, (object _flag) =>
            {
                listener.bind();
                return persistence;
            });
        }

        public int AddEventListener(string Event, Continuation<object> listener, bool persistent = false)
        {
            ReturnState persistence = persistent ? ReturnState.Keep : ReturnState.Done;
            return AddEventListener(Event, (object _flag) =>
            {
                listener.apply(_flag).bind();
                return persistence;
            });
        }

        public void Invoke(string Event, object param = null, bool allowLateListeners = false)
        {
            InvokeSet.Enqueue(new Tuple<string, object, bool>(Event, param, allowLateListeners));
        }

        void _invoke(Tuple<string, object, bool> par)
        {
            _invoke(par.car, par.cdr, par.cpr);
        }

        void _invoke(string Event, object param = null, bool allowLateListeners = false)
        {
            if (allowLateListeners)
            {
                lateEvents.Add(Event, param);
            }
            if (!registry.ContainsKey(Event))
            {
                return;
            }
            List<int> removals = new List<int>();
            foreach(KeyValuePair<int, Callback> listener in registry[Event])
            {
                if (listener.Value.Invoke(param) == ReturnState.Done)
                    removals.Add(listener.Key);
            }

            foreach(int removal in removals)
            {
                registry[Event].Remove(removal);
            }

            gc_counter++;
            if (gc_counter == gc_max) Clean();
        }

        public void DisableLateListeners(string Event)
        {
            if (lateEvents != null &&
                lateEvents.ContainsKey(Event))
                lateEvents.Remove(Event);
        }

        public int GetListenerCount(string Event)
        {
            if (!registry.ContainsKey(Event)) return 0;
            return registry[Event].Count;
        }

        public Tuple<string, int> SetTimeout(float seconds, Callback callback, object param = null)
        {
            string evt = Guid.NewGuid().ToString();
            int id = AddEventListener(evt, callback);

            Tuple<string, int> ret = new Tuple<string, int>(evt, id);

            InvokeAfter(seconds, evt, param);
            return ret;
        }

        public Tuple<string, int> SetTimeout(float seconds, Lambda callback)
        {
            return SetTimeout(seconds, (object _flag) =>
            {
                callback.Invoke();
                return ReturnState.Done;
            });
        }

        public Tuple<string, int> SetTimeout<T>(float seconds, Action<T> callback, T param)
        {
            return SetTimeout(seconds, () =>
            {
                callback.Invoke(param);
            });
        }

        public Tuple<string, int> SetTimeout(float seconds, Future callback)
        {
            return SetTimeout(seconds, callback.bind);
        }

        public Tuple<string, int> SetTimeout<T>(float seconds, Continuation<T> callback, T param)
        {
            return SetTimeout(seconds, callback.apply(param).bind);
        }

        public void InvokeAfter(float seconds, string Event, object param = null)
        {
            if (EventQueue.Count == 0)
            {
                time = 0;
            }
            Event e = new EventRegistry.Event();
            e.evt = Event;
            e.param = param;
            EventQueue.Insert(e, time + seconds);
        }

        public void RemoveEventListener(string Event, int id = -1)
        {
            if (!registry.ContainsKey(Event)) return;
            if (id < 0) registry[Event] = new Dictionary<int, Callback>();
            if (!registry[Event].ContainsKey(id)) return;
            else registry[Event].Remove(id);
        }

        public void Update(float deltaTime)
        {
            while (InvokeSet.Count > 0)
            {
                _invoke(InvokeSet.Dequeue());
            }

            if (EventQueue.Count > 0 && deltaTime > 0)
            {
                time += deltaTime;
                while (EventQueue.Count > 0 && EventQueue.Peek().Key < time)
                {
                    Event e = EventQueue.DeleteMin().Value;
                    _invoke(e.evt, e.param);
                }

            }
        }


        public void Clean()
        {
            List<string> clean = new List<string>();
            foreach(KeyValuePair<string, Dictionary<int, Callback>> p in registry)
            {
                if (p.Value.Count == 0) clean.Add(p.Key);
            }
            foreach(string evt in clean)
            {
                registry.Remove(evt);
            }
            gc_counter = 0;
            Debug.Log("Cleaned " + clean.Count + " unused event(s).");
        }
    }

    class EventRegistryTimerUpdater : MonoBehaviour
    {
        void Update()
        {
            EventRegistry.instance.Update(Time.deltaTime);
        }
    }
}