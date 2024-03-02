namespace Akiyama.IPC.Shared.Network.Packets
{
    /// <summary>
    /// A basic packet used for transferring a single <see langword="short"/> via IPC.
    /// <br /><br />This packet implements both <see cref="Packet.Prepare"/> and <see cref="Packet.Populate"/> to automatically process its Properties into/from its payload when send or received respectively.
    /// <br /><br /><b>WARNING</b>: This packet does not sanity check its payload to ensure its length fits the conversion being made.
    /// </summary>
    public class ShortPacket : Packet
    {
        public override int ID => (int)PacketType.SHORT;

        /// <summary>
        /// The <see cref="short"/> that is to be, or was transferred.
        /// </summary>
        public short Value { get; set; }

        public override void Populate()
        {
            this.Value = PacketConstructor.BytesToShort(this.Payload);
        }

        public override void Prepare()
        {
            this.SetPayload(PacketConstructor.Int16ToBytes(this.Value));
        }
    }
}
