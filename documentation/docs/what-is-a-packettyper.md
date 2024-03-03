# What is a PacketTyper?

On our [Basic IPC Setup](basic-setup.md) page, we briefly mentioned PacketTypers. Saying the examples would use the "default" one and that "custom ones" were also possible, but that's about as far as we went.

Here we'll explain more of what a PacketTyper actually is, and detail what it does.

## So, what is it?
Simply put, a PacketTyper allows the server and client to take a packet ID when it receives a packet, and easily map that to the packet's specific class. They do this by calling a method within the typer and passing it the ID.
As an example, let's look at the [DefaultPacketTyper](~/api/Akiyama.IPC.Shared.Typers.DefaultPacketTyper.yml) class:

```csharp
public class DefaultPacketTyper : PacketTyper
{
    public override Packet GetPacketObjectFromId(int id)
    {
        switch (id)
        {
            case 255: return new TestPacket();
            case 1: return new StringPacket();
            case 2: return new IntPacket();
            case 3: return new ShortPacket();
            case 4: return new LongPacket();
            case 5: return new UIntPacket();
            case 6: return new UShortPacket();
            case 7: return new ULongPacket();
            case 8: return new GenericDataPacket();
            case 100: return new GZipPacket();
            default: throw new UnknownPacketException(id);
        }
    }
}
```
> [!NOTE]
> Usually this class uses [an enum](~/api/Akiyama.IPC.Shared.Network.PacketType.yml) for type comparisons, however to make this easier to understand, these have been translated directly into integers for the sake of this example. The comments and documentation strings have also been removed to make it easier to read.

You will see here that inside the method `GetPacketObjectFromId` which takes an `int` as an argument, `id` is compared in a switch statement and then the method will return whichever class matches that ID. If there is no match, the method reaches the `default` case and an `UnknownPacketException` is thrown.
> [!NOTE]
> Comparisons do not need to be done using a switch statement, however personally, and for this example, that is the approach I prefer. More examples of how it can be done can be found in [Custom PacketTypers](custom-packettypers.md).
