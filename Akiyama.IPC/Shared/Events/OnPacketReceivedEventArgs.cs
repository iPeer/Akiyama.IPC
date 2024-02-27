using Akiyama.IPC.Shared.Network;
using System;

namespace Akiyama.IPC.Shared.Events
{
    public class OnPacketReceivedEventArgs : EventArgs
    {
        public readonly IPacket Packet;
        public OnPacketReceivedEventArgs(IPacket packet) { this.Packet = packet; }

    }
}
