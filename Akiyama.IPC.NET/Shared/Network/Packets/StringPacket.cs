using System.Text;

namespace Akiyama.IPC.Shared.Network.Packets
{
    /// <summary>
    /// A basic packet used for transferring a single <see cref="string"/> via IPC.
    /// <br /><br />This packet implements both <see cref="Packet.Prepare"/> and <see cref="Packet.Populate"/> to automatically process its Properties into/from its payload when send or received respectively.
    /// </summary>
    public class StringPacket : Packet
    {

        /// <summary>
        /// The <see cref="string"/> that is to be, or was transferred.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        public override int ID { get; } = (int)PacketType.STRING;

        public override void Populate()
        {
            // Here we can use this packet's data to fill in the packet's properties (in this case Text), you can do any amount of assignments here,
            // so long as you read the data correctly!
            this.Text = PacketConstructor.BytesToString(this.Payload, encoding: Encoding.UTF8);
        }

        public override void Prepare()
        {
            this.SetPayload(PacketConstructor.StringToBytes(this.Text, encoding: Encoding.UTF8));
        }

    }
}
