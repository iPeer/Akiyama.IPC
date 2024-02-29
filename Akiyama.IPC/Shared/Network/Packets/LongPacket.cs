namespace Akiyama.IPC.Shared.Network.Packets
{
    public class LongPacket : Packet
    {
        public override int ID => (int)PacketType.LONG;

        public long Value { get; private set; }

        public override void Populate()
        {
            this.Value = PacketConstructor.BytesToLong(this.Data);
        }
    }
}
