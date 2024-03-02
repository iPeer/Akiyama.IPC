namespace Akiyama.IPC.Shared.Network.Packets
{
    /// <summary>
    /// A basic packet used for transferring a single <see langword="int"/> via IPC.
    /// <br /><br />This packet implements both <see cref="Packet.Prepare"/> and <see cref="Packet.Populate"/> to automatically process its Properties into/from its payload when send or received respectively.
    /// <br /><br /><b>WARNING</b>: This packet does not sanity check its payload to ensure its length fits the conversion being made.
    /// </summary>
    public class IntPacket : Packet
    {

        /// <summary>
        /// The <see langword="int"/> that is to be, or was transferred.
        /// </summary>
        public int Value { get; set; }

        public override int ID { get; } = (int)PacketType.INT;

        public override void Populate()
        {
            this.Value = PacketConstructor.BytesToInt32(this.Payload);
        }

        public override void Prepare()
        {
            this.SetPayload(PacketConstructor.Int32ToBytes(this.Value));
        }

    }
}
