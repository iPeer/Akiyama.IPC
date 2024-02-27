using Akiyama.IPC.Client;
using Akiyama.IPC.Server;
using Akiyama.IPC.Shared.Events;
using Akiyama.IPC.Shared.Network;
using Akiyama.IPC.Shared.Network.Packets;
using System;

namespace AkiyamaIPC.TestsNF
{
    internal class Program
    {
        static void Main(string[] args)
        {

            PipeServer server = new PipeServer("IPC-Tests-CLI");
            PipeClient client = new PipeClient("IPC-Tests-CLI");

            client.PacketReceived += OnPacketRecvClient;
            server.PacketReceived += OnPacketRecvServer;

            server.Start();
            Console.WriteLine("Server started, press any key to start client");
            Console.ReadKey();
            client.Start();
            Console.WriteLine("Client started, use the following to test packets:");
            Console.WriteLine("'string', 'string2', 'int', 'test'.\nYou wull see the packet's data printed into the console!");
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
                            sp.SetData(PacketConstructor.StringToBytes(@string));
                            server.SendPacket(sp);
                        }
                        break;
                    case "string2":
                        Console.WriteLine("Enter a string to send in the FIRST packet:");
                        string stringOne = Console.ReadLine();

                        Console.WriteLine("Enter a string to send in the SECOND packet:");
                        string stringTwo = Console.ReadLine();

                        StringPacket packetOne = new StringPacket();
                        packetOne.SetData(PacketConstructor.StringToBytes(stringOne));

                        StringPacket packetTwo = new StringPacket();
                        packetTwo.SetData(PacketConstructor.StringToBytes(stringTwo));

                        server.SendPackets(new Packet[] { packetOne, packetTwo });

                        StringPacket packetThree = new StringPacket();
                        packetThree.SetData(PacketConstructor.StringToBytes("A completely unrelated string packet"));
                        server.SendPacket(packetThree);

                        break;
                    case "int":
                        Console.WriteLine("Enter an integer to send:");
                        string intStr = Console.ReadLine();
                        int @int = int.Parse(intStr);
                        using (IntPacket ip = new IntPacket())
                        {
                            ip.SetData(PacketConstructor.Int32ToBytes(@int));
                            server.SendPacket(ip);
                        }
                        break;
                }
            }

        }

        static void OnPacketRecvClient(object sender, OnPacketReceivedEventArgs eventArgs)
        {
            Packet packet = eventArgs.Packet;
            Console.WriteLine($"[CLIENT] Packet received: {packet.TotalLength} bytes");
            Console.WriteLine($"[CLIENT] Header: 0x{string.Join(", 0x", packet.Header)}");
            Console.WriteLine($"[CLIENT] Data: 0x{string.Join(", 0x", packet.Data)}");

            PacketType pt = (PacketType)packet.ID;
            if (pt == PacketType.STRING)
            {
                Console.WriteLine($"[CLIENT] String packet, got value: {((StringPacket)packet).StringValue}");
            }
            else if (pt == PacketType.INT)
            {
                Console.WriteLine($"[CLIENT] Int packet, got value: {((IntPacket)packet).NumberValue}");
            }
            else if (pt == PacketType.TEST_PACKET)
            {
                Console.WriteLine($"[CLIENT] Test packet - no output");
            }

        }

        static void OnPacketRecvServer(object sender, OnPacketReceivedEventArgs eventArgs)
        {
            Packet packet = eventArgs.Packet;
            Console.WriteLine($"[SERVER] Packet received: {packet.TotalLength} bytes");
            Console.WriteLine($"[SERVER] Header: 0x{string.Join(", 0x", packet.Header)}");
            Console.WriteLine($"[SERVER] Data: 0x{string.Join(", 0x", packet.Data)}");

            PacketType pt = (PacketType)packet.ID;
            if (pt == PacketType.STRING)
            {
                Console.WriteLine($"[SERVER] String packet, got value: {((StringPacket)packet).StringValue}");
            }
            else if (pt == PacketType.INT)
            {
                Console.WriteLine($"[SERVER] Int packet, got value: {((IntPacket)packet).NumberValue}");
            }
            else if (pt == PacketType.TEST_PACKET)
            {
                Console.WriteLine($"[SERVER] Test packet - no output");
            }

        }

    }
}
