﻿namespace Akiyama.IPC.Shared.Network.Packets
{
    public class ULongPacket : Packet
    {
        public override int ID => (int)PacketType.ULONG;

        public ulong Value { get; private set; }

        public override void Populate()
        {
            this.Value = PacketConstructor.BytesToUInt64(this.Data);
        }

    }
}
