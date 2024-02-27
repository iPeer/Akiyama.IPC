using System.Text;

namespace Akiyama.IPC.Shared.Network.Packets
{
    public class TestPacket : Packet
    {

        public override int ID { get; } = (int)PacketType.TEST_PACKET;

        public string Value = string.Empty;

        public TestPacket() : base(Encoding.UTF8.GetBytes("This is a test packet")) { }

        public override void Populate()
        {
            this.Value = PacketConstructor.BytesToString(this.Data);
        }

    }
}
