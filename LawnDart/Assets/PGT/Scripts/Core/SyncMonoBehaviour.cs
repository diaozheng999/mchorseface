namespace PGT.Core
{
    using UnityEngine;
    using Func;

    public class SyncMonoBehaviour : MonoBehaviour
    {
        public new Transform transform
        {
            get
            {
                return new Future<Transform>(() => base.transform).bind();
            }
        }

        public Future<Vector3> GetPosition()
        {
            return new Future<Vector3>(() => base.transform.position);
        }

        public Future SetPosition(Vector3 position)
        {
            return new Future(() => base.transform.position = position);
        }

        public Future SetPosition(Future<Vector3> position)
        {
            return position.applyTo((Vector3 pos) => 
                {
                    base.transform.position = pos;
                });
        }

        public Future<Quaternion> GetRotation()
        {
            return new Future<Quaternion>(() => base.transform.rotation);
        }

        public Future SetRotation(Quaternion rotation)
        {
            return new Future(() => base.transform.rotation = rotation);
        }

        public Future SetRotation(Future<Quaternion> rotation)
        {
            return rotation.applyTo((Quaternion q) =>
            {
                base.transform.rotation = q;
            });
        }

        void Awake()
        {

        }

        public override string ToString()
        {
            return new Future<string>(base.ToString).bind();
        }
    }
}
