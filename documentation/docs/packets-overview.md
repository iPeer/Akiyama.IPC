# Packets Overview

This page aims to give an overview to what Packets are in the context of Akiyama.IPC.

## How This Library Uses Packets

Akiyama.IPC uses packets to send data back and forth between the server and client endpoints, while also presenting them in an easy to work with format for developers looking to implement things.

Packets within Akiyama.IPC follow the general structure of "normal" packets, in that they have a header containing information about them and their payload, as well as the payload itself.

For a complete overview of Akiyama.IPC's packet format, see [IPC Packet Protocol](ipc-packet-protocol.md#ipc-packet-format).

## What Data Can Be Sent In Packets?
Any. There are no limitations on what data you can send or receive using Akiyama.IPC. While we do implement some [default packets](~/api/Akiyama.IPC.Shared.Network.Packets.yml) to use, you don't have to use them. There's nothing stopping you from [creating your own](creating-custom-packets.md) to fit whatever purpose you need.

## How Packets Are Sent

> [!NOTE]
> For a complete overview of how Akiyama.IPC handles packets, see [IPC Packet Protocol](ipc-packet-protocol.md).

Packets in Akiyama.IPC are sent over what are called "Named Pipes". Named Pipes allow two or more processes to communicate with each other without having to rely on files or operating system messages.

## How Packets Are Received

> [!NOTE]
> For a complete overview of how Akiyama.IPC handles packets, see [IPC Packet Protocol](ipc-packet-protocol.md).

Packets sent to an Akiyama.IPC endpoint are read from the pipe's stream and then constructed by their [PacketConstructor](~/api/Akiyama.IPC.Shared.Network.PacketConstructor.yml) into something that developers can easilly interact with.
