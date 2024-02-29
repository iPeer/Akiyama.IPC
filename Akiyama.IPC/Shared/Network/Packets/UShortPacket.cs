namespace Akiyama.IPC.Shared.Network.Packets
{
    public class UShortPacket : Packet
    {
        public override int ID => (int)PacketType.USHORT;

        public short Value { get; private set; }

        public override void Populate()
        {
            this.Value = PacketConstructor.BytesToInt16(this.Data);
        }
    }
}
