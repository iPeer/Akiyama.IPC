using System.Text;

namespace Akiyama.IPC.Shared.Network.Packets
{
    public class StringPacket : Packet
    {

        public string Text { get; set; }

        public override int ID { get; } = (int)PacketType.STRING;

        public override void Populate()
        {
            // Here we can use this packet's data to fill in the packet's properties (in this case Text), you can do any amount of assignments here,
            // so long as you read the data correctly!
            this.Text = PacketConstructor.BytesToString(this.Data, encoding: Encoding.UTF8);
        }

        public override void Prepare()
        {
            this.SetData(PacketConstructor.StringToBytes(this.Text, encoding: Encoding.UTF8));
        }

    }
}
