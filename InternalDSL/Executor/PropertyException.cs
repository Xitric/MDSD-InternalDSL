using System;

namespace InternalDSL.Executor
{
    /// <summary>
    /// Exception thrown during the evaluation of boolean expressions within a
    /// property in case a violation is found.
    /// </summary>
    internal class PropertyException : Exception
    {
        public PropertyException(string msg) : base(msg)
        {
        }
    }
}
