using Akiyama.IPC.Shared.Exceptions;
using Akiyama.IPC.Shared.Network.Packets;
using Akiyama.IPC.Shared.Typers;
using System.Text;

namespace Akiyama.IPC.Shared.Network
{
    public class PacketConstructor
    {

        /// <summary>
        /// See the instance of the specific <see cref="PacketTyper"/> used for this instance.
        /// </summary>
        internal readonly PacketTyper packetTyper;

        /// <summary>
        /// The byte that, when received via the IPC streams indicates the start of a data packet.
        /// </summary>
        public const byte PRE_PACKET_BYTE = 0x69;

        /// <summary>
        /// The absolute maximum number of times any one <see cref="Packet"/> may be split.
        /// <br />Packet splitting operations that would result in a number of splits greater than this number will throw a <see cref="TooManySplitsException"/>.
        /// </summary>
        public const int MAX_PACKET_SPLITS = byte.MaxValue;

        /// <summary>
        /// The minimum allowed packet version that this <see cref="PacketConstructor"/> will accept.
        /// <br />Packets with a lower version than this will throw an <see cref="IncorrectPacketVersionException"/> during construction.
        /// </summary>
        public static readonly Version MINIMUM_PACKET_VERSION = new Version(1, 1, 0);

        /// <summary>
        /// Creates a new instance of <see cref="PacketConstructor"/> to use in an <see cref="IPCEndpoint"/>.
        /// </summary>
        /// <param name="packetTyper">An instance of a <see cref="PacketTyper"/> to use in this constructor.</param>
        public PacketConstructor(PacketTyper packetTyper)
        {
            this.packetTyper = packetTyper;
        }

        /// <summary>
        /// Creates an <see cref="Packet"/> from <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream from which to read the bytes for this <see cref="Packet"/>.</param>
        /// <returns>A <see cref="Packet"/> constructed from the bytes contained within <paramref name="stream"/>.</returns>
        public Packet CreateFromStream(Stream stream)
        {
            if (stream.CanSeek && stream.Position != 0) { stream.Seek(0, SeekOrigin.Begin); }
            byte[] idBytes = new byte[4];
            stream.Read(idBytes, 0, idBytes.Length);
            byte[] dataLen = new byte[4];
            stream.Read(dataLen, 0, dataLen.Length);
            byte[] versionBytes = new byte[3];
            stream.Read(versionBytes, 0, versionBytes.Length);
            Version packetVersion = new Version(versionBytes[0], versionBytes[1], versionBytes[2]);
            if (packetVersion < MINIMUM_PACKET_VERSION)
            {
                
                throw new IncorrectPacketVersionException(packetVersion);
            }

            int id = BytesToInt32(idBytes);
            int dataLength = BytesToInt32(dataLen);
            Packet packet = this.packetTyper.GetPacketObjectFromId(id) ?? throw new UnknownPacketException(id);

            // Setting values while still reading the stream lmao ———————————————————————

            // Backwards compatibility baybee
            if (versionBytes[0] >= 1 && versionBytes[1] >= 2) // Packet version >= 1.2.0
            {
                bool splitPacketIndicator = stream.ReadByte() == 1;
                _ = stream.ReadByte(); // Skip currently unused header byte
                packet.SetIsSplit(splitPacketIndicator);
            }

            byte[] customData = new byte[Packet.CUSTOM_HEADER_BYTES];
            stream.Read(customData, 0, customData.Length);

            // ———————————————— END STREAM READING ————————————————

            packet.SetCustomHeaderBytes(customData, 0); // BF 29/02/2024: Fix packets losing their custom data after transmission over the socket

            byte[] pData = new byte[dataLength];
            stream.Read(pData, 0, pData.Length);
            packet.SetAutomaticHeaderUpdates(false);

            packet.SetPayload(pData);

            packet.SetAutomaticHeaderUpdates(true);
            packet.UpdateHeader();

            if (!packet.IsSplit) { packet.Populate(); } // v1.2 - 07/03/24 -- Don't run Populate() on split packets

            return packet;
        }

