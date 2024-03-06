using System;

namespace Akiyama.IPC.Shared.Exceptions
{
    public class TooManySplitsException : Exception
    {

        public TooManySplitsException(int length, int numSplits) : base($"Splitting packing with length of {length}, results in too many splits: {numSplits}. Maximum splits is {byte.MaxValue}.") { }

    }
}
