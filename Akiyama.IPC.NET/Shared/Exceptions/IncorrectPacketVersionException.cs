using Akiyama.IPC.Shared.Network;
using System;

namespace Akiyama.IPC.Shared.Exceptions
{
    public class IncorrectPacketVersionException : Exception
    {

        public IncorrectPacketVersionException(Version v) : base($"Packet with incorrect version received. Expected {PacketConstructor.MINIMUM_PACKET_VERSION}, got {v}") { }

    }
}
