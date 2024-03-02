using Akiyama.IPC.Shared.Network.Packets;

namespace Akiyama.IPC.Shared.Typers
{
    /// <summary>
    /// The base class for all Packet Typers. This class is <see langword="abstract"/>.
    /// </summary>
    public abstract class PacketTyper
    {
        /// <summary>
        /// When overridden, returns a new class instance for the <see cref="Packet"/>, corresponding to <paramref name="id"/> within this specific Packet Typer.
        /// <br />For examples on how to create your own Packet Typer, see <see cref="DefaultPacketTyper"/>.
        /// </summary>
        /// <param name="id">The ID to return an instance of.</param>
        /// <returns>A instantiated instance of the class for the <see cref="Packet"/> that <paramref name="id"/> corresponds to.</returns>
        public abstract Packet GetPacketObjectFromId(int id);
    }
}
