using Akiyama.IPC.Client;
using Akiyama.IPC.Server;
using Akiyama.IPC.Shared.Events;
using Akiyama.IPC.Shared.Network;
using Akiyama.IPC.Shared.Network.Packets;
using System;
using System.Collections.Generic;
using System.IO;

namespace AkiyamaIPC.TestsNF
{
    internal class Program
    {
        static void Main(string[] args)
        {

            IPCServer server = new IPCServer("IPC-Tests-CLI");
            IPCClient client = new IPCClient("IPC-Tests-CLI");

            client.PacketReceived += OnPacketRecvClient;
            server.PacketReceived += OnPacketRecvServer;

            client.SplitPacketsReceived += OnSplitPacketRecvClient;
            server.SplitPacketsReceived += OnSplitPacketRecvServer;

            server.Start();
            Console.WriteLine("Server started, press any key to start client");
            Console.ReadKey();
            client.Start();
            Console.WriteLine("Client started, use the following to test packets:");
            Console.WriteLine("'split', 'string', 'string2', 'int', 'test'.\nYou will see the packet's data printed into the console!");
            Console.WriteLine("\nWrite 'exit' to terminate.");

            bool keepRunning = true;

            while (keepRunning)
            {
                string command = Console.ReadLine();
                switch (command.ToLower())
                {
                    case "exit":
                        client.Stop();
                        server.Stop();
                        client.Dispose();
                        server.Dispose();
                        keepRunning = false;
                        break;
                    case "test":
                        using (TestPacket tp = new TestPacket())
                        {
                            server.SendPacket(tp);
                        }
                        break;
                    case "string":
                        Console.WriteLine("Enter a string to send:");
                        string @string = Console.ReadLine();
                        using (StringPacket sp = new StringPacket())
                        {
                            sp.Text = @string;
                            server.SendPacket(sp);
                        }
                        break;
                    case "string2":
                        Console.WriteLine("Enter a string to send in the FIRST packet:");
                        string stringOne = Console.ReadLine();

                        Console.WriteLine("Enter a string to send in the SECOND packet:");
                        string stringTwo = Console.ReadLine();

                        StringPacket packetOne = new StringPacket();
                        packetOne.SetCustomHeaderByte(1, 0);
                        packetOne.Text = stringOne;

                        StringPacket packetTwo = new StringPacket();
                        packetTwo.SetCustomHeaderByte(2, 0);
                        packetTwo.Text = stringTwo;

                        server.SendPackets(new Packet[] { packetOne, packetTwo });

                        StringPacket packetThree = new StringPacket();
                        packetThree.SetCustomHeaderByte(3, 0);
                        packetThree.Text = "A completely unrelated string packet";
                        server.SendPacket(packetThree);

                        break;
                    case "int":
                        Console.WriteLine("Enter an integer to send:");
                        string intStr = Console.ReadLine();
                        int @int = int.Parse(intStr);
                        using (IntPacket ip = new IntPacket())
                        {
                            ip.Value = @int;
                            server.SendPacket(ip);
                        }
                        break;
                    case "split":
                        Console.WriteLine("Enter the string to send:");
                        string splitStr = Console.ReadLine();
                        if (splitStr == null || splitStr.Length < 5) { splitStr = "The Quick Brown Fox Jumped Over The Lazy Dog."; }
                        List<Packet> packets = new List<Packet>();
                        using (StringPacket split = new StringPacket())
                        {
                            split.Text = splitStr;
                            int index = 0;
                            foreach (StringPacket _packet in PacketConstructor.SplitPacket(split, 5))
                            {
                                packets.Add(_packet);
                                if (index++ == 0)
                                {
                                    StringPacket stringPacket = new StringPacket
                                    {
                                        Text = "----!!---- This is a completely unrelated string packet to test non-sequential split packets :)"
                                    };
                                    packets.Add(stringPacket);
                                }
                            }
                            server.SendPackets(packets);
                        }

                        break;
                }
            }

        }

        // !!! This is a TEST case, don't do this in real code! In a test we don't need to differentiate between a normal and split packet, we just need to know that we received it !!!
        static void OnSplitPacketRecvClient(object sender, OnAllSplitPacketsReceivedEventArgs e) => OnPacketRecvClient(sender, new OnPacketReceivedEventArgs(e.Packet)); // Yo this is DIRTY
        static void OnSplitPacketRecvServer(object sender, OnAllSplitPacketsReceivedEventArgs e) => OnPacketRecvServer(sender, new OnPacketReceivedEventArgs(e.Packet));

        static void OnPacketRecvClient(object sender, OnPacketReceivedEventArgs eventArgs)
        {
            Packet packet = eventArgs.Packet;
            Console.WriteLine($"[CLIENT] Packet received: {packet.TotalLength} bytes");
            Console.WriteLine($"[CLIENT] Header: 0x{string.Join(", 0x", packet.Header)}");
            Console.WriteLine($"[CLIENT] Data: 0x{string.Join(", 0x", packet.Payload)}");

            PacketType pt = (PacketType)packet.ID;
            if (pt == PacketType.STRING)
            {
                Console.WriteLine($"[CLIENT] String packet, got value: {((StringPacket)packet).Text}");
                using (FileStream fr = new FileStream("./packet.bin", FileMode.OpenOrCreate))
                {
                    fr.Write(packet.Header, 0, packet.HeaderLength);
                    fr.Write(packet.Payload, 0, packet.PayloadLength);
                }
            }
            else if (pt == PacketType.INT)
            {
                Console.WriteLine($"[CLIENT] Int packet, got value: {((IntPacket)packet).Value}");
            }
            else if (pt == PacketType.TEST_PACKET)
            {
                Console.WriteLine($"[CLIENT] Test packet - String: {((TestPacket)packet).Text}, Int: {((TestPacket)packet).ANumber}");
            }
            else if (pt == PacketType.GENERIC_DATA)
            {
                Console.WriteLine($"[CLIENT] Generic_Data packet! - Payload length: {packet.PayloadLength}");
            }
            packet.Dispose();
        }

        static void OnPacketRecvServer(object sender, OnPacketReceivedEventArgs eventArgs)
        {
            Packet packet = eventArgs.Packet;
            Console.WriteLine($"[SERVER] Packet received: {packet.TotalLength} bytes");
            Console.WriteLine($"[SERVER] Header: 0x{string.Join(", 0x", packet.Header)}");
            Console.WriteLine($"[SERVER] Data: 0x{string.Join(", 0x", packet.Payload)}");

            PacketType pt = (PacketType)packet.ID;
            if (pt == PacketType.STRING)
            {
                Console.WriteLine($"[SERVER] String packet, got value: {((StringPacket)packet).Text}");
            }
            else if (pt == PacketType.INT)
            {
                Console.WriteLine($"[SERVER] Int packet, got value: {((IntPacket)packet).Value}");
            }
            else if (pt == PacketType.TEST_PACKET)
            {
                Console.WriteLine($"[SERVER] Test packet - String: {((TestPacket)packet).Text}, Int: {((TestPacket)packet).ANumber}");
            }

        }

    }
}
