namespace Akiyama.IPC.Shared.Network.Packets
{
    public class UIntPacket : Packet
    {
        public override int ID => (int)PacketType.UINT;

        public uint Value { get; set; }

        public override void Populate()
        {
            this.Value = PacketConstructor.BytesToUInt32(this.Payload);
        }

        public override void Prepare()
        {
            this.SetPayload(PacketConstructor.UInt32ToBytes(this.Value));
        }
    }
}
