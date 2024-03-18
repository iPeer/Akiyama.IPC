# Update History

## 1.2.0
* Both versions of the framework (.NETFramework and .NET6+) now utilise a shared codebase instead of separate ones.
  - **Note**: This is a non-breaking change and will not require any changes to existing implementations.
* `IPCEndpoint` class received the following changes:
  - Can now natively support and automatically combine incoming split packets.
  - New Properties:
    - [AutoHandleSplitPackets](~/api/Akiyama.IPC.Shared.Network.IPCEndpoint.yml#Akiyama_IPC_Shared_Network_IPCEndpoint_AutoHandleSplitPackets)
    - [BytesSent](~/api/Akiyama.IPC.Shared.Network.IPCEndpoint.yml#Akiyama_IPC_Shared_Network_IPCEndpoint_BytesSent)
    - [BytesReceived](~/api/Akiyama.IPC.Shared.Network.IPCEndpoint.yml#Akiyama_IPC_Shared_Network_IPCEndpoint_BytesReceived)
  - New Events:
    - [SplitPacketsReceived](~/api/Akiyama.IPC.Shared.Network.IPCEndpoint.yml#Akiyama_IPC_Shared_Network_IPCEndpoint_SplitPacketsReceived)
* `PacketConstructor` class received the following changes:
  - Changed protection level of the `packetTyper` property from `private` to `internal`.
  - New Fields:
    - [MAX_PACKET_SPLITS](~/api/Akiyama.IPC.Shared.Network.PacketConstructor.yml#Akiyama_IPC_Shared_Network_PacketConstructor_MAX_PACKET_SPLITS)
  - New Methods:
    - [SplitPacket(Packet, int)](~/api/Akiyama.IPC.Shared.Network.PacketConstructor.yml#Akiyama_IPC_Shared_Network_PacketConstructor_SplitPacket__1___0_System_Int32_)
    - [SplitPacket(Packet, int, byte)](~/api/Akiyama.IPC.Shared.Network.PacketConstructor.yml#Akiyama_IPC_Shared_Network_PacketConstructor_SplitPacket__1___0_System_Int32_System_Byte_)
* `Packet` class received the following changes:
  - `Packet._disposed` has been renamed to `Packet.Disposed`
  - `Packet.Disposed` is now also a publicly gettable Property.
    - This means child classes can now also check its status to determine if they have already been disposed of, and act accordingly.
  - New Properties:
    - [IsSplit](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_IsSplit)
    - [SplitID](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_SplitID)
  - New Methods:
    - [internal] [SetIsSplit(bool)](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_SetIsSplit_System_Boolean_)
    - [internal] [SetSplitId(byte)](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_SetSplitId_System_Byte_)
* New class: [SplitPacketContainer](~/api/Akiyama.IPC.Shared.Helpers.SplitPacketContainer.yml)
* Fixed `GZipPacket` not correctly disposing of itself.
* Fixed an exception that could be thrown from `StringPacket` if its `Text` property wasn't changed.
* Changed the order of some checks in `IPCEndpoint` to reduce the chances that the endpoint attempts to read from a nulled stream.
* A packet's `CustomHeaderBytes` are now correctly disposed of when said packet is disposed.
* Removed several methods and properties that were deprecated in v1.1.0 referring to a Packet's "Data" instead of "Payload"

## 1.1.0

* `Packet` class received the following changes:
  - New Properties:
    - [CustomHeaderBytes](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_CustomHeaderBytes)
    - [Version](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_Version)
  - New Methods:
    - [Init](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_Init)
    - [AppendPayload(byte[])](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_AppendPayload_System_Byte___)
    - [private] [UpdateCustomHeaderBytes](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_UpdateCustomHeaderBytes)
    - [internal] [SetVersion(byte, byte, byte)](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_SetVersion_System_Byte_System_Byte_System_Byte_)
  - New Fields:
    - [BASE_HEADER_SIZE](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_BASE_HEADER_SIZE)
    - [CUSTOM_HEADER_BYTES](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_CUSTOM_HEADER_BYTES)
* `PacketTyper` class received the following changes:
  - New Fields:
    - [MINIMUM_PACKET_VERSION](~/api/Akiyama.IPC.Shared.Network.PacketConstructor.yml#Akiyama_IPC_Shared_Network_PacketConstructor_MINIMUM_PACKET_VERSION)

## 1.0.0

* Initial library release!