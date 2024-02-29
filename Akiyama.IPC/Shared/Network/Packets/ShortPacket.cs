namespace Akiyama.IPC.Shared.Network.Packets
{
    public class ShortPacket : Packet
    {
        public override int ID => (int)PacketType.SHORT;

        public short Value { get; private set; }

        public override void Populate()
        {
            this.Value = PacketConstructor.BytesToShort(this.Data);
        }
    }
}
