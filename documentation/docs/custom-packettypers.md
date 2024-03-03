# Custom PacketTypers

Custom [PacketTypers](what-is-a-packettyper.md) allow developes to customise their IPC implementation by adding the ability to support custom packets.

> [!WARNING]
> In order to ensure parity, both the server and the client must have the same mapping for packet IDs as well as access to the same packet classes themselves. For this reason, instead of creating duplicated copies on both sides, it is recommended to put your custom Typer(s) and Packets into their own Dynamic Link Library. That way both ends can simply reference them from there, and it automatically means they're both the same.

## Creating a Custom PacketTyper

### Class structure

Creating a custom Typer is as easy as creating a new class and inheriting the [PacketTyper](~/api/Akiyama.IPC.Shared.Typers.PacketTyper.yml) class:

```csharp
public class MyPacketTyper : PacketTyper
```

### Method override

Once you've created your class you'll need to make sure to override the `GetPacketObjectFromId(int)` method. There is where the typing will take place.

```csharp
public override Packet GetPacketObjectFromId(int id) 
{ 

}
```
> [!NOTE]
> This section will not cover how to create your own Packets. For that, see [Creating Custom Packets](creating-custom-packets.md).

> [!NOTE]
> Currently, packets with negative IDs are supported, however it is currently being discussed if support for this should be removed in favour of unsigned IDs instead in a later release.

### ID mapping

Now, within the method we just created, let's define our first packet:
```csharp
public override Packet GetPacketObjectFromId(int id) 
{ 
    switch (id) {
        case 1: return new MyPacket();
    }
}
```
> [!NOTE]
> While this example uses a [switch statement](https://learn.microsoft.com/dotnet/csharp/language-reference/statements/selection-statements#the-switch-statement) for matching IDs to classes, this is not the way you have to do it. So long as the method matches the ID to the correct class and returns an instance of it, any implementation method is fine.
>
> You also do not need to "hard code" the values into the statement, you can use a const field, property, or even an enum if you wish. Though the latter will need to be converted appropriately.

### Unknown packets

While were updating this method, now is a good time to add our default, or "fallback" case - that is the case that will happen if no match is found. For this, based on the previous example, we add a `default` clause to the bottom our switch statement after all of our `case` clauses:
```csharp
    default: throw new UnknownPacketException(id);
```

Which means that our entire method now looks like this:
```csharp
public override Packet GetPacketObjectFromId(int id) 
{ 
    switch (id) {
        case 1: return new MyPacket();
        default: throw new UnknownPacketException(id);
    }
}
```

This will make it so that if our Typer doesn't find a matching ID, be that because it's actually not a valid ID, or we simply forgot to add a new packet we created, we'll know what's happened.

> [!NOTE]
> If the `GetPacketObjectFromId` method returns a `null` value, the same exception will be automatically raised by the endpoint's [PacketConstructor](~/api/Akiyama.IPC.Shared.Network.PacketConstructor.yml).

### Enforcing Limits or Configuration

If you don't want to do it witin [packet's class itself](creating-custom-packets.md#enforcing-limits-and-configurations), you can use your typer to enforce certain limits on any packets such as their maximum allowed payload length, or AutoDispose status by including those in the constructors within your typer.

For example, enforcing a payload length limit:
```csharp
case 1: return new MyPacket(maxPayloadLength: 100);
```
> [!NOTE]
>For more information on these configuration options, see [Creating Custom Packets](creating-custom-packets.md#enforcing-limits-and-configurations).

### Using the custom packet typer

Now that we've created our own Packet Typer, we need to tell our server and client to use it. To do that, we simply pass them an instance of our typer's class when we're created our server/client instances:

```csharp
IPCServer myServer = new IPCServer("my-awesome-pipe", new MyPacketTyper());
IPCClient myClient = new IPCClient("my-awesome-pipe", new MyPacketTyper());
```

With that done, we can now sent our custom packet to our client or server, and observe the results.

> [!NOTE]
> You can use the basic implementation we created in [Basic IPC Setup](basic-setup.md) to test sending and receiving packets to the server/client.
