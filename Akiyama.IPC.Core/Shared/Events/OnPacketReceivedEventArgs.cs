using Akiyama.IPC.Shared.Network.Packets;
using System;

namespace Akiyama.IPC.Shared.Events
{
    public class OnPacketReceivedEventArgs : EventArgs
    {
        public readonly Packet Packet;
        public OnPacketReceivedEventArgs(Packet packet) { this.Packet = packet; }

    }
}
