namespace Akiyama.IPC.Shared.Network.Packets
{
    public class UShortPacket : Packet
    {
        public override int ID => (int)PacketType.USHORT;

        public ushort Value { get; set; }

        public override void Populate()
        {
            this.Value = PacketConstructor.BytesToUInt16(this.Payload);
        }

        public override void Prepare()
        {
            this.SetPayload(PacketConstructor.UInt16ToBytes(this.Value));
        }
    }
}
