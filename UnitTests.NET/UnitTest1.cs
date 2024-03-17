using Akiyama.IPC.Shared.Exceptions;
using Akiyama.IPC.Shared.Network;
using Akiyama.IPC.Shared.Network.Packets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.NET
{
    [TestClass]
    public class UnitTest1
    {
        private TestContext? _testContext;
        public TestContext TestContext
        {
            get { return this._testContext; }
            set { this._testContext = value; }
        }


        [TestMethod]
        public void TestPacketRangeWriteFullRange()
        {
            byte[] values = new byte[Packet.CUSTOM_HEADER_BYTES];
            for (int x = 0; x < values.Length; x++)
            {
                values[x] = 255;
            }
            TestPacket tp = new TestPacket();
            tp.SetCustomHeaderBytes(values, 0);
            TestContext.WriteLine($"0x{string.Join(", 0x", tp.Header)}");
        }


        [TestMethod]
        public void TestPacketSingleHeaderCustomDataClobberFix()
        {
            Random rand = new Random();
            int index = rand.Next(11);
            Packet test = new TestPacket();
            byte _byte = (byte)rand.Next(256);
            TestContext.WriteLine($"Byte {_byte}, written at index {index}");
            test.SetCustomHeaderByte(_byte, index);
            TestContext.WriteLine($"0x{string.Join(", 0x", test.Header)}");
            test.SetPayload(PacketConstructor.StringToBytes("test"));
            byte r = test.GetCustomHeaderByte(index);
            TestContext.WriteLine($"Recv: 0x{r}");
            TestContext.WriteLine($"0x{string.Join(", 0x", test.Header)}");
            Assert.AreEqual(_byte, r);

        }

        [TestMethod]
        public void TestPacketSingleHeaderAssignInRange()
        {
            Random rand = new Random();
            int index = rand.Next(11);
            Packet test = new TestPacket();
            byte _byte = (byte)rand.Next(256);
            TestContext.WriteLine($"Byte {_byte}, written at index {index}");
            test.SetCustomHeaderByte(_byte, index);
            TestContext.WriteLine($"0x{string.Join(", 0x", test.Header)}");
            // No assertion needed, an exception will fail the test.

        }

        [TestMethod]
        public void TestPacketSingleHeaderAssignOutOfRange()
        {

            Packet test = new TestPacket();
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { test.SetCustomHeaderByte(255, 13); }); // index >= 12 = fail

        }

        [TestMethod]
        public void TestPacketSingleHeaderReadInRange()
        {
            Random rand = new Random();
            int index = rand.Next(11);
            Packet test = new TestPacket();
            byte _byte = (byte)rand.Next(256);
            TestContext.WriteLine($"Byte {_byte}, written at index {index}");
            test.SetCustomHeaderByte(_byte, index);
            TestContext.WriteLine($"0x{string.Join(", 0x", test.Header)}");
            byte r = test.GetCustomHeaderByte(index);
            TestContext.WriteLine($"Recv: 0x{r}");
            // No assertion needed, an exception will fail the test.

        }

        [TestMethod]
        public void TestPacketSingleHeaderReadOutOfRange()
        {
            Packet test = new TestPacket();
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => { test.GetCustomHeaderByte(12); }); // index >= 12 = fail
            // No assertion needed, an exception will fail the test.

        }

        [TestMethod]
        public void TestPacketBufferHeaderAssignInRange()
        {
            byte[] testBytes = new byte[6] { 0x0, 0x1, 0x3, 0x3, 0x7, 0x0 };
            Packet test = new TestPacket();
            Random rand = new Random();
            int index = rand.Next(6);
            TestContext.WriteLine($"Bytes written at index {index}");
            test.SetCustomHeaderBytes(testBytes, index);
            TestContext.WriteLine($"0x{string.Join(", 0x", test.Header)}");
            // No assertion, thrown exceptions auto fail
        }

        [TestMethod]
        public void TestPacketBufferHeaderAssignOutOfRange()
        {
            byte[] testBytes = new byte[6] { 0x0, 0x1, 0x3, 0x3, 0x7, 0x0 };
            Packet test = new TestPacket();
            Assert.ThrowsException<ArgumentException>(() => { test.SetCustomHeaderBytes(testBytes, 7); }); // anything above and index of 6 here should throw an exception
        }

        [TestMethod]
        public void TestPacketBufferHeaderReadInRange()
        {
            byte[] testBytes = new byte[6] { 0x0, 0x1, 0x3, 0x3, 0x7, 0x0 };
            Packet test = new TestPacket();
            Random rand = new Random();
            int index = rand.Next(6);
            TestContext.WriteLine($"Bytes written at index {index}");
            test.SetCustomHeaderBytes(testBytes, index);
            TestContext.WriteLine($"0x{string.Join(", 0x", test.Header)}");
            byte[] bytes = new byte[testBytes.Length];
            test.GetCustomHeaderBytes(bytes, index);
            TestContext.WriteLine($"Recv: 0x{string.Join(", 0x", bytes)}");
            // No assertion, thrown exceptions auto fail
        }

        [TestMethod]
        public void TestPacketBufferHeaderReadInRangeAlt()
        {
            byte[] testBytes = new byte[6] { 0x0, 0x1, 0x3, 0x3, 0x7, 0x0 };
            Packet test = new TestPacket();
            Random rand = new Random();
            int index = rand.Next(6);
            TestContext.WriteLine($"Bytes written at index {index}");
            test.SetCustomHeaderBytes(testBytes, index);
            TestContext.WriteLine($"0x{string.Join(", 0x", test.Header)}");
            byte[] bytes = test.GetCustomHeaderBytes(index, testBytes.Length);
            TestContext.WriteLine($"Recv: 0x{string.Join(", 0x", bytes)}");
            // No assertion, thrown exceptions auto fail
        }

        [TestMethod]
        public void TestPacketBufferHeaderReadOutOfRange()
        {
            byte[] testBytes = new byte[6] { 0x0, 0x1, 0x3, 0x3, 0x7, 0x0 };
            Packet test = new TestPacket();
            byte[] t = new byte[testBytes.Length];
            Assert.ThrowsException<ArgumentException>(() => { test.GetCustomHeaderBytes(t, 7); }); // anything above and index of 6 here should throw an exception
        }

        [TestMethod]
        public void TestPacketBufferHeaderReadOutOfRangeAlt()
        {
            byte[] testBytes = new byte[6] { 0x0, 0x1, 0x3, 0x3, 0x7, 0x0 };
            Packet test = new TestPacket();
            Assert.ThrowsException<ArgumentException>(() => { byte[] t = test.GetCustomHeaderBytes(7, testBytes.Length); }); // anything above and index of 6 here should throw an exception
        }

        [TestMethod]
        public void PacketPayloadAppend()
        {
            byte[] iP = new byte[] { 255, 255 };
            byte[] append = new byte[] { 1, 2 };
            using (TestPacket tp = new TestPacket())
            {
                tp.SetPayload(iP);
                TestContext.WriteLine($"Before: 0x{string.Join(", 0x", tp.Payload)}");
                tp.AppendPayload(append);
                TestContext.WriteLine($"After: 0x{string.Join(", 0x", tp.Payload)}");
                Assert.AreEqual("255 255 1 2", string.Join(" ", tp.Payload));
            }
        }

        [TestMethod]
        public void PacketSplitTooMany()
        {
            // Passes if the library correctly raises a TooManySplitsException when trying to split a packet more than 256 times
            byte[] testPayload = new byte[513];
            using (GenericDataPacket tp = new GenericDataPacket())
            {
                tp.SetPayload(testPayload);
                Assert.ThrowsException<TooManySplitsException>(() => { List<Packet> list = PacketConstructor.SplitPacket(tp, 2); }); // A split of 2 against a packet payload of 513 bytes, should result in 257 packets, which is not allowed
            }
        }

        [TestMethod]
        public void PacketSplitMaxSplits()
        {
            // Passes if the library correctly raises a TooManySplitsException when trying to split a packet more than 256 times
            byte[] testPayload = new byte[510];
            using (GenericDataPacket tp = new GenericDataPacket())
            {
                tp.SetPayload(testPayload.Concat(new byte[] { 69, 69 }).ToArray());
                List<Packet> list = PacketConstructor.SplitPacket(tp, 2);
                Assert.AreEqual(PacketConstructor.MAX_PACKET_SPLITS, (list.Count - 1)); // We - 1 on count here because PacketConstructor.MAX_PACKET_SPLITS
                                                                                        // is on the assumption that 1 == 0, 2 == 1, etc. thus meaning that 255 == 256.
            }
        }

        [TestMethod]
        public void PacketSplitTen()
        {
            // Passes if the library correctly raises a TooManySplitsException when trying to split a packet more than 256 times
            using (StringPacket tp = new StringPacket())
            {
                tp.Text = "11223344556677889900";
                List<Packet> list = PacketConstructor.SplitPacket(tp, 2);
                Assert.AreEqual(10, list.Count);
            }
        }

        [TestMethod]
        public void PacketSplitPayloadTest()
        {
            byte[] payloadOne = new byte[] { 1, 2, 3, 4, 5 };
            byte[] payloadTwo = new byte[] { 6, 7, 8, 9, 10 };

            using (GenericDataPacket tp = new GenericDataPacket())
            {
                tp.SetPayload(payloadOne.Concat(payloadTwo).ToArray());
                List<Packet> list = PacketConstructor.SplitPacket(tp, 5);
                Packet packetOne = list.First();
                Packet packetTwo = list.Last();
                TestContext.WriteLine($"Payload One: 0x{string.Join(", 0x", packetOne.Payload)}");
                Assert.AreEqual("1 2 3 4 5", string.Join(" ", packetOne.Payload));

                TestContext.WriteLine($"Payload Two: 0x{string.Join(", 0x", packetTwo.Payload)}");
                Assert.AreEqual("6 7 8 9 10", string.Join(" ", packetTwo.Payload));
            }

        }

    }
}
