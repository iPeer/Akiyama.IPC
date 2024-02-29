namespace Akiyama.IPC.Shared.Network.Packets
{
    public class ShortPacket : Packet
    {
        public override int ID => (int)PacketType.SHORT;

        public short Value { get; set; }

        public override void Populate()
        {
            this.Value = PacketConstructor.BytesToShort(this.Data);
        }

        public override void Prepare()
        {
            this.SetData(PacketConstructor.Int16ToBytes(this.Value));
        }
    }
}
