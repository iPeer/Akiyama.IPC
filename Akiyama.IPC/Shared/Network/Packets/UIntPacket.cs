namespace Akiyama.IPC.Shared.Network.Packets
{
    public class UIntPacket : Packet
    {
        public override int ID => (int)PacketType.UINT;

        public uint Value { get; private set; }

        public override void Populate()
        {
            this.Value = PacketConstructor.BytesToUInt32(this.Data);
        }
    }
}
