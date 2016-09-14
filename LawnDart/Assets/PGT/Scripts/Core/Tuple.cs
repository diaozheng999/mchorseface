
namespace PGT.Core.Func
{
    /// <summary>
    /// A struct representing a two-tuple
    /// </summary>
    /// <typeparam name="A"></typeparam>
    /// <typeparam name="B"></typeparam>
    public struct Tuple<A, B> 
    {
        public A car { get; private set; }
        public B cdr { get; private set; }
        public Tuple(A _car, B _cdr)
        {
            car = _car;
            cdr = _cdr;
        }
        public override bool Equals(object obj)
        {
            Tuple<A, B> b = (Tuple<A, B>)obj;
            return car.Equals(b.car) && cdr.Equals(b.cdr);
        }
        public override int GetHashCode()
        {
            return car.GetHashCode()*31 + cdr.GetHashCode();
        }
        public override string ToString()
        {
            return "("+car.ToString()+", "+cdr.ToString()+")";
        }
    }

    public struct Tuple<A, B, C>
    {
        public A car { get; private set; }
        public B cdr { get; private set; }
        public C cpr { get; private set; }
        public Tuple(A _car, B _cdr, C _cpr) 
        {
            car = _car;
            cdr = _cdr;
            cpr = _cpr;
        }
        public override bool Equals(object obj)
        {
            Tuple<A, B, C> b = (Tuple<A, B,C>)obj;
            return car.Equals(b.car) && cdr.Equals(b.cdr) && cpr.Equals(b.cpr);
        }
        public override int GetHashCode()
        {
            return (cpr.GetHashCode() * 31 + car.GetHashCode()) * 31 + cdr.GetHashCode();
        }
        public override string ToString()
        {
            return "(" + car.ToString() + ", " + cdr.ToString() + ", " + cpr.ToString() + ")";
        }
    }
    public struct Tuple<A, B, C, D>
    {
        public A car { get; private set; }
        public B cdr { get; private set; }
        public C cpr { get; private set; }
        public D ctr { get; private set; }
        public Tuple(A _car, B _cdr, C _cpr, D _ctr)
        {
            car = _car;
            cdr = _cdr;
            cpr = _cpr;
            ctr = _ctr;
        }
        public override bool Equals(object obj)
        {
            Tuple<A, B, C, D> b = (Tuple<A, B, C, D>)obj;
            return car.Equals(b.car) && cdr.Equals(b.cdr) && cpr.Equals(b.cpr) && ctr.Equals(b.ctr);
            
        }
        public override int GetHashCode()
        {
            return ((ctr.GetHashCode() * 31 + cpr.GetHashCode()) * 31 + car.GetHashCode()) * 31 + cdr.GetHashCode();
        }
        public override string ToString()
        {
            return "(" + car.ToString() + ", " + cdr.ToString() + ", " + cpr.ToString() + ", " + ctr.ToString() + ")";
        }
    }
}
