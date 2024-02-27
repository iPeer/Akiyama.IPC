using System.Text;

namespace Akiyama.IPC.Shared.Network.Packets
{
    public class TestPacket : Packet
    {

        public string Value = string.Empty;

        public TestPacket() : this((int)PacketType.TEST_PACKET) { }
        public TestPacket(int type) : base(type) { this.SetData(Encoding.UTF8.GetBytes("This is a test packet")); }

        public override void Populate()
        {
            this.Value = PacketConstructor.BytesToString(this.Data);
        }

    }
}
