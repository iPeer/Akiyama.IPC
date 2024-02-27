namespace Akiyama.IPC.Shared.Network.Packets
{
    public class IntPacket : Packet
    {


        public int NumberValue = 0;

        public override int ID { get; } = (int)PacketType.INT;

        public override void Populate()
        {
            this.NumberValue = PacketConstructor.BytesToInt32(this.Data);
        }


    }
}
