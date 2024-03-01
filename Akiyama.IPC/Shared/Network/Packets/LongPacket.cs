namespace Akiyama.IPC.Shared.Network.Packets
{
    public class LongPacket : Packet
    {
        public override int ID => (int)PacketType.LONG;

        public long Value { get; set; }

        public override void Populate()
        {
            this.Value = PacketConstructor.BytesToInt64(this.Payload);
        }

        public override void Prepare()
        {
            this.SetPayload(PacketConstructor.Int64ToBytes(this.Value));
        }
    }
}
