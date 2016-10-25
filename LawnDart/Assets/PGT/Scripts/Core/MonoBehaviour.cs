using System.Collections.Generic;
using PGT.Core.Func;
namespace PGT.Core
{
    public class Behaviour : SyncMonoBehaviour
    {
        List<Tuple<string, int>> registeredEventListeners = null;
        
        void InitRegisteredEvents()
        {
            if(registeredEventListeners == null)
            registeredEventListeners = new List<Tuple<string, int>>();
        }

        protected void AddEventListener<T>(string evt, System.Action<T> handler, bool persistent = false, bool exec_when_disabled = false)
        {
            InitRegisteredEvents();


            System.Action<object> _handler;

            if (exec_when_disabled)
            {
                _handler = (object state) =>
                {
                    handler((T)state);
                };
            }else
            {
                _handler = (object state) =>
                {
                    if (enabled) handler((T)state);
                };
            }


            var p = new Tuple<string, int>(
                evt, EventRegistry.instance.AddEventListener(evt, _handler, persistent)
            );

            registeredEventListeners.Add(p);
        }

        protected void AddEventListener(string evt, Lambda handler, bool persistent = false, bool exec_when_disabled = false)
        {
            InitRegisteredEvents();


            System.Action<object> _handler;

            if (exec_when_disabled)
            {
                _handler = (object state) =>
                {
                    handler();
                };
            }
            else
            {
                _handler = (object state) =>
                {
                    if (enabled) handler();
                };
            }


            var p = new Tuple<string, int>(
                evt, EventRegistry.instance.AddEventListener(evt, _handler, persistent)
            );

            registeredEventListeners.Add(p);
        }


        protected void UnregisterEventListeners()
        {
            if (registeredEventListeners != null)
            {
                foreach(var e in registeredEventListeners)
                {
                    EventRegistry.instance.RemoveEventListener(e.car, e.cdr);
                }
            }
        }
        

        protected virtual void OnDestroy()
        {
            UnregisterEventListeners();
        }
    }
}
