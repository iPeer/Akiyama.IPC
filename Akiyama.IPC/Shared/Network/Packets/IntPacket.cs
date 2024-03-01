namespace Akiyama.IPC.Shared.Network.Packets
{
    public class IntPacket : Packet
    {


        public int Value { get; set; }

        public override int ID { get; } = (int)PacketType.INT;

        public override void Populate()
        {
            this.Value = PacketConstructor.BytesToInt32(this.Payload);
        }

        public override void Prepare()
        {
            this.SetPayload(PacketConstructor.Int32ToBytes(this.Value));
        }

    }
}
