namespace Akiyama.IPC.Shared.Network.Packets
{
    public class IntPacket : Packet
    {


        public int Value { get; set; }

        public override int ID { get; } = (int)PacketType.INT;

        public override void Populate()
        {
            this.Value = PacketConstructor.BytesToInt32(this.Data);
        }

        public override void Prepare()
        {
            this.SetData(PacketConstructor.Int32ToBytes(this.Value));
        }

    }
}
