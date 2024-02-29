using Akiyama.IPC.Shared.Exceptions;
using Akiyama.IPC.Shared.Network.Packets;
using Akiyama.IPC.Shared.Typers;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Akiyama.IPC.Shared.Network
{
    public class PacketConstructor
    {

        PacketTyper packetTyper;
        internal byte PRE_PACKET_BYTE = 0x69;

        public PacketConstructor(PacketTyper packetTyper)
        {
            this.packetTyper = packetTyper;
        }

        /// <summary>
        /// Creates an <see cref="Packet"/> from <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream from which to read the bytes for this <see cref="Packet"/>.</param>
        /// <returns>An <see cref="Packet"/> constructed fro mthe bytes contained within <paramref name="stream"/>.</returns>
        public Packet CreateFromStream(Stream stream)
        {
            if (stream.CanSeek && stream.Position != 0) { stream.Seek(0, SeekOrigin.Begin); }
            byte[] idBytes = new byte[4];
            stream.Read(idBytes, 0, idBytes.Length);
            byte[] _ = new byte[Packet.MAX_HEADER_SIZE - (sizeof(int) * 2)]; // Skip over the "customisable" bytes of the header
            stream.Read(_, 0, _.Length);
            byte[] dataLen = new byte[4];
            stream.Read(dataLen, 0, dataLen.Length);

            int id = BytesToInt32(idBytes);
            int dataLength = BytesToInt32(dataLen);
            Packet packet = this.packetTyper.GetPacketObjectFromId(id) ?? throw new UnknownPacketException(id);
            byte[] pData = new byte[dataLength];
            stream.Read(pData, 0, pData.Length);
            packet.SetAutomaticHeaderUpdates(false);

            packet.SetData(pData);

            packet.SetAutomaticHeaderUpdates(true);
            packet.UpdateHeader();

            packet.Populate();

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
        /// Converts the specified integer into its representitive bytes.
        /// </summary>
        /// <param name="int">The int to convert.</param>
        /// <returns>A byte array containing the bytes required to construct <paramref name="int"/>.</returns>
        public static byte[] Int32ToBytes(int @int)
        {
            byte[] bytes = BitConverter.GetBytes(@int);
            if (!BitConverter.IsLittleEndian) { bytes.Reverse(); }
            return bytes;
        }

        /// <inheritdoc cref="Int64ToBytes(long)"/>
        public static byte[] LongToBytes(long @long) => Int64ToBytes(@long);
        /// <summary>
        /// Converts <paramref name="long"/> into its representitive bytes.
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

    }
}
