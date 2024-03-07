using Akiyama.IPC.Shared.Network.Packets;
using System;

namespace Akiyama.IPC.Shared.Events
{
    public class OnAllSplitPacketsReceivedEventArgs : EventArgs
    {

        public Packet Packet { get; private set; }

        public OnAllSplitPacketsReceivedEventArgs(Packet fullPacket)
        {
            this.Packet = fullPacket;
        }

    }
}
