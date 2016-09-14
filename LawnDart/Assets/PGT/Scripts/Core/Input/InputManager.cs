using UnityEngine;
using System.Collections.Generic;
using PGT.Core;
using PGT.Core.Func;
using System.IO;
using System.Threading;
using System;

namespace PGT.Core.Input
{

    public class InputManager : SyncMonoBehaviour
    {
        public static InputManager instance = null;

        public static string MOUSE_POSITION = "_mouse_position";

        public enum KeyState { DOWN, HELD, UP }


        Dictionary<string, List<Key>> keyMap;
        Dictionary<string, List<Axis>> axisMap;

        protected AutoResetEvent readlock;

        HashSet<string> blocked;

        //Singleton
        void Awake()
        {
            //DontDestroyOnLoad(this);
            //if (instance != null) Destroy(gameObject);
            instance = this;
            readlock = new AutoResetEvent(true);

            keyMap = new Dictionary<string, List<Key>>();
            axisMap = new Dictionary<string, List<Axis>>();
            blocked = new HashSet<string>();
        }

        //raw values
        public bool GetKeyDown(string evt)
        {
            bool p = false;
            lock (keyMap)
            {
                lock (blocked)
                {
                    if (!blocked.Contains(evt) && keyMap.ContainsKey(evt))
                    {
                        foreach (var key in keyMap[evt])
                        {
                            if (key.IsDown())
                            {
                                p = true;
                                break;
                            }
                        }
                    }
                }
            }
            return p;
        }
        public bool GetKeyHeld(string evt)
        {
            bool p = false;
            lock (keyMap)
            {
                lock (blocked)
                {
                    if (!blocked.Contains(evt) && keyMap.ContainsKey(evt))
                    {
                        foreach (var key in keyMap[evt])
                        {
                            if (key.IsHeld())
                            {
                                p = true;
                                break;
                            }
                        }
                    }
                }
            }
            return p;
        }
        public bool GetKeyUp(string evt)
        {
            bool p = false;
            lock (keyMap)
            {
                lock (blocked)
                {
                    if (!blocked.Contains(evt) && keyMap.ContainsKey(evt))
                    {
                        foreach (var key in keyMap[evt])
                        {
                            if (key.IsUp())
                            {
                                p = true;
                                break;
                            }
                        }
                    }
                }
            }
            return p;
        }

        void Start()
        {
            if (UnityEngine.Input.GetJoystickNames().Length > 0)
            {
                AddKey("jump", new Key(KeyCode.Joystick1Button8));
                AddKey("fire1", new Key(9));
                AddKey("fire2", new Key(10));
                AddKey("interact", new Key(KeyCode.Joystick1Button0));
            }
        }

        void Update()
        {
            //potential deadlock with
            // BlockAll
            // BlockAllExcept
            // GetKeyDown

            lock (keyMap)
            {
                foreach(var keybind in keyMap)
                {
                    bool isBlocked = false;

                    lock (blocked)
                    {
                        if (blocked.Contains(keybind.Key))
                        {
                            isBlocked = true;
                        }
                    }
                    if (isBlocked) continue;

                    foreach(var key in keybind.Value)
                    {
                        if (key.IsDown())
                        {
                            EventRegistry.instance.Invoke(keybind.Key, KeyState.DOWN);
                        }
                        if (key.IsHeld())
                        {
                            EventRegistry.instance.Invoke(keybind.Key, KeyState.HELD);
                        }
                        if (key.IsUp())
                        {
                            EventRegistry.instance.Invoke(keybind.Key, KeyState.UP);
                        }
                    }
                }
            }
        }

        public bool JoystickConnected()
        {
            return UnityEngine.Input.GetJoystickNames().Length > 0;
        }

        public IDictionary<string, IEnumerable<string>> GetKeymap()
        {
            Dictionary<string, IEnumerable<string>> ret = new Dictionary<string, IEnumerable<string>>();
            lock (keyMap)
            {
                foreach(var keymap in keyMap)
                {
                    string[] keys = new string[keymap.Value.Count];
                    for(int i = 0; i < keymap.Value.Count; i++)
                    {
                        keys[i] = keymap.Value[i].ToString();
                    }
                    ret.Add(keymap.Key, keys);
                }
            }
            return ret;
        }

        void BlockUnsync(string evt)
        {
            if (!blocked.Contains(evt)) blocked.Add(evt);
        }

        public void Block(string evt)
        {
            lock (blocked)
            {
                BlockUnsync(evt);
            }
        }



        public void Block(IEnumerable<string> evt)
        {
            lock (blocked)
            {
                blocked.UnionWith(evt);
            }
        }

