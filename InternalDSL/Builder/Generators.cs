using InternalDSL.SemanticModel.Generator;

namespace InternalDSL.Builder
{
    public class Generators
    {
        public static readonly IntGenerator Integer = new IntGenerator();
        public static readonly PosIntGenerator PosInteger = new PosIntGenerator();
        public static readonly SmallIntGenerator SmallInteger = new SmallIntGenerator();
        public static readonly PosSmallIntGenerator PosSmallInteger = new PosSmallIntGenerator();
        public static readonly CharGenerator Character = new CharGenerator();
        public static readonly DoubleGenerator Double = new DoubleGenerator();
        public static readonly FloatGenerator Float = new FloatGenerator();
        public static readonly StringGenerator Str = new StringGenerator();
        public static ArrayGenerator<T> Array<T>(Generator<T> generator) => new ArrayGenerator<T>(generator);
        public static ListGenerator<T> List<T>(Generator<T> generator) => new ListGenerator<T>(generator);
        public static PairGenerator<T1, T2> Pair<T1, T2>(Generator<T1> generator1, Generator<T2> generator2) => new PairGenerator<T1, T2>(generator1, generator2);
        public static TripletGenerator<T1, T2, T3> Triplet<T1, T2, T3>(Generator<T1> generator1, Generator<T2> generator2, Generator<T3> generator3) => new TripletGenerator<T1, T2, T3>(generator1, generator2, generator3);
    }
}
