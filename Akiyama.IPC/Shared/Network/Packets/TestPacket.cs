using System;
using System.Linq;
using System.Text;

namespace Akiyama.IPC.Shared.Network.Packets
{
    public class TestPacket : Packet
    {

        public override int ID { get; } = (int)PacketType.TEST_PACKET;

        public string Text = "This is a test packet";
        public int ANumber = 69;

        public TestPacket() : base() { }

        public override void Populate()
        {
            // Here (now at the receiving end), we can parse the data we put in on the sending side (The code in the Prepare() method).
            // This looks a little more complicated due to how we have to specify offsets and such, but once you figure it out, it's actually fairly simple

            byte[] strLenBytes = this.Data.Take(4).ToArray(); // Here we grab the first 4 bytes of our data, since we know that the first 4 bytes are the length of the following string

            int strLen = PacketConstructor.BytesToInt32(strLenBytes); // Convert the bytes into an integer

            byte[] strBytes = this.Data.Skip(4).Take(strLen).ToArray(); // Here we grab all of the bytes for our string, we use Skip(4) as we want to skip over the 4 bytes we already read for the string's length

            string str = PacketConstructor.BytesToString(strBytes, Encoding.UTF8); // Here we convert the bytes of the string into the string itself, we pass an encoding object that matches what the string was encoded with at the other side.
                                                                                   // The encoding parameter here is optional, and the method itself defaults to using Encoding.UTF8, however it's included for clarity.

            byte[] intBytes = this.Data.Skip(4 + strLen).Take(4).ToArray(); // Here we grab the bytes for our final integer, skipping over the integer for our string's length, as well as the length of the string itself

            int outInt = PacketConstructor.BytesToInt32(intBytes); // Converting the int's bytes into an actual int.

            // Then, all we need to do is assign our values to this packet's properties and we're done!
            // Now we can verify if they're the same on the recieving end.
            this.Text = str;
            this.ANumber = outInt;

        }

        public override void Prepare()
        {
            // Here (on the sending side) we put a string (along with its length to allow easy parsing at the other end, inside Populate()),
            // and a random integer, just so we can show multiple data types in one place.

            byte[] strBytes = Encoding.UTF8.GetBytes(Text); // Convert the string to its bytes (using UTF-8 in this case)


            byte[] strLenBytes = PacketConstructor.Int32ToBytes(strBytes.Length); // Here we convert the length of the array containing the string's bytes into its representative bytes.


            byte[] intBytes = PacketConstructor.Int32ToBytes(this.ANumber); // For the integer we'll use this class' property of "ANumber"


            // Here we'll be using Array.Copy() to populate our data array, but using IEnumerable.Concat() is also a valid approach (you'll just need to chain them together)

            byte[] data = new byte[strBytes.Length + intBytes.Length + strLenBytes.Length]; // Create a byte[] that is big enough to hold all of our bytes

            Array.Copy(strLenBytes, data, strLenBytes.Length); // First, write the length of the string to the array

            Array.Copy(strBytes, 0, data, 4, strBytes.Length); // Write the bytes for the string to the array

            Array.Copy(intBytes, 0, data, strBytes.Length + strLenBytes.Length, intBytes.Length); // Finally, write the integer to the array.

            this.SetData(data); // Finally, set this packet's data!

            if (this.AutomaticHeaderUpdatesDisabled) { this.UpdateHeader(); } // Ensure the header for the packet gets updated, even if this particular packet is set to NOT automaticallty update it.

        }

    }
}
