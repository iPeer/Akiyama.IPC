# Packet Fundamentals
This page documents some things that are what we consider to be required knowledge when dealing with packets within Akiyama.IPC.

## Packet Payload

### Setting

> [!CAUTION]
> When setting a packet's payload, any existing payload data will be overwritten. If you need to keep the existing data, make sure to save it somewhere.

A packet's payload can be set in one of 3 ways:
1. At creation via its constructor:
```csharp
byte[] payloadBytes = PacketConstructor.StringToBytes("Hello World");
MyPacket myPacket = new MyPacket(data: payloadBytes);
```
2. After creation, using the [SetPayload](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_SetPayload_System_Byte___) method:
```csharp
byte[] payloadBytes = PacketConstructor.StringToBytes("Hello World");
MyPacket myPacket = new MyPacket();
myPacket.SetPayload(payloadBytes);
```
3. Automatically before being sent using the packet's `Prepare` method.
> [!NOTE]
> For more information on the `Prepare` method, see [Creating Custom Packets](creating-custom-packets.md#prepare-method).

### Reading

Reading a packet's payload can be done by simply referencing its `Payload` property:
```csharp
byte[] thePayload = myPacket.Payload;
```

## Packet Length Limit

> [!CAUTION]
> Packets with extremely long payloads can negatively impact performance. Extremely long payloads can take the packet to take longer to be sent and/or read on the opposing side. For packets with extremely large payloads, it is recommended to split it across multiple Packets.

> [!WARNING]
> Packet payload limits are not conveyed by the packet when it is transferred over IPC. These limits should be enforced by the [packet's class file](creating-custom-packets.md#enforcing-limits-and-configurations), or [by the PacketTyper](custom-packettypers.md#enforcing-limits-or-configuration). 

Since Akiyama.IPC packets use a signed 32-bit integer to determine their length, the hard limit on payload length is `2,147,483,647` bytes. All packets will follow this length by default, however, if required, developers may limit the length of their packet's supported payload with one of two methods:
1. When creating the instance of the packet:
```csharp
MyPacket myPacket = new MyPacket(maxPayloadLength: 100);
```
2. After creating the packet, using the [SetMaxLength](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_SetMaxLength_System_Int32_) method:
```csharp
MyPacket myPacket = new MyPacket();
myPacket.SetMaxLength(100);
```

Both of these will set the packet's maximum allowed payload length to be 100 bytes. 

> [!TIP]
> A packet's current maximum length limit can be accessed via the `MaxPayloadLength` property:
>```csharp
>int maxLen = myPacket.MaxPayloadLength;
>```


> [!TIP]
> Attempting to set the payload to one that exceeds the packet's size limit, or attempting to set a limit on a packet whose payload currently exceeds the given limit will result in an [InvalidOperationException](https://learn.microsoft.com/dotnet/api/system.invalidoperationexception).

## Disable Automatic Header Updates

If your packet has a large payload, or is currently receiving a lot of payload updates, it may be beneficial performance wise to disable automatic header updates until all your processing is complete. To do this simply set the packet's `AutomaticHeaderUpdatesDisabled` to `true`:

```csharp
myPacket.AutomaticHeaderUpdatesDisabled = true;
```

> [!NOTE]
> Headers are **not** automatically updated when this property is changed back to `false`. A payload change, or manual calling of [UpdateHeader](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_UpdateHeader) are required.

> [!IMPORTANT]
> As described, when this property is set to `true`, automatic header updates are disabled. This means that the payload length bytes in the packets will not be updated automatically. You **must** manually update the header yourself when you're done via [UpdateHeader](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_UpdateHeader).

## Disabling AutoDispose

By default, after packets are sent, the endpoint will automatically dispose of them, freeing up the memory and other resources they may have been using.

Sometimes you may with to prevent this behaviour, this can be done in two ways:
1. When creating the instance of the packet:
```csharp
MyPacket myPacket = new MyPacket(autoDispose: false);
```
2. After creating the packet, using the [SetAutoDispose](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_SetAutoDispose_System_Boolean_) method:
```csharp
MyPacket myPacket = new MyPacket();
myPacket.SetAutoDispose(false);
```
> [!CAUTION]
> Disabling AutoDispose means that you are responible for manually ensuring that packets are properly disposed of. Not doing so could result in your application(s) using lots of memory and potentially even crashing.

## Custom Header Data
In their headers, Packets contain bytes that can be freely modified for anything you want. These bytes occupy the last 12 bytes in the header and are preserved on the receiving end.

> [!NOTE]
> The functions described below refer to a zero-indexed index within the Custom Header Data. As such, when using, for example index `3`, you will be referring to the `4th` byte in the data. Using an index of `0` will refer to the first byte.

### Writing

To set a single, specific byte of the header, the [SetCustomHeaderByte](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_SetCustomHeaderByte_System_Byte_System_Int32_) method can be used:
```csharp
TestPacket myPacket = new TestPacket();
myPacket.SetCustomHeaderByte(3, 10);
```
This sets the 11th byte within the custom header data to the byte `3`.

Alternatively, to set a range of bytes, [SetCustomHeaderBytes](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_SetCustomHeaderBytes_System_Byte___System_Int32_) can be used:
```csharp
TestPacket myPacket = new TestPacket();
byte[] data = new byte[] { 3, 4, 5 };
myPacket.SetCustomHeaderBytes(data, 8);
```
This will set the bytes starting at position 7 through to position 10 (again, zero-indexed) to `3 4 5`.

### Reading

Reading the custom data is very similar to setting it.
To read a single byte, the [GetCustomHeaderByte](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_GetCustomHeaderByte_System_Int32_) method can be used:
```csharp
byte data = myPacket.GetCustomHeaderByte(10);
```
This will get the value of the 11th custom header byte, and store it in the variable `data`. If we use the [example above](#writing), `data` now contains the byte `3`.

Alternatively, you can supply a byte array as a buffer for the bytes to be read into starting at the specified offset using [GetCustomHeaderBytes](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_GetCustomHeaderBytes_System_Byte___System_Int32_):
```csharp
TestPacket myPacket = new TestPacket();
byte[] data = new byte[3];
myPacket.GetCustomHeaderBytes(data, 8);
```
Again, using the [example above](#writing), `data` is now a byte array containing the values `3 4 5`.

The final way to reteive a range of bytes is by using the alternate [GetCustomHeaderBytes](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_GetCustomHeaderBytes_System_Int32_System_Int32_) which takes two `integers`, the `offset`, and the `length`:

```csharp
TestPacket myPacket = new TestPacket();
byte[] data = myPacket.GetCustomHeaderBytes(8, 3);
```
Using this method returns a new `byte array` containing the values starting at  `offset` and going for `length` bytes. In this example they are the same as the previous method, `3 4 5`.
