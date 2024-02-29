namespace Akiyama.IPC.Shared.Network.Packets
{
    public class StringPacket : Packet
    {

        public string Text = string.Empty;

        public override int ID { get; } = (int)PacketType.STRING;

        public override void Populate()
        {
            // Here we can use this pakcet's data to fill in the packet's properties (in this case StringValue), you can do any amount of assignments here,
            // so long as you read the data correctly!
            this.Text = PacketConstructor.BytesToString(this.Data);
        }

    }
}
