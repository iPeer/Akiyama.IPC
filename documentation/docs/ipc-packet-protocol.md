# IPC Packet Protocol

This page acts to describe the protocol which Akiyama.IPC implements.

## IPC Packet Format

Below is an overview of the general layout of the bytes contained within an Akiyama.IPC packet.

The packet used in this example is a [`StringPacket`](~/api/Akiyama.IPC.Shared.Network.Packets.StringPacket.yml), with a payload of `Hello World`.

> [!NOTE]
> Typically, a hexadecimal readout is 16 bytes per line, however for the sake of visualisation, here we are showing the `header` on the top line, and the `payload` on the bottom.

```
              Payload     Packet
| Packet ID | Length    | Ver.   | Custom header data                |
 01 00 00 00 0B 00 00 00 01 01 00 00 00 00 00 00 00 00 00 00 00 00 00 
| Payload                    ... |
 48 65 6C 6C 6F 20 57 6F 72 6C 64

```
The first 4 bytes consist of the packet ID:   
`01 00 00 00`  
The next 4 bytes consist of the packet's payload length:  
`0B 00 00 00`  
The next 3 bytes make up the version of the **sending side**'s assembly version.  
`01 01 00`  
The next 12 bytes consist of the packet's Custom Header Data bytes:  
`00 00 00 00 00 00 00 00 00 00 00 00`  
The remaining bytes make up the packet's payload (in this case, the string `Hello World`):  
`48 65 6C 6C 6F 20 57 6F 72 6C 64`

> [!TIP]
> If you wish to examine this in your own time and/or favourite hex editor, you can download this sample data from the [examples](~/examples/index.md) page.

## Sending Packets

When sending or receiving a packet, the [IPCEndpoint](~/api/Akiyama.IPC.Shared.Network.IPCEndpoint.yml) ("endpoint") prepares the packet by setting up an array of bytes. The length of this array is calculated using:

$1 + HeaderLength + PayloadLength$.
 
After creation of this array, the endpoint sets the first byte of it to the [`Pre Packet Byte`](~/api/Akiyama.IPC.Shared.Network.PacketConstructor.yml#Akiyama_IPC_Shared_Network_PacketConstructor_PRE_PACKET_BYTE). This byte is used so that when reading the inbound stream, it ca read this byte and knows that the bytes following it are a packet.

After setting the first byte, the endpoint then copies the header and then the payload into the array. Once this is complete it is written out to the outbound stream.

## Receiving Packets

One the opposing endpoint, the connection receives the new incoming data, and reads the first byte, if that byte is the Pre Packet Byte, then it skips that byte and immediately asks the [PacketConstructor](~/api/Akiyama.IPC.Shared.Network.PacketConstructor.yml) to create a packet from the bytes remaining in the stream.

To parse the packet from the stream, the packet constructor first reads the first 4 bytes to get the packet ID, and stores these into memory.

Next, the constructor reads the following 12 bytes. This data is the [Custom Header Bytes](packets-fundamentals.md#custom-header-data) for this packet. It stores these in memory.

Before reading the payload, the constructor needs to know how big it is, to find that out, the constructor reads another 4 bytes.

Before reading the payload, the constructor now converts the first 4 bytes and the last 4 bytes into integers.

Once done, the constructor calls the `GetPacketObjectFromId` method of its registered [PacketTyper](~/api/Akiyama.IPC.Shared.Typers.PacketTyper.yml) to attempt to initialise an instance of the correct packet for the ID it received.

> [!NOTE]
> If the typer does not return a value, then an [InvalidOperationException](https://learn.microsoft.com/dotnet/api/system.invalidoperationexception) will be thrown.

Once the constructor has a packet instance, it sets the classes [Custom Header Bytes](packets-fundamentals.md#custom-header-data) to the same ones it received.

Immediately following the custom header bytes, the constructor then sets up another byte array with a length of what is parsed from the packet header earlier, and then writes the payload into this array. It is then written into the instance's payload.

> [!NOTE]
> The [PacketConstructor](~/api/Akiyama.IPC.Shared.Network.PacketConstructor.yml) never writes any header information other than the Custom Header Bytes. The ID bytes are inferred by the packet type that is being created, and the length is automatically set when the packet's payload is set.

After the setting the payload, the Constructor calls the packet's [`Populate`](creating-custom-packets.md#populate-method) method.

Finally, the packet is returned to the endpoint, and the endpoint raises its [`PacketReceived`](basic-setup.md#events) event. At this point, the scope changes to your code to handle the packet however is required.