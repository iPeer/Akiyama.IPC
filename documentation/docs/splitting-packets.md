# Splitting Packets

Akiyama.IPC will not automatically split packets before sending them, however it does facilitate splitting them manually, and can automatically merge them on the way in.

## Warnings & Tips

Before going in to this section, there are several things that need to be detailed:

> [!CAUTION]
> * Using split packets requires the use of the **first 2 bytes** of the [Custom Header Bytes](packets-fundamentals.md#custom-header-data). Anything within these bytes will be overwritten when a packet is split.
> * When the Endpoint's automatic handling of split packets is enabled, the usual `PacketReceived` event is not fired when receiving a split packet. Instead, no events are raised until all pieces of the packet are received, after which the `SplitPacketsReceived` event will be raised.
> * The packet received in the event after all pieces are recieved is **not** automatically disposed of and must be done so manually.
> * When sending multiple split packets at once (actual separate packets that have been split - not the pieces themselves), there is no guarantee they will arrive in the same order they are sent.
> * Before a packet is split into its pieces, its `Prepare` method is called.
> * Packets that have been split will not have their `Prepare` methods called *before sending*.
> * Packets that have been split will not have their `Populate` methods called after receiving.
> * The Packet's split index (what # piece this specific packet is in the split) will be at the **first** byte within the [Custom Header Bytes](packets-fundamentals.md#custom-header-data).
> * The total splits in a packet's respective split will be at the **second** byte within the [Custom Header Bytes](packets-fundamentals.md#custom-header-data).
> * Both the split index, and split total are **zero-based**. More details in [Split Packet Custom Header Bytes](#split-packet-custom-header-bytes).

> [!TIP]
> * Pieces of split packets do not need to be recieved in order.
> * Multiple split packets of the same type can be handled at the same time.
> * Each "part" of a split packet is called a "piece".
> * A "split" is a collection of "pieces".

## Splitting Packets

> [!WARNING]
> The packet you are splitting will be disposed of after it has been split.

To split a packet, simply use the [SplitPacket](~/api/Akiyama.IPC.Shared.Network.PacketConstructor.yml#Akiyama_IPC_Shared_Network_PacketConstructor_SplitPacket_Akiyama_IPC_Shared_Network_Packets_Packet_System_Int32_) method within the [PacketConstructor](~/api/Akiyama.IPC.Shared.Network.PacketConstructor.yml) class.

```csharp
List<Packet> splits = PacketConstructor.SplitPacket(myPacket, 5000);
```
This code above will take `myPacket` and split it at every `5,000` bytes of its payload.

> [!NOTE]
> * If the packet does not need to be split, a list containing only the *original* packet will be returned.
> * The source type of the `List` should always be `Packet` regardless of what the type of the packet you are splitting is.

Once the packet is split, we can send it to the opposing [endpoint](~/api/Akiyama.IPC.Shared.Network.IPCEndpoint.yml):
```csharp
myClient.SendPackets(splits);
```

## Receiving Split Packets

My default, an [endpoint](~/api/Akiyama.IPC.Shared.Network.IPCEndpoint.yml) will automatically handle receiving split packets. 

> [!NOTE]
> * If automatic handling is **enabled**, the `PacketReceived` event will not be raised when receiving a packet. Instead, the `SplitPacketsReceived` event will be raises *once all pieces have been received*.
> * If automatic handling is **disabled** the `PacketReceived` event will be raised for each received packet, and you'll need to handle merging the packets yourself.

If you wish to disable the automatic handling, you can disable it with:
```csharp
myEndpoint.AutoHandleSplitPackets = false;
```

If automatic handling is disabled, you will need to merge the incoming packets yourself. To help with this, you can use the [SplitPacketContainer](~/api/Akiyama.IPC.Shared.Helpers.SplitPacketContainer.yml) class to handle all the heavy lifting for you, or, you can also implement your own solution.

## Split Packet [Custom Header Bytes](packets-fundamentals.md#custom-header-data)

The bytes detailing the Packet's current positiono within the split, and the total amount of packets that are incoming are written to the first `two` bytes of the [Custom Header Bytes](packets-fundamentals.md#custom-header-data).

As an example, let's say our first 2 bytes are `01 06`, this would indicate that:
* This packet is the *second* piece within this particular split, and;
* This split has *seven* pieces.