namespace Akiyama.IPC.Shared.Network.Packets
{
    public class GenericDataPacket : Packet
    {
        public override int ID => (int)PacketType.GENERIC_DATA;

        // *winds noises, tumble weed passes...*
        // (there's nothing to put here, we're just a packet for handling generic data (bytes) transferral)

    }
}
