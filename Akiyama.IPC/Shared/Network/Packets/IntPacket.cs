namespace Akiyama.IPC.Shared.Network.Packets
{
    public class IntPacket : Packet
    {


        public int Value = 0;

        public override int ID { get; } = (int)PacketType.INT;

        public override void Populate()
        {
            this.Value = PacketConstructor.BytesToInt32(this.Data);
        }


    }
}
