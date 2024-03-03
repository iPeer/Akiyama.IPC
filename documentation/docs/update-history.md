# Update History

## 1.1.0

* `Packet` class received the following changes:
  - New Properties:
    - [CustomHeaderBytes](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_CustomHeaderBytes)
    - [Version](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_Version)
  - New methods:
    - [Init](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_Init)
    - [AppendPayload](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_AppendPayload_System_Byte___)
    - [private] [UpdateCustomHeaderBytes](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_UpdateCustomHeaderBytes)
    - [internal] [SetVersion](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_SetVersion_System_Byte_System_Byte_System_Byte_)
  - New fields:
    - [BASE_HEADER_SIZE](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_BASE_HEADER_SIZE)
    - [CUSTOM_HEADER_BYTES](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#Akiyama_IPC_Shared_Network_Packets_Packet_CUSTOM_HEADER_BYTES)
* `PacketTyper` class received the following changes:
  - New fields
    - [MINIMUM_PACKET_VERSION](~/api/Akiyama.IPC.Shared.Network.PacketConstructor.yml#Akiyama_IPC_Shared_Network_PacketConstructor_MINIMUM_PACKET_VERSION)

## 1.0.0

* Initial library release!