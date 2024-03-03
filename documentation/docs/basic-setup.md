# Basic IPC Setup

> [!NOTE]
> Usually, this is done in two separate processes (hence "Inter-process"), however for the sake of this tutorial, we'll be doing both in the same application.

> [!NOTE]
> All of the examples in this documentation will be using the default PacketTyper. For info on how to create and use your own, see [Custom PacketTypers](custom-packettypers.md).

## Setup

In order to begin sending and recieving data, we first need to set up our server and client.

First, we need to create a server:
```csharp
IPCServer myServer = new IPCServer("my-awesome-pipe");
```
Here, `my-awesome-pipe`, is the name of the pipe. We'll need this as we'll need to tell our client to connect to this as well.
Speaking of our client, let's create that as well:
```csharp
IPCClient myClient = new IPCClient("my-awesome-pipe");
```

## Events

The next part of the setup involves actually getting the packets out of the endpoints. In order to do this, we need to subscribe to their `PacketReceived` events. But first we'll need methods that can handle the incoming packets. For this we'll need to create two methods, one for the client, and one for the server.

We'll keep these methods simple for now and have them just print the incoming packet's ID to the console.
First, the one for the server:
```csharp
private void ServerOnPacketReceived(object sender, OnPacketReceivedEventArgs e) {
	Console.WriteLine($"[SERVER] Got packet ID {e.Packet.ID}");
}
```
... and now the same for the client:
```csharp
private void ClientOnPacketReceived(object sender, OnPacketReceivedEventArgs e) {
	Console.WriteLine($"[CLIENT] Got packet ID {e.Packet.ID}");
}
```
Now that we have our handling methods set up, we need to actually get the data passed to them, to do this, we subscribe to the respective endpoint's `PacketReceived` event, and add our method to it:
```csharp
myServer.PacketReceived += ServerOnPacketReceived;
myClient.PacketReceived += ClientOnPacketReceived;
```
> [!NOTE]
> Another valid way to do this is by using lambda expressions such as:
> ```csharp
> myServer.PacketReceived += (sender, _event) => { Console.WriteLine($"[SERVER] {_event.Packet.ID}"); }
> ```
> But for ease of reading, we recommend sticking to dedicated methods for now.

## Starting instances

Now that we have both our server and client set up, let's start them both:
```csharp
myServer.Start(); // Start the server
myClient.Start(); // Start the client
```

## Sending a Packet

Now, if we run our code as it is, both instances will just kind of sit there doing nothing, so let's change that.
Let's have the server send a simple string to the client:
```csharp
using (StringPacket sp = new StringPacket()) { // Create a new StringPacket - we use 'using' here so the packet is automatically disposed of when no longer needed (see note below).
	sp.Text = "Hello from the server!"; // Set the string we want to set.
	myServer.SendPacket(sp); // Send the packet to the client!
}
```
> [!NOTE]
> It is usually not required to use a `using` statement when sending a packet in most cases, as packets are automatically disposed after being sent. However in cases where that functionality [is disabled](packets-fundamentals.md#disabling-autodispose), it is required to manually handle disposal. In most cases, the `using` statement in almost all cases has little impact on performance, so it's generally a good habit to get into anyway.

After the packet has been send, you should see a new message printed in your console:
```
[CLIENT] Got packet ID 1
```

## Stopping instances

Now that we've got our packet from one side to the other, we can gracefully shut down the server and client:
```csharp
myServer.Stop();
myClient.Stop();
```

## Closing statement
Congratulations, you just did your first Inter-process communication using Akiyama.IPC! Play around with this a little and see what you can learn. Perhaps you can find a way to have the client print the text you sent from the server into the console? Or even make the client reply to the server with its own packet!
