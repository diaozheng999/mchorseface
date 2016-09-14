using System;
using UnityEngine;
namespace PGT.Core.Input
{
    public class Axis
    {
        public const int MAX_AXIS = 28;
        public const int MIN_AXIS = 1;
        public const string PREFIX = "_joy_axis";

        int axis;
        string axis_str;

        Key positive;
        Key negative;

        float deadzone_min;
        float deadzone_max;

        bool negate;

        public Axis(int axis, float deadzone_min, float deadzone_max)
        {
            negate = false;
            if (axis < 0)
            {
                negate = true;
                axis = -axis;
            }
            if (axis < MIN_AXIS || axis > MAX_AXIS) throw new Exception("Invalid axis.");
            this.axis = axis;
            this.deadzone_min = deadzone_min;
            this.deadzone_max = deadzone_max;
            axis_str = PREFIX + axis.ToString();
        }

        public Axis(int axis) : this(axis, -0.01f, 0.01f) { }

        public Axis(int axis, float deadzone_max) : this(axis, -0.01f, deadzone_max) { }

        public Axis(string str, float deadzone_min, float deadzone_max)
        {
            this.axis_str = str;

            if (str.StartsWith("virtual~"))
            {
                string[] p = str.Split('~');

                if (p[1] != "")
                    positive = Key.FromString(p[1]);

                if (p[2] != "")
                    negative = Key.FromString(p[2]);


            }

            this.deadzone_min = deadzone_min;
            this.deadzone_max = deadzone_max;
        }

        public Axis(Key positive = null, Key negative = null)
        {
            axis_str = "virtual~";
            this.positive = positive;
            this.negative = negative;

            if (positive != null)
                axis_str += positive.ToString();

            axis_str += "~";

            if (negative != null)
                axis_str += negative.ToString();
        }

        public float GetRawValue()
        {
            if (axis_str.StartsWith("virtual~")) return GetVirtualValue();
            float v = UnityEngine.Input.GetAxisRaw(axis_str);
            if (negate) return -v;
            return v;
        }

        public float GetValue()
        {
            if (axis_str.StartsWith("virtual~")) return GetVirtualValue();
            float v = UnityEngine.Input.GetAxis(axis_str);
            if (v < deadzone_max && v > deadzone_min) return 0;
            if(negate) return -v;
            return v;
        }

        float GetVirtualValue()
        {
            float v = 0;
            if (positive != null && positive.IsHeld()) v += 1f;
            if (negative != null && negative.IsHeld()) v -= 1f;
            return v;
        }   

        public static Axis FromString(string str)
        {
            var p = str.Split('/');
            var dz_min = float.Parse(p[1]);
            var dz_max = float.Parse(p[2]);
            return new Axis(p[0], dz_min, dz_max); 
        }

        public static Axis MouseX()
        {
            return new Axis("Mouse X", 0, 0);
        }

        public static Axis MouseY()
        {
            return new Axis("Mouse Y", 0, 0);
        }

        public int GetAxis()
        {
            if (negate) return -axis;
            return axis;
        }

        public override string ToString()
        {
            return axis_str+"/"+deadzone_min+"/"+deadzone_max;
        }
    }

    public class Key
    {
        KeyCode? keyCode;
        Axis axis;

        public static float sensitivity = 0.5f;

        bool prevDown;

        public Key(KeyCode code)
        {
            keyCode = code;
        }

        public Key(int axis)
        {
            if (axis < 0)
            {
                this.axis = new Axis(axis, -sensitivity, 0);
            }
            else
            {
                this.axis = new Axis(axis, -1, sensitivity);
            }
            prevDown = false;
        }

        public bool IsDown()
        {
            if (keyCode != null) return UnityEngine.Input.GetKeyDown(keyCode.Value);
            bool prev = prevDown;
            return ((!prev) && IsHeld());
        }

        public bool IsHeld()
        {
            if (keyCode != null) return UnityEngine.Input.GetKey(keyCode.Value);
            bool _prevDown = axis.GetValue() > sensitivity;
            if (_prevDown) prevDown = true;
            return _prevDown;
        }

        public bool IsUp()
        {
            if (keyCode != null) return UnityEngine.Input.GetKeyUp(keyCode.Value);
            bool prev = prevDown;
            if (prev && (!IsHeld()))
            {
                prevDown = false;
                return true;
            }
            return false;
        }

        public static Key FromString(string str)
        {
            var p = str.Split(':');

            switch (p[0])
            {
                case "key":
                    return new Key((KeyCode)int.Parse(p[1]));
                case "axis":
                    return new Key(int.Parse(p[1]));
                default:
                    return null;
            }
        }

        public override string ToString()
        {
            if (keyCode != null) return "key:"+(int)keyCode;
            return "axis:" + axis.GetAxis();
        }
    }
}