        public void BlockAll()
        {
            //avoid deadlock with Update
            lock (keyMap)
            {
                lock (blocked)
                {
                    blocked = new HashSet<string>(keyMap.Keys);
                }
            }
        }
        

        public void BlockAllExcept(string evt)
        {
            BlockAllExcept(new[] { evt });
        }

        public void BlockAllExcept(IEnumerable<string> evt)
        {
            //avoid deadlock with Update
            lock (keyMap)
            {
                lock (blocked)
                {
                    blocked.UnionWith(keyMap.Keys);

                    lock (axisMap)
                    {
                        blocked.UnionWith(axisMap.Keys);
                    }

                    blocked.ExceptWith(evt);
                }
            }

        }

        void UnblockUnsync(string evt)
        {
            if (blocked.Contains(evt)) blocked.Remove(evt);
        }

        public void Unblock(string evt)
        {
            lock (blocked)
            {
                UnblockUnsync(evt);
            }
        }

        public void Unblock(IEnumerable<string> evt)
        {
            lock (blocked)
            {
                foreach(var e in evt)
                {
                    UnblockUnsync(e);
                }
            }
        }

        public void UnblockAll()
        {
            lock (blocked)
            {
                blocked = new HashSet<string>();
            }
        }

        public void UnblockAllExcept(IEnumerable<string> evts)
        {
            HashSet<string> p = new HashSet<string>(evts);

            lock (blocked)
            {
                blocked.IntersectWith(p);
            }
        }

        public void UnblockAllExcept(string evt)
        {
            UnblockAllExcept(new[] { evt });
        }

        public void SetKeymap(IDictionary<string, IEnumerable<string>> keymap)
        {
            lock (keyMap)
            {
                keyMap = new Dictionary<string, List<Key>>();
                foreach(var kb in keymap)
                {
                    List<Key> map = new List<Key>();
                    foreach(var k in kb.Value)
                    {
                        map.Add(Key.FromString(k));
                    }

                    keyMap.Add(kb.Key, map);
                }
            }
        }

        public IEnumerable<Key> GetKeys(string evt)
        {
            Key[] keys = null;
            lock (keyMap)
            {
                if (keyMap.ContainsKey(evt))
                {
                    keys = new Key[keyMap[evt].Count];

                    for(int i = 0; i < keyMap[evt].Count; i++)
                    {
                        keys[i] = keyMap[evt][i];
                    }
                }
            }

            return keys;
        }

        public void AddKey(string evt, string k)
        {
            AddKey(evt, Key.FromString(k));
        }

        public void AddKey(string evt)
        {
            AddKey(evt, Detect());
        }

        public void AddKey(string evt, Key k)
        {
            lock (keyMap)
            {
                if (!keyMap.ContainsKey(evt))
                {
                    keyMap[evt] = new List<Key>();
                }

                keyMap[evt].Add(k);
            }
        }

        public int AddKeyWithHandler(string evt, Key k, Action<KeyState> handler)
        {
            AddKey(evt, k);
            return AddListener(evt, handler);
        }

        public int AddKeyOnce(string evt, Key k, Action<KeyState> handler)
        {
            bool p = false;
            lock (keyMap)
                p = keyMap.ContainsKey(evt);

            if (p) return AddListener(evt, handler);
            return AddKeyWithHandler(evt, k, handler);
        }

        public void AddKeyOnce(string evt, Key k)
        {
            bool p = false;
            lock (keyMap)
                p = keyMap.ContainsKey(evt);

            if (!p) AddKey(evt, k);
        }

        public int AddKeyOnceWithHandler(string evt, Key k, Lambda up= null, Lambda held = null, Lambda down = null)
        {
            bool p = false;
            lock (keyMap)
                p = keyMap.ContainsKey(evt);

            if (p) return AddListener(evt, up, held, down);
            return AddKeyWithHandler(evt, k, up, held, down);
        }

        public int AddKeyWithHandler(string evt, Key k, Lambda up = null, Lambda held = null, Lambda down = null)
        {

            AddKey(evt, k);
            return AddListener(evt, up, held, down);
        }

        public int AddListener(string evt, Action<KeyState> handler)
        {
            return EventRegistry.instance.AddEventListener(evt, (object keystate) =>
            {
                handler.Invoke((KeyState)keystate);
                return ReturnState.Keep;
            });
        }

