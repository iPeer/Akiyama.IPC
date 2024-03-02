namespace Akiyama.IPC.Shared.Network.Packets
{
    /// <summary>
    /// A basic packet used for transferring generic data (raw bytes) via IPC.
    /// <br /><br /><b>Note</b>: This packet is not required for transferring generic data, any packet that does no processing on its payload is inherently "generic".
    /// </summary>
    public class GenericDataPacket : Packet
    {
        public override int ID => (int)PacketType.GENERIC_DATA;

        // *winds noises, tumble weed passes...*
        // (there's nothing to put here, we're just a packet for handling generic data (bytes) transferral)

    }
}
