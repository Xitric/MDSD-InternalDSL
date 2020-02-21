using System;

namespace InternalDSL.Executor
{
    internal class PropertyException : Exception
    {
        public PropertyException(string msg) : base(msg)
        {
        }
    }
}
