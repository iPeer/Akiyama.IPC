# Creating Custom Packets
This page will go over the details of creating custom packets within your implementation for use with Akiyama.IPC.

> [!WARNING]
> If you are using custom packets in any capacity, you must also create and register a custom [PacketTyper](~/api/Akiyama.IPC.Shared.Typers.PacketTyper.yml). You can read more on creating custom PacketTypers [here](custom-packettypers.md).

> [!TIP]
> Since both ends of the connection will need to access both the packet classes and the [PacketTyper](~/api/Akiyama.IPC.Shared.Typers.PacketTyper.yml) you use, it maybe be useful to write them in a separate project that is compiled into a DLL along-side the client and server sides.

## Class Structure
### Basic class structure
The basic class structure of a custom packet class is simple. All you need is any class, and have it implement the [Packet](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml) class from Akiyama.IPC:
```csharp
public class MyPacket : Packet 
{

}
```

### Overriding the packet ID

Once setting this up, you'll notice that you will be required to over the `ID` property. This is where you define the ID of you packet, so make sure it's unique, and also make sure to add it correctly to your [PacketTyper](custom-packettypers.md).

> [!TIP]
> You can also inherit Akiyama.IPC's built-in packets if they provide functionality you're looking for. However, since you're creating custom packets, you will still need to use a custom [PacketTyper](custom-packettypers.md), and override the packet's `ID` property just like a base packet.

Once we've done that, your class will look something like this:
```csharp
public class MyPacket : Packet 
{
    public override int ID => 1;
}
```
... and that's it. Your packet is now completely functional, however, what if we want it to do a little more?

### Some added complexity

Let's say we want this packet to transmit a string, let's do that.

First of all, we're going to need somewhere to store the string we're sending or receiving, so let's define a Property within our class. You can place it under the ID override. In this example, we'll give it the name `Text`:
```csharp
public class MyPacket : Packet 
{
    public override int ID => 1;
    public string Text { get; set; } = string.Empty;
}
```
Here, as this is a `Property`, we use `{ get; set; }` (accessors) to allow our code to interact with it. `get` meaning the string can be read, and `set` that the string can be set. The following ` = string.Empty` just assigns the default value of the Property.