        /// <summary>
        /// Converts <paramref name="bytes"/> into a 32-bit integer.<br />Throws <see cref="InvalidOperationException"/> if <paramref name="bytes"/> is not a 4 byte array.
        /// </summary>
        /// <param name="bytes">The bytes to convert</param>
        /// <returns>An integer created from <paramref name="bytes"/>.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static int BytesToInt32(byte[] bytes)
        {
            if (bytes.Length != 4) { throw new InvalidOperationException("Specified byte[] is not an integer"); }
            if (!BitConverter.IsLittleEndian) { bytes.Reverse(); }
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// Converts the specified integer into its representative bytes.
        /// </summary>
        /// <param name="int">The int to convert.</param>
        /// <returns>A byte array containing the bytes required to construct <paramref name="int"/>.</returns>
        public static byte[] Int32ToBytes(int @int)
        {
            byte[] bytes = BitConverter.GetBytes(@int);
            if (!BitConverter.IsLittleEndian) { bytes.Reverse(); }
            return bytes;
        }

        /// <summary>
        /// Convert the given <see cref="UInt32"/> into its representatives bytes.
        /// </summary>
        /// <param name="uint">The UInt32 to convert</param>
        /// <returns>A byte array containing the bytes that represent <paramref name="uint"/>.</returns>
        public static byte[] UInt32ToBytes(uint @uint) => Int32ToBytes((int)@uint);
        /// <summary>
        /// Turns the given <paramref name="bytes"/> into the <see cref="UInt32"/> they represent.
        /// </summary>
        /// <param name="bytes">The bytes to convert.</param>
        /// <returns>A <see cref="UInt32"/> constructive from its representive bytes in <paramref name="bytes"/>.</returns>
        public static uint BytesToUInt32(byte[] bytes) => (uint)BytesToInt32(bytes);

        /// <inheritdoc cref="Int64ToBytes(long)"/>
        public static byte[] LongToBytes(long @long) => Int64ToBytes(@long);
        /// <summary>
        /// Converts <paramref name="long"/> into its representative bytes.
        /// </summary>
        /// <param name="long">The <see cref="long"/> to convert.</param>
        /// <returns>A byte array containing the bytes required to construct <paramref name="long"/>.</returns>
        public static byte[] Int64ToBytes(long @long)
        {
            byte[] bytes = BitConverter.GetBytes(@long);
            if (!BitConverter.IsLittleEndian) { bytes.Reverse(); }
            return bytes;
        }

        /// <inheritdoc cref="BytesToInt64(byte[])"/>
        public static long BytesToLong(byte[] bytes) => BytesToInt64(bytes);
        /// <summary>
        /// Converts an array of 8 bytes into its <see cref="long"/> representation.<br />Throws <see cref="InvalidOperationException"/> if <paramref name="bytes"/> is not an 8 byte array.
        /// </summary>
        /// <param name="bytes">The bytes to convert.</param>
        /// <returns>A long constructed from the bytes in <paramref name="bytes"/>.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static long BytesToInt64(byte[] bytes)
        {
            if (bytes.Length != 8) { throw new InvalidOperationException("Specified byte[] is not a long"); }
            if (!BitConverter.IsLittleEndian) { bytes.Reverse(); }
            return BitConverter.ToInt64(bytes, 0);
        }

        /// <summary>
        /// Converts <paramref name="ulong"/> into its representative bytes.
        /// </summary>
        /// <param name="ulong">The <see cref="UInt64"/> to convert.</param>
        /// <returns>A byte array containing the bytes that represent <paramref name="ulong"/>.</returns>
        public static byte[] ULongToBytes(ulong @ulong) => UInt64ToBytes(@ulong);
        /// <inheritdoc cref="ULongToBytes(ulong)"/>
        public static byte[] UInt64ToBytes(ulong @ulong) => Int64ToBytes((long)@ulong);
        /// <summary>
        /// Converts the given <paramref name="bytes"/> into the <see cref="UInt64"/> they represent.
        /// </summary>
        /// <param name="bytes">The bytes to convert.</param>
        /// <returns>A <see cref="UInt64"/> constructed from the bytes in <paramref name="bytes"/>.</returns>
        public static ulong BytesToULong(byte[] bytes) => BytesToUInt64(bytes);
        /// <inheritdoc cref="BytesToULong(byte[])"/>
        public static ulong BytesToUInt64(byte[] bytes) => (ulong)BytesToInt64(bytes);

        /// <summary>
        /// Converts <paramref name="short"/> into its representative bytes.
        /// </summary>
        /// <param name="short">The <see cref="UInt16"/> to convert.</param>
        /// <returns>A byte array containing the bytes that represent <paramref name="short"/>.</returns>
        public static byte[] ShortToBytes(short @short) => Int16ToBytes(@short);
        /// <inheritdoc cref="ShortToBytes(short)"/>
        public static byte[] Int16ToBytes(short @short)
        {
            byte[] bytes = BitConverter.GetBytes(@short);
            if (!BitConverter.IsLittleEndian) { bytes.Reverse(); }
            return bytes;
        }
        /// <summary>
        /// Converts the given <paramref name="bytes"/> into the <see cref="Int16"/> they represent.
        /// </summary>
        /// <param name="bytes">The bytes to convert.</param>
        /// <returns>A <see cref="Int16"/> constructed from the bytes in <paramref name="bytes"/>.</returns>
        public static short BytesToShort(byte[] bytes) => BytesToInt16(bytes);
        /// <inheritdoc cref="BytesToShort(byte[])"/>
        public static short BytesToInt16(byte[] bytes)
        {
            if (!BitConverter.IsLittleEndian) { bytes.Reverse(); }
            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Converts <paramref name="bytes"/> into the UTF-16 <see cref="char"/> they represent.
        /// </summary>
        /// <param name="bytes">The bytes to convert.</param>
        /// <returns>A <see cref="char"/> constructed from the bytes in <paramref name="bytes"/>.</returns>
        public static char BytesToChar(byte[] bytes) => (char)BytesToInt16(bytes);
        /// <summary>
        /// Converts a UTF-16 character into its representative bytes.
        /// </summary>
        /// <param name="char">The <see cref="char"/> to convert.</param>
        /// <returns>A byte array containing the bytes that represent <paramref name="char"/>.</returns>
        public static byte[] CharToBytes(char @char) => Int16ToBytes((short)@char);

        /// <summary>
        /// Converts <paramref name="ushort"/> into its representative bytes.
        /// </summary>
        /// <param name="ushort">The <see cref="UInt16"/> to convert.</param>
        /// <returns>A byte array containing the bytes that represent <paramref name="ushort"/>.</returns>
        public static byte[] UShortToBytes(ushort @ushort) => UInt16ToBytes(@ushort);
        /// <inheritdoc cref="UShortToBytes(ushort)"/>
        public static byte[] UInt16ToBytes(ushort @ushort) => Int16ToBytes((short)@ushort);
        /// <summary>
        /// Converts the given <paramref name="bytes"/> into the <see cref="UInt16"/> they represent.
        /// </summary>
        /// <param name="bytes">The bytes to convert.</param>
        /// <returns>A <see cref="UInt16"/> constructed from the bytes in <paramref name="bytes"/>.</returns>
        public static ushort BytesToUShort(byte[] bytes) => (ushort)BytesToInt16(bytes);
        /// <inheritdoc cref="BytesToUShort(byte[])"/>
        public static ushort BytesToUInt16(byte[] bytes) => (ushort)BytesToInt16(bytes);

        /// <summary>
        /// Converts <paramref name="float"/> into its representative bytes.
        /// </summary>
        /// <param name="float">The <see cref="Single"/> to convert.</param>
        /// <returns>A byte array containing the bytes that represent <paramref name="float"/>.</returns>
        public static byte[] SingleToBytes(float @float) => FloatToBytes(@float);
        /// <inheritdoc cref="SingleToBytes(float)"/>
        public static byte[] FloatToBytes(float @float)
        {
            byte[] bytes = BitConverter.GetBytes(@float);
            if (!BitConverter.IsLittleEndian) { bytes.Reverse(); }
            return bytes;
        }
        /// <summary>
        /// Converts the given <paramref name="bytes"/> into the <see cref="Single"/> they represent.
        /// </summary>
        /// <param name="bytes">The bytes to convert.</param>
        /// <returns>A <see cref="Single"/> constructed from the bytes in <paramref name="bytes"/>.</returns>
        public static float BytesToSingle(byte[] bytes) => BytesToFloat(bytes);
        /// <inheritdoc cref="BytesToSingle(byte[])"/>
        public static float BytesToFloat(byte[] bytes)
        {
            if (!BitConverter.IsLittleEndian) { bytes.Reverse(); }
            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        /// Converts <paramref name="double"/> into its representative bytes.
        /// </summary>
        /// <param name="double">The <see cref="Double"/> to convert.</param>
        /// <returns>A byte array containing the bytes that represent <paramref name="double"/>.</returns>
        public static byte[] DoubleToBytes(double @double)
        {
            byte[] bytes = BitConverter.GetBytes(@double);
            if (!BitConverter.IsLittleEndian) { bytes.Reverse(); }
            return bytes;
        }
        /// <summary>
        /// Converts the given <paramref name="bytes"/> into the <see cref="Double"/> they represent.
        /// </summary>
        /// <param name="bytes">The bytes to convert.</param>
        /// <returns>A <see cref="Double"/> constructed from the bytes in <paramref name="bytes"/>.</returns>
        public static double BytesToDouble(byte[] bytes)
        {
            if (!BitConverter.IsLittleEndian) { bytes.Reverse(); }
            return BitConverter.ToDouble(bytes, 0);
        }

        /// <summary>
        /// Converts the given <paramref name="bool"/> into its representative byte.
        /// </summary>
        /// <param name="bool">The <see cref="Boolean"/> to convert.</param>
        /// <returns>A byte array consisting of the byte used to represent <paramref name="bool"/>.</returns>
        public static byte[] BoolToBytes(bool @bool)
        {
            return new byte[] { (byte)(@bool ? 1 : 0) };
        }
        /// <summary>
        /// Converts the given <paramref name="bool"/> into its representative byte.
        /// </summary>
        /// <param name="bool">The <see cref="Boolean"/> to convert.</param>
        /// <returns>A byte that represents the state of <paramref name="bool"/>.</returns>
        public static byte BoolToByte(bool @bool) => BoolToBytes(@bool)[0];
        /// <summary>
        /// Converts the given <paramref name="bytes"/> into the <see cref="Boolean"/> it represents. Any value other than 0 is considered to be <see langword="true"/>.
        /// <br />Throws <see cref="ArgumentOutOfRangeException"/> if <paramref name="offset"/> is greater than the size of <paramref name="bytes"/>, or if <paramref name="offset"/> is less than 0.
        /// </summary>
        /// <param name="bytes">The bytes to convert.</param>
        /// <param name="offset">The offset for which byte to convert.</param>
        /// <returns>A <see cref="Boolean"/> constructed from the byte in <paramref name="bytes"/> at the offset <paramref name="offset"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static bool BytesToBool(byte[] bytes, int offset = 0)
        {
            if (offset < 0 || bytes.Length < (offset + 1)) { throw new ArgumentOutOfRangeException("offset"); }
            return bytes[offset] != 0;
        }
        /// <summary>
        /// Converts the given <paramref name="byte"/> into the <see cref="Boolean"/> it represents. Any value other than 0 is considered to be <see langword="true"/>.
        /// </summary>
        /// <param name="byte">The byte to convert.</param>
        /// <returns>A <see cref="Boolean"/> constructed from the byte <paramref name="byte"/>.</returns>
        public static bool ByteToBool(byte @byte)
        {
            return @byte != 0;
        }

        /// <summary>
        /// Returns the bytes that represent the given string. This method uses UTF-8 encoding by default. To specify encoding, see <see cref="StringToBytes(string, Encoding)"/>.
        /// </summary>
        /// <param name="string">The string to convert</param>
        /// <returns>A byte array containing the bytes that make up <paramref name="string"/>.</returns>
        public static byte[] StringToBytes(string @string) => StringToBytes(@string, Encoding.UTF8);
        /// <summary>
        /// Returns the bytes that represent the given string.
        /// </summary>
        /// <param name="string">The string to convert</param>
        /// <param name="encoding">The encoding to use when encoding the string.</param>
        /// <returns>A byte array containing the bytes that make up <paramref name="string"/>. Encoded using <paramref name="encoding"/>.</returns>
        public static byte[] StringToBytes(string @string, Encoding encoding)
        {
            if (encoding != null)
            {
                return encoding.GetBytes(@string);
            }
            return Encoding.UTF8.GetBytes(@string);
        }

        /// <summary>
        /// Takes an array of bytes and returns the string representation. This method uses UTF-8 encoding by default. To specify encoding, see <see cref="BytesToString(byte[], Encoding)"/>.
        /// </summary>
        /// <param name="bytes">The bytes to convert.</param>
        /// <returns>A string created from the bytes contained with <paramref name="bytes"/>.</returns>
        public static string BytesToString(byte[] bytes) => BytesToString(bytes, Encoding.UTF8);
        /// <summary>
        /// Takes an array of bytes and returns the string representation.
        /// </summary>
        /// <param name="bytes">The bytes to convert.</param>
        /// <param name="encoding">The encoding to use for conversion.</param>
        /// <returns>A string created from the bytes contained with <paramref name="bytes"/>. Encoded using <paramref name="encoding"/>.</returns>
        public static string BytesToString(byte[] bytes, Encoding encoding)
        {
            return encoding.GetString(bytes);
        }

        /// <summary>
        /// Splits <paramref name="packet"/> into enough packets of its type given <paramref name="lengthLimit"/>.
        /// <br />If <paramref name="lengthLimit"/> is equal to or greater than the current payload length of <paramref name="packet"/>, a list containing the unmodified will be returned.
        /// <br />Throws <see cref="TooManySplitsException"/> is the resulting split would result in &gt;256 packets.
        /// <br /><br /><b>WARNING</b>: This method may be destructive to previously existing Custom Header Bytes. This method assigns the packet's current index into the split, and the total number of splits to the first two bytes of Custom Header Bytes respectively.
        /// <br /><br /><b>WARNING</b>: The initial packet given to this method will automatically be disposed of after it has been split. If you need to retain the initial packet, consider using <see cref="SplitPacket(Packet, int, out Packet)"/> instead.
        /// <br /><br /><b>NOTE</b>: Packet split indexes are zero-indexed, ie. the FIRST packet in the split will have a split index of 0.
        /// </summary>
        /// <param name="packet">The packet to split</param>
        /// <param name="lengthLimit">The length size at which to split the packet.</param>
        /// <returns>A typed list containing packets derived from <paramref name="packet"/> with their payload split at <paramref name="lengthLimit"/> intervals.</returns>
        /// <exception cref="TooManySplitsException"></exception>
        public static List<Packet> SplitPacket(Packet packet, int lengthLimit)
        {
            return SplitPacketInternal(packet, lengthLimit, true);
        }
        /// <summary>
        /// Splits <paramref name="packet"/> into enough packets of its type given <paramref name="lengthLimit"/>.
        /// <br />If <paramref name="lengthLimit"/> is equal to or greater than the current payload length of <paramref name="packet"/>, a list containing the unmodified will be returned.
        /// <br />Throws <see cref="TooManySplitsException"/> is the resulting split would result in &gt;256 packets.
        /// <br /><br /><b>WARNING</b>: This method may be destructive to previously existing Custom Header Bytes. This method assigns the packet's current index into the split, and the total number of splits to the first two bytes of Custom Header Bytes respectively.
        /// <br /><br /><b>NOTE</b>: Packet split indexes are zero-indexed, ie. the FIRST packet in the split will have a split index of 0.
        /// </summary>
        /// <param name="packet">The packet to split</param>
        /// <param name="lengthLimit">The length size at which to split the packet.</param>
        /// <param name="originalPacket">The packet that was originally passed to this method</param>
        /// <returns>A typed list containing packets derived from <paramref name="packet"/> with their payload split at <paramref name="lengthLimit"/> intervals.</returns>
        /// <exception cref="TooManySplitsException"></exception>
        public static List<Packet>SplitPacket(Packet packet, int lengthLimit, out Packet originalPacket)
        {
            originalPacket = packet;
            return SplitPacketInternal(packet, lengthLimit, false);
        }
        /// <summary>
        /// Splits <paramref name="packet"/> into enough packets of its type given <paramref name="lengthLimit"/>.
        /// <br />If <paramref name="lengthLimit"/> is equal to or greater than the current payload length of <paramref name="packet"/>, a list containing the unmodified will be returned.
        /// <br />Throws <see cref="TooManySplitsException"/> is the resulting split would result in &gt;256 packets.
        /// <br /><br /><b>WARNING</b>: This method may be destructive to previously existing Custom Header Bytes. This method assigns the packet's current index into the split, and the total number of splits to the first two bytes of Custom Header Bytes respectively.
        /// <br /><br /><b>NOTE</b>: Packet split indexes are zero-indexed, ie. the FIRST packet in the split will have a split index of 0.
        /// </summary>
        /// <param name="packet">The packet to split</param>
        /// <param name="lengthLimit">The length size at which to split the packet.</param>
        /// <param name="dispose">If <see langword="true"/>, <paramref name="packet"/> will be disposed of once splitting is complete.</param>
        /// <returns>A typed list containing packets derived from <paramref name="packet"/> with their payload split at <paramref name="lengthLimit"/> intervals.</returns>
        /// <exception cref="TooManySplitsException"></exception>
        /// <exclude/>
        private static List<Packet> SplitPacketInternal(Packet packet, int lengthLimit, bool dispose = true)
        {
            if (lengthLimit >= packet.PayloadLength)
            {
                return new List<Packet> { packet };
            }
            int maxSplits = ((int)Math.Ceiling((double)((double)packet.PayloadLength / (double)lengthLimit) - 1d));
            if (maxSplits > MAX_PACKET_SPLITS) { throw new TooManySplitsException(lengthLimit, maxSplits); }
            Type pType = packet.GetType();
            List<Packet> @out = new List<Packet>();
            for (int x = 0; x <= maxSplits; x++)
            {
                Packet splitPacket = (Packet)Activator.CreateInstance(pType);
                splitPacket.SetMaxLength(lengthLimit);
                int offset = (x * lengthLimit);
                byte[] bytes = packet.Payload.Skip(offset).Take(lengthLimit).ToArray();
                splitPacket.SetIsSplit(true);
                splitPacket.SetPayload(bytes);
                splitPacket.SetCustomHeaderBytes(new byte[] { (byte)x, (byte)maxSplits }, 0);
                @out.Add(splitPacket);
            }
            if (dispose) { packet.Dispose(); }
            return @out;
        }

    }
}
