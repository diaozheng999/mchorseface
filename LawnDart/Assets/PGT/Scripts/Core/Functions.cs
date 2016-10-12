namespace PGT.Core.Func
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using UnityEngine;
    
    public delegate void Lambda();
    
    public static class Function
    {
        public static void noop() { }
        public static T id<T>(T x) { return x; }
    }

    public class Future
    {
        Lambda _f;
        Action<Lambda> _app;
        public Future(Lambda f, Action<Lambda> app)
        {
            _f = f;
            _app = app;
        }
        public Future(Lambda f)
        {
            _f = f;
            _app = null;
        }

        public void bind()
        {
            if (_app != null)
                _app.Invoke(_f);
            else
                UnityExecutionThread.instance.ExecuteInMainThread(_f);
        }

        public void bind(Action<Lambda> app)
        {
            app.Invoke(_f);
        }

        public void bind(ExecutionOrder order)
        {
            UnityExecutionThread.instance.ExecuteInMainThread(_f, order);
        }
        public void bind(string order)
        {
            switch (order)
            {
                case "update":
                    bind(ExecutionOrder.Update);
                    return;
                case "fixedUpdate":
                    bind(ExecutionOrder.FixedUpdate);
                    return;
                case "coroutine":
                    bind(ExecutionOrder.Coroutine);
                    return;
                case "lateUpdate":
                    bind(ExecutionOrder.LateUpdate);
                    return;
                default:
                    bind(ExecutionOrder.Any);
                    return;
            }
        }

        internal void Invoke()
        {
            _f.Invoke();
        }

        public Future join(Future other)
        {
            return new Future(() =>
            {
                _f.Invoke();
                other.Invoke();
            });
        }

        public Future<T> join<T>(Future<T> other)
        {
            return new Future<T>(() =>
            {
                _f.Invoke();
                return other.Invoke();
            });
        }

        public override string ToString()
        {
            return "<Future (void) with function " + _f.ToString() + ">";
        }

        static void _id() { }

        public static Future id
        {
            get
            {
                return new Future(_id);
            }
        }

        


    }

    public class Future<T>
    {
        protected Func<T> _f;
        protected Func<Func<T>, T> _app;

        public Future(Func<T> f, Func<Func<T>, T> app)
        {
            _f = f;
            _app = app;
        }

        public Future(Func<T> f)
        {
            _f = f;
            _app = null;
        }

        public T bind()
        {
            if (_app != null)
                return _app.Invoke(_f);
            
            else
                return UnityExecutionThread.instance.ExecuteInMainThread<T>(_f);
        }

        public Future<U> applyTo<U>(Continuation<T,U> cont)
        {
            return cont.apply(this);
        }

        public Future<U> applyTo<U>(Func<T,U> cont)
        {
            return (new Continuation<T,U>(cont)).apply(this);
        }

        public Future applyTo(Continuation<T> cont)
        {
            return cont.apply(this);
        }

        public Future applyTo(Action<T> cont)
        {
            return (new Continuation<T>(cont)).apply(this);
        }

        public Future<T> join(Future f)
        {
            return new Future<T>(() =>
            {
                T result = _f.Invoke();
                f.Invoke();
                return result;
            });
        }

        public Future<Tuple<T, U>> join<U>(Future<U> other)
        {
            return new Future<Tuple<T, U>>(
                () => new Tuple<T, U>(_f.Invoke(), other.Invoke())
                );
        }

        public T bind(ExecutionOrder order)
        {
            return UnityExecutionThread.instance.ExecuteInMainThread<T>(_f, order);
        }

        public T bind(string order)
        {
            switch (order)
            {
                case "update":
                    return bind(ExecutionOrder.Update);
                case "fixedUpdate":
                    return bind(ExecutionOrder.FixedUpdate);
                case "coroutine":
                    return bind(ExecutionOrder.Coroutine);
                case "lateUpdate":
                    return bind(ExecutionOrder.LateUpdate);
                default:
                    return bind(ExecutionOrder.Any);
            }
        }

        public T bind(Func<Func<T>, T> app)
        {
            return app.Invoke(_f);
        }

        internal T Invoke()
        {
            return _f.Invoke();
        }

        public override string ToString()
        {
            return "<Future (" + typeof(T).Name + ") with function " + _f.ToString() + ">";
        }

        public static implicit operator Future<T>(Func<T> f)
        {
            return new Future<T>(f);
        }

        static T _id() { return default(T); }

        public static Future<T> id
        {
            get
            {
                return new Future<T>(_id);
            }
        }

    }

    public class Continuation<S, T>
    {
        Func<S, T> _f;

        public Continuation(Func<S,T> cont)
        {
            _f = cont;
        }

        public Future<T> apply(S param)
        {
            return new Future<T>(() => _f.Invoke(param));
        }

        public Future<T> apply(Future<S> param)
        {
            return new Future<T>(() =>
            {
                return _f.Invoke(param.Invoke());
            });
        }
    }

    public class Continuation<S>
    {
        Action<S> _f;

        public Continuation(Action<S> cont)
        {
            _f = cont;
        }
        public Future apply(S param)
        {
            return new Future(() => _f.Invoke(param));
        }

        public Future apply(Future<S> param)
        {
            return new Future(() =>
            {
                _f.Invoke(param.Invoke());
            });
        }
    }
}