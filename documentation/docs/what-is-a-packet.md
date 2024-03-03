# What is a Packet?
A Packet is a bunch of data packed together (hence "Packet") in a way that makes it easy for both ends to easily understand what the data is about. While `streams` send and read data byte-by-byte. A packet, while still technically sent byte-by-byte can be sent, constructed and then dealt with all at once, instead of a byte at a time. In a nutshell, packets make it easier to process the data before it's sent and after it's received.

> [!NOTE]
> The explanation here is simplified a lot to make it easier for those unfamiliar to understand the basic principle of network packets.

Packets contain their data in the form of `header` data, and `payload` data.

The `header` tells the receiver what the packet is, what it contains and also allows the reciever to infer what it should do with the information in the `payload`.

The `payload` contains the data which the receiver should do something with, which is, again inferred from the data within the header.

Some packets may also contain data in the form of a `trailer` (also sometimes called a `footer`) as well as the `header` and `payload`.

> [!NOTE]
> Footers (or Trailers) are not currently supported in Akiyama.IPC at a base level. Though with the use of [custom packets](creating-custom-packets.md), they could be unofficially implemented if the developer chooses to do so.