using System;

namespace InternalDSL.Builder
{
    /// <summary>
    /// A set of static methods for wrapping primitive types into reference
    /// types.
    /// </summary>
    public static class PrimitiveWrappers
    {
        public static Byte By(this byte self)
        {
            return self;
        }

        public static Integer I(this int self)
        {
            return self;
        }

        public static Long L(this long self)
        {
            return self;
        }

        public static Float F(this float self)
        {
            return self;
        }

        public static Double D(this double self)
        {
            return self;
        }

        public static Character C(this char self)
        {
            return self;
        }

        public static Boolean B(this bool self)
        {
            return self;
        }
    }

    /// <summary>
    /// Superclass for wrapper types. This class provides the capability to
    /// perform equality checks and comparisons, as well as printing the wrapped
    /// value.
    ///
    /// Subtypes are expected to provide implicit conversion operators.
    /// </summary>
    /// <typeparam name="T">The type of value to wrap</typeparam>
    public abstract class Wrapper<T> : IComparable
    {
        public T Value { get; }

        protected Wrapper(T value) =>
            Value = value;

        public static bool operator ==(Wrapper<T> a, Wrapper<T> b)
        {
            if (a is null) return b is null;
            return a.Equals(b);
        }

        public static bool operator !=(Wrapper<T> a, Wrapper<T> b) =>
            !(a == b);

        public override bool Equals(object other) =>
            other is Wrapper<T> wrapper && ValueEquals(wrapper.Value);

        protected abstract bool ValueEquals(T other);

        public override int GetHashCode() =>
            Value.GetHashCode();

        public abstract int CompareTo(object other);

        public override string ToString() =>
            $"{Value}";
    }

    public class Byte : Wrapper<byte>
    {
        public Byte(byte b) : base(b)
        {
        }

        public static implicit operator Byte(byte b) => new Byte(b);

        protected override bool ValueEquals(byte other)
        {
            return Value == other;
        }

        public override int CompareTo(object other)
        {
            var by = other as Byte;
            return by != null ? Value.CompareTo(by.Value) : Value.CompareTo((byte)other);
        }
    }

    public class Integer : Wrapper<int>
    {
        public Integer(int i) : base(i)
        {
        }

        public static implicit operator Integer(int i) => new Integer(i);

        protected override bool ValueEquals(int other)
        {
            return Value == other;
        }

        public override int CompareTo(object other)
        {
            var integer = other as Integer;
            return integer != null ? Value.CompareTo(integer.Value) : Value.CompareTo((int) other);
        }
    }

    public class Long : Wrapper<long>
    {
        public Long(long l) : base(l)
        {
        }

        public static implicit operator Long(long l) => new Long(l);

        protected override bool ValueEquals(long other)
        {
            return Value == other;
        }

        public override int CompareTo(object other)
        {
            var lo = other as Long;
            return lo != null ? Value.CompareTo(lo.Value) : Value.CompareTo((long)other);
        }
    }

    public class Float : Wrapper<float>
    {
        public Float(float f) : base(f)
        {
        }

        public static implicit operator Float(float f) => new Float(f);

        protected override bool ValueEquals(float other)
        {
            return Value == other;
        }

        public override int CompareTo(object other)
        {
            var fl = other as Float;
            return fl != null ? Value.CompareTo(fl.Value) : Value.CompareTo((float)other);
        }
    }

    public class Double : Wrapper<double>
    {
        public Double(double d) : base(d)
        {
        }

        public static implicit operator Double(double d) => new Double(d);

        protected override bool ValueEquals(double other)
        {
            return Value == other;
        }

        public override int CompareTo(object other)
        {
            var doub = other as Double;
            return doub != null ? Value.CompareTo(doub.Value) : Value.CompareTo((double)other);
        }
    }

    public class Character : Wrapper<char>
    {
        public Character(char c) : base(c)
        {
        }

        public static implicit operator Character(char c) => new Character(c);

        protected override bool ValueEquals(char other)
        {
            return Value == other;
        }

        public override int CompareTo(object other)
        {
            var character = other as Character;
            return character != null ? Value.CompareTo(character.Value) : Value.CompareTo((char)other);
        }
    }

    public class Boolean : Wrapper<bool>
    {
        public Boolean(bool b) : base(b)
        {
        }

        public static implicit operator Boolean(bool b) => new Boolean(b);

        protected override bool ValueEquals(bool other)
        {
            return Value == other;
        }

        public override int CompareTo(object other)
        {
            var boolean = other as Boolean;
            return boolean != null ? Value.CompareTo(boolean.Value) : Value.CompareTo((bool)other);
        }
    }
}
