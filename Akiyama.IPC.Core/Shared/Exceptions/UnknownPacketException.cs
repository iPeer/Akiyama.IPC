using System;

namespace Akiyama.IPC.Shared.Exceptions
{
    public class UnknownPacketException : Exception
    {

        public UnknownPacketException(int id) : base($"Unknown packet id '{id}'") { }

    }
}