> [!TIP]
> Any Properties [contained within the base Packet class](~/api/Akiyama.IPC.Shared.Network.Packets.Packet.yml#properties) are accessible from within your class. This is in addition to those found inside [default packets](~/api/Akiyama.IPC.Shared.Network.Packets.yml) if you inherit one of those instead.

> [!TIP]
> You do not need to use a `Property` here if you don't want to. A plain [Field](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/fields) will also do the job just fine. The examples here use `Properties` as they can give a greater amount of control over what can and can't access them, and how they refer to the data they contain, which is something you may need later. For more information on `Properties` in C#, see [this link](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/properties).

### Populate & Prepare

Now, in order to assign this Property using the packet's payload automatically, we'll need to use the packet's `Populate` method. To see how to implement this method, see [this section](#populate-method).

If we want our packet to automatically populate its payload based on the value of the Property, we'll need to use the packet's `Prepare` method. To see how to implement this method, see [this section](#prepare-method).

> [!TIP]
> Usage of these methods is completely optional, though learning how to use them effectively will make manipulating packet data much easier as without them you'll have to do all the manipulation yourself everywhere it's needed, instead of just once within these methods.

With that done, your packet is now complete and can be used in your codebase!

## Prepare Method

> [!IMPORTANT]
> When using the `Prepare` method, your packet's payload will be replaced with whatever it is set to during that method. If you wish to preserve any payload data already present, you must do that manually.

The `Prepare` method is inteded to allow developers to write all the code that produces the packet's relevent [Properties](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/properties)/[Fields](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/fields) and automatically set the packet's Payload based on them.

Using our packet example from the [class structure](#some-added-complexity) section, we'll have our packet take its `Text` property, and set its payload to it's value automatically before being sent.

First we'll need to override the `Prepare` method from the base class:
```csharp
public override void Prepare() 
{

}
```
Now we've done that, all we need to do is appropriately handle the `Text` Property, and set the payload to the result:
```csharp
public override void Prepare() 
{
    byte[] textBytes = PacketConstructor.StringTobytes(this.Text);
    this.SetPayload(textBytes);
}
```

That is basically all we need to do to have the `Prepare` method set up, however there is one thing we need to be careful of: sometimes packets have [automatic header updates disabled](packets-fundamentals.md#disable-automatic-header-updates), especially when their headers are large, or frequently updated. So we'll want to add a check for this in our `Prepare` method to make absolutely sure the header gets updated, even if automatic updates are disabled.

To do this, we can simply add a simple if statement after all of out logic within our `Prepare` method:

```csharp
public override void Prepare() 
{
    byte[] textBytes = PacketConstructor.StringTobytes(this.Text);
    this.SetPayload(textBytes);

    // Force the header to be updated, even if automatic updates are disabled
    if (this.AutomaticHeaderUpdatesDisabled) { this.UpdateHeader(); }
}
```

With our `Prepare` method now complete, whenever this packet is about to be sent, its payload will automatically be updated, without it having to be done manually every time.

> [!NOTE]
> This is a very simple example use case of `Prepare`, and is how the default packet [StringPacket](https://github.com/iPeer/Akiyama.IPC/blob/master/Akiyama.IPC/Shared/Network/Packets/StringPacket.cs) does it. For a more complex, commented example, see the source code for the [TestPacket](https://github.com/iPeer/Akiyama.IPC/blob/master/Akiyama.IPC/Shared/Network/Packets/TestPacket.cs).

## Populate Method

> [!IMPORTANT]
> When using the `Populate` method, if you use it to populate your packet class' properties, their default values (if any) will be overwritten by the values assigned within the method. If you wish to preserve their initial values, you must do that manually.

The `Populate` method is intended to allow developers to write all the code that would process the packet's payload once and then just have that data accessible from the packet via the use of [Properties](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/properties) or [Fields](https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/fields).

Using our packet example from the [class structure](#some-added-complexity) section, we'll have our packet process its payload and fill the `Text` property automatically after being received.

First of all, the class will need to override the `Populate` method from the base class:

```csharp
public override void Populate() 
{

}
```
Once we have that we can simply add the code that assigns the property to the method:
```csharp
public override void Populate() 
{
    this.Text = PacketConstructor.BytesToString(this.Payload);
}
```
Now, when an endpoint receives this packet, the `Text` will automatically be set the value obtained from the packet's payload.

> [!NOTE]
> This is a very simple example use case of `Populate`, and is how the default packet [StringPacket](https://github.com/iPeer/Akiyama.IPC/blob/master/Akiyama.IPC/Shared/Network/Packets/StringPacket.cs) does it. For a more complex, commented example, see the source code for the [TestPacket](https://github.com/iPeer/Akiyama.IPC/blob/master/Akiyama.IPC/Shared/Network/Packets/TestPacket.cs).

## Init method

The `Init` method allows packets to do some setup when they're created, such as [enforcing limits](#enforcing-limits-and-configurations), or setting default values.

To implement this method, simply override the `Init` method from the base class:
```csharp
public override void Init() 
{
    this.SomeProperty = "SomeDefaultValue";
}
```

> [!IMPORTANT]
> This on receiving ends, this method is called **before** population of the packet's payload. For automating Payload processing, see [Populate](#populate-method).


## Enforcing Limits and Configurations

Sometimes you may want to enforce a certain maximum payload length, or even a certain value for the packet's AutoDispose setting. There are a couple of ways that this can be achieved.

The first is by creating a constructor within your packet's class with the restrictions included. For example, to **always** limit the maximum payload length of `MyPacket` to 100 bytes, we could do the following:
```csharp
public MyPacket() : base(maxPayloadLength: 100) { }
```
The other way of enforcing them is via the packet's `Init` method:
```csharp
public override void Init() 
{
    this.SetMaxPayloadLength(100);
}
```