using Akiyama.IPC.Shared.Exceptions;
using Akiyama.IPC.Shared.Network;
using Akiyama.IPC.Shared.Network.Packets;

namespace Akiyama.IPC.Shared.Typers
{
    /// <summary>
    /// The default Packet Typer. This class is used when the user does not specify their own when creating the IPC client/server. Both the server and client must use the same typer in order for them to work correctly.
    /// </summary>
    public class DefaultPacketTyper : PacketTyper
    {

        /// <summary>
        /// Converts the incoming <see cref="Packet"/> ID into an instance of its respective class.
        /// <br />Throws <see cref="UnknownPacketException"/> if no <see cref="Packet"/> with the ID <paramref name="id"/> is found.
        /// </summary>
        /// <param name="id">The id to return an instance of</param>
        /// <returns>A instance of a <see cref="Packet"/>'s respective class, determined by <paramref name="id"/>.</returns>
        /// <exception cref="UnknownPacketException"></exception>
        public override Packet GetPacketObjectFromId(int id)
        {

            // There are multiple ways you can do this, but this is the way I prefer to do it.
            // For example, you could use literal int comparisons, or defined int consts, etc.

            switch (id)
            {

                // You can use an enum (like here), compare the ints directly; there are several ways - whichever is easier for you.

                // TestPacket here is an example of just literally defining the int values instead of using an enum. You can use consts, fields or Properties too, if you like.
                // So long as the comparison matches the correct ID of the correct packet, it doesn't matter.
                case 255: return new TestPacket();
                case (int)PacketType.STRING: return new StringPacket();
                case (int)PacketType.INT: return new IntPacket();
                case (int)PacketType.SHORT: return new ShortPacket();
                case (int)PacketType.LONG: return new LongPacket();
                case (int)PacketType.UINT: return new UIntPacket();
                case (int)PacketType.USHORT: return new UShortPacket();
                case (int)PacketType.ULONG: return new ULongPacket();
                case (int)PacketType.GENERIC_DATA: return new GenericDataPacket();
                case (int)PacketType.GZIP: return new GZipPacket();

                // 'default' should be your "this packet doesn't exist" result.
                //
                // You should do one of two things here:
                //      1. Raise this exception yourself like here, or
                //      2. return null
                // If you return null, the PacketConstructor class will raise the same error instead.
                default: throw new UnknownPacketException(id);
            }
        }


    }
}
