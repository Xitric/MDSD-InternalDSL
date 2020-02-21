namespace InternalDSL.SemanticModel.Generator
{
    /// <summary>
    /// A generator for creating two-tuples of random values.
    /// </summary>
    /// <typeparam name="T1">The type of the first value in the tuple</typeparam>
    /// <typeparam name="T2">The type of the second value in the tuple</typeparam>
    public class PairGenerator<T1, T2> : Generator<(T1, T2)>
    {
        private readonly Generator<T1> _generator1;
        private readonly Generator<T2> _generator2;

        public PairGenerator(Generator<T1> generator1, Generator<T2> generator2)
        {
            _generator1 = generator1;
            _generator2 = generator2;
        }

        public override (T1, T2) Next()
        {
            return (_generator1.Next(), _generator2.Next());
        }
    }

    /// <summary>
    /// A generator for creating three-tuples of random values.
    /// </summary>
    /// <typeparam name="T1">The type of the first value in the tuple</typeparam>
    /// <typeparam name="T2">The type of the second value in the tuple</typeparam>
    /// <typeparam name="T3">The type of the third value in the tuple</typeparam>
    public class TripletGenerator<T1, T2, T3> : Generator<(T1, T2, T3)>
    {
        private readonly Generator<T1> _generator1;
        private readonly Generator<T2> _generator2;
        private readonly Generator<T3> _generator3;

        public TripletGenerator(Generator<T1> generator1, Generator<T2> generator2, Generator<T3> generator3)
        {
            _generator1 = generator1;
            _generator2 = generator2;
            _generator3 = generator3;
        }

        public override (T1, T2, T3) Next()
        {
            return (_generator1.Next(), _generator2.Next(), _generator3.Next());
        }
    }
}
