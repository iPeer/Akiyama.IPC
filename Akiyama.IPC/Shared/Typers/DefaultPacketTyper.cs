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

        // There are multiple ways you can do this, but this is the way I prefer to do it.
        // For example, you could use literal int comparisons, or defined int consts, etc.
        public override Packet GetPacketObjectFromId(int id)
        {
            switch (id)
            {
                // You can use an enum (like I do here), or just literally cross referenced the int values - whichever is easier for you.
                case (int)PacketType.TEST_PACKET: return new TestPacket();
                case (int)PacketType.STRING: return new StringPacket();
                case (int)PacketType.INT: return new IntPacket();
                // 'default' should be your "this packet doesn't exist" result.
                //
                // You should do one of two things here:
                //      1. Raise this expetion yourself like I have here, or
                //      2. return null
                // If you return null, the PacketConstructor class will raise the same error instead.
                default: throw new UnknownPacketException(id);
            }
        }


    }
}
