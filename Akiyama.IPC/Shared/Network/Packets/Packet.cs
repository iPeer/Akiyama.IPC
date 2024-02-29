using System;
using System.Linq;

namespace Akiyama.IPC.Shared.Network.Packets
{

    /// <summary>
    /// Represents the base class for IPC packets. This class is <see langword="abstract"/>.
    /// </summary>
    public abstract class Packet : IDisposable
    {

        private bool _disposed;

        public static readonly int MAX_HEADER_SIZE = 20;

        public abstract int ID { get; }

        public byte[] Data { get; private set; } = Array.Empty<byte>(); // BF-08-02-2024: Prevent Data from defaulting to null in packets that don't pass any data
        public byte[] Header { get; private set; } = new byte[MAX_HEADER_SIZE];
        public int HeaderLength => this.Header.Length;
        public int DataLength => this.Data.Length;
        public int TotalLength => this.Data.Length + this.Header.Length;
        public bool AutomaticHeaderUpdatesDisabled { get; private set; }

        public bool AutoDispose { get; private set; } = true;

        public int MaxDataLength { get; private set; } = int.MaxValue;

        public Packet(byte[] data, int maxDataLength = int.MaxValue, bool autoDispose = true)
        {
            this.MaxDataLength = maxDataLength;
            this.AutoDispose = autoDispose;
            this.SetData(data);
        }
        public Packet(int maxDataLength = int.MaxValue, bool autoDispose = true)
        {
            this.MaxDataLength = maxDataLength;
            this.AutoDispose = autoDispose;
            this.UpdateHeader(); // BF-28-02-2024: Ensure headers are updated on new packets to prevent their values being nulls
        }

        /// <summary>
        /// When overridden, allows the packet to populate its own properties from its data, for reference later.<br />
        /// For example, see <see cref="StringPacket.Populate"/>.
        /// <br />If not overridden, the packet will not be populated, however its data will still be available via <see cref="Packet.Data"/>.
        /// </summary>
        public virtual void Populate() { }

        /// <summary>
        /// Sets the data for this packet.
        /// <br/><b>Note</b>: While there is no specifically coded length limit, the packet header uses 4 bytes for Data length. Thus the length of the data is limited to <see cref="int.MaxValue"/>.
        /// <br />Also note that larger packets will take longer to send, take longer to be read, and will use more RAM (on both ends) while being processed. Consider splitting larger packets to make them more efficient.
        /// </summary>
        /// <param name="data"></param>
        public void SetData(byte[] data)
        {
            if (data.Length > this.MaxDataLength)
            {
                throw new OverflowException($"Payload is too big for this packet's configuration. Max length is {this.MaxDataLength}, but given data was {data.Length}.");
            }
            this.Data = data;
            if (!this.AutomaticHeaderUpdatesDisabled) { this.UpdateHeader(); }
        }

        private void SetHeader(byte[] header)
        {
            this.Header = header;
        }

        // TODO: docstring
        public void SetCustomHeaderByte(byte value, int offset)
        {
            int realOffset = sizeof(int) + offset;
            if (offset < 0 || realOffset >= MAX_HEADER_SIZE - 4) { throw new ArgumentOutOfRangeException("offset"); }
            this.Header[realOffset] = value;
        }

        // TODO: docstring
        public void SetCustomHeaderBytes(byte[] data, int offset)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            if (data.Length == 0) { return; }
            int realOffset = sizeof(int) + offset;
            if (offset < 0 || realOffset + (data.Length - 1) >= MAX_HEADER_SIZE - 4) { throw new ArgumentException("Supplied data falls outside of available space.", "data"); }
            Array.Copy(data, 0, this.Header, realOffset, data.Length);
        }

        // TODO: docstring
        public byte GetCustomHeaderByte(int offset)
        {
            int realOffset = offset + 4;
            if (offset < 0 || realOffset >= MAX_HEADER_SIZE - 4) { throw new ArgumentOutOfRangeException("offset"); }
            return this.Header[realOffset];
        }

        // TODO: docstring
        public byte[] GetCustomHeaderBytes(int offset, int length)
        {
            byte[] buffer = new byte[length];
            this.GetCustomHeaderBytes(buffer, offset);
            return buffer;
        }

        // TODO: docstring
        public void GetCustomHeaderBytes(byte[] buffer, int offset)
        {
            if (buffer == null) { throw new ArgumentNullException("buffer"); }
            if (buffer.Length == 0) { throw new InvalidOperationException("Cannot read into 0-length buffer"); }
            int realOffset = offset + 4;
            if (offset < 0 || realOffset + (buffer.Length - 1) >= MAX_HEADER_SIZE - 4) { throw new ArgumentException("Supplied buffer falls outside of available space.", "buffer"); }
            Array.Copy(this.Header, realOffset, buffer, 0, buffer.Length);
        }

        public void UpdateHeader()
        {
            /*
             * CURRENT HEADER STRUCTURE:
             * [Bytes 0-3]: int32 indicating the TYPE of this packet
             * [Bytes 4-15]: Customisable header data that the user can set to convey additional data
             * [Bytes 16-19]: int32 indicating the LENGTH of the DATA in this packet
             */
            int _dLen = this.Data.Length;
            byte[] type = BitConverter.GetBytes(this.ID);
            byte[] dLen = BitConverter.GetBytes(_dLen);

            if (!BitConverter.IsLittleEndian) // I honestly have no idea if this is correct, how I think it works:
            {                                 // Ex. Our end IS little, so no reversal, the receiving in IS NOT, so later (in Populate()), it is reversed, and it SHOULD work??
                type.Reverse();
                dLen.Reverse();
            }


            byte[] header = this.Header;
            // Copy the ID bytes to the START of the packet
            Array.Copy(type, 0, header, 0, type.Length);
            // Copy the length bytes to the END of the packet
            // "- 5" since the Length is 1-indexed, where actual entries are 0-indexed.
            Array.Copy(dLen, 0, header, header.Length - 4, dLen.Length);

            this.SetHeader(header);
        }

        public void SetAutomaticHeaderUpdates(bool enabled)
        {
            this.AutomaticHeaderUpdatesDisabled = !enabled;
        }

        public void SetAutoDispose(bool value)
        {
            this.AutoDispose = value;
        }

        public void SetMaxLength(int length)
        {
            this.MaxDataLength = length;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed) return;
            if (disposing)
            {
                this.Header = null;
                this.Data = null;
            }
            this._disposed = true;
        }
    }
}
