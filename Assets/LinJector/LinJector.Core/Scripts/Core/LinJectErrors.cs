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

        public static InvalidProgramException LoopedDependencyChainDetected()
        {
            return new InvalidProgramException(
                "There are two resolver relying on each other which make resolving impossible");
        }

        public static InvalidOperationException SuperEmptyContainerDidNotAllowThisOperation()
        {
            return new InvalidOperationException(
                "You are trying to access Super-Empty container and trying to do some reaction, but this " +
                "operation is not allowed for Super-Empty!");
        }

        public static InvalidOperationException ContainerBuilderStateInvalid()
        {
            return new InvalidOperationException("Trying to start/end the building of container, but the" +
                                                 "state of container builder seems to be not sync with your call.");
        }
    }
}