        public int AddListener(string evt, Lambda up = null, Lambda held = null, Lambda down = null)
        {
            if (up == null && held == null && down == null) throw new Exception("Null handler");
            return EventRegistry.instance.AddEventListener(evt, (object keystate) =>
            {
                switch ((KeyState)keystate)
                {
                    case KeyState.DOWN:
                        if (down != null) down.Invoke();
                        break;
                    case KeyState.HELD:
                        if (held != null) held.Invoke();
                        break;
                    case KeyState.UP:
                        if (up != null) up.Invoke();
                        break;
                }
                return ReturnState.Keep;
            });
        }

        public void AddAxis(string key, Axis axis)
        {
            AddAxis(key, new[] { axis });
        }

        public void AddAxis(string key, IEnumerable<Axis> axes)
        {
            lock (axisMap)
            {
                if (axisMap.ContainsKey(key))
                {
                    foreach(var axis in axes)
                    {
                        axisMap[key].Add(axis);
                    }
                }
                else
                {
                    axisMap.Add(key, new List<Axis>(axes));
                }
            }
        }

        public void AddAxisOnce(string key, IEnumerable<Axis> axes)
        {
            bool p;
            lock (axisMap)
            {
                p = axisMap.ContainsKey(key);
            }
            if (p) return;
            AddAxis(key, axes);
        }

        public bool GetAxisNormalised(string axis, ref float[] val)
        {
            if (!GetAxis(axis, ref val)) return false;
            int count = 0;
            lock (axisMap)
            {
                //get count
                count = axisMap[axis].Count;
            }

            float norm = 0;

            for(int i=0; i<count; i++)
            {
                norm += val[i] * val[i];
            }

            norm = Mathf.Sqrt(norm);

            for(int i=0; i<count; i++)
            {
                val[i] /= norm;
            }
            return true;
        }

        public bool GetAxis(string axis, ref float[] val)
        {
            bool p;
            lock (blocked)
            {
                p = blocked.Contains(axis);
            }

            if (!p)
            {
                lock (axisMap)
                {
                    if (!axisMap.ContainsKey(axis) || val.Length < axisMap[axis].Count)
                    {
                        p = true;
                    }
                    else
                    {
                        lock (val)
                        {
                            for (int i = 0; i < axisMap[axis].Count; i++)
                            {
                                val[i] = axisMap[axis][i].GetValue();
                            }
                        }
                    }
                }
            }


            return !p;
        }

        public bool GetRawAxis(string axis, ref float[] val)
        {
            bool p;
            lock (blocked)
            {
                p = blocked.Contains(axis);
            }

            if (!p)
            {
                lock (axisMap)
                {
                    if (val.Length < axisMap[axis].Count)
                    {
                        p = true;
                    }
                    else
                    {
                        lock (val)
                        {
                            for (int i = 0; i < axisMap[axis].Count; i++)
                            {
                                val[i] = axisMap[axis][i].GetRawValue();
                            }
                        }
                    }
                }
            }


            return !p;
        }

        public bool GetMousePosition(ref Vector3 pos)
        {

            bool p;

            lock (blocked)
            {
                p = blocked.Contains(MOUSE_POSITION);
            }
            if (!p)
                pos = UnityEngine.Input.mousePosition;
            return !p;
        }

        public bool GetMousePositionRelative(ref Vector3 pos)
        {
            if (!GetMousePosition(ref pos)) return false;


            pos.x -= Screen.width / 2f;
            pos.z = pos.y - Screen.height / 2f;
            pos.y = 0;


            return true;
        }

        public void ResetKeys(string evt)
        {
            lock (keyMap)
            {
                if (keyMap.ContainsKey(evt))
                {
                    keyMap[evt] = new List<Key>();
                }
            }
        }

        public Key Detect()
        {
            if (UnityExecutionThread.instance.InUnityThread()) throw new Exception("Cannot execute in main thread.");

            Key key = null;
            bool pressed = false;

            Array codes = Enum.GetValues(typeof(KeyCode));

            Future<bool> GetKey = new Future<bool>(() =>
            {

                foreach(KeyCode kc in codes)
                {
                    if (UnityEngine.Input.GetKeyDown(kc))
                    {
                        key = new Key(kc);
                        return true;
                    }
                }

                for(int i = Axis.MIN_AXIS; i <= Axis.MAX_AXIS; i++)
                {
                    if (UnityEngine.Input.GetAxisRaw(Axis.PREFIX + i) > Key.sensitivity)
                    {
                        key = new Key(i);
                        return true;
                    }

                    if (UnityEngine.Input.GetAxisRaw(Axis.PREFIX + i) < -Key.sensitivity)
                    {
                        key = new Key(-i);
                        return true;
                    }
                }

                return false;
            });

            while (!pressed) {
                pressed = GetKey.bind();
            }
            return key;
        }
    }
}
