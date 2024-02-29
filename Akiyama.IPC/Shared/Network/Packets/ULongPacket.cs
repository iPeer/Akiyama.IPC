namespace Akiyama.IPC.Shared.Network.Packets
{
    public class ULongPacket : Packet
    {
        public override int ID => (int)PacketType.ULONG;

        public ulong Value { get; set; }

        public override void Populate()
        {
            this.Value = PacketConstructor.BytesToUInt64(this.Data);
        }

        public override void Prepare()
        {
            this.SetData(PacketConstructor.UInt64ToBytes(this.Value));
        }

    }
}
