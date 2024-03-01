namespace Akiyama.IPC.Shared.Network.Packets
{
    public class ShortPacket : Packet
    {
        public override int ID => (int)PacketType.SHORT;

        public short Value { get; set; }

        public override void Populate()
        {
            this.Value = PacketConstructor.BytesToShort(this.Payload);
        }

        public override void Prepare()
        {
            this.SetPayload(PacketConstructor.Int16ToBytes(this.Value));
        }
    }
}
