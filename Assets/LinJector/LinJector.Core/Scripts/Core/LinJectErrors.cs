using System;

namespace LinJector.Core
{
    internal static class LinJectErrors
    {
        public static InvalidProgramException TypedResolverNotMatch()
        {
            return new InvalidProgramException(
                "Typed resolver is trying to casting an object into unmatched type, but this is impossible.");
        }
        
        public static InvalidProgramException TypedResolverCanNotActivate()
        {
            return new InvalidProgramException(
                "Typed resolver is trying to activate new object by type, but this is impossible.");
        }
    }
}