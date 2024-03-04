using System.Reflection;

namespace Akiyama.IPC.Shared.Network.Packets
{

    /// <summary>
    /// Represents the base class for IPC packets. This class is <see langword="abstract"/>.
    /// </summary>
    public abstract class Packet : IDisposable
    {

        /// <summary>
        /// <see langword="true"/> if this instance has been disposed, otherwise <see langword="false"/>.
        /// </summary>
        private bool _disposed;
        /// <summary>
        /// Returns the minimum functional header length that contains enough space for all the required elements. This field is <see langword="static"/> and <see langword="readonly"/>.
        /// </summary>
        /// <remarks>Added in 1.1.0</remarks>
        public static readonly int BASE_HEADER_SIZE = ((sizeof(byte) * 3) + (sizeof(int) * 2));

        /// <summary>
        /// The number of bytes to add to <see cref="BASE_HEADER_SIZE"/> for user-customisable bytes. This field is <see langword="static"/> and <see langword="readonly"/>.
        /// </summary>
        /// <remarks>Added in 1.1.0</remarks>
        public static readonly int CUSTOM_HEADER_BYTES = 12;
        /// <summary>
        /// Indicates the length the Packet header. This field is <see langword="static"/> and <see langword="readonly"/>.
        /// </summary>
        public static readonly int HEADER_SIZE = (BASE_HEADER_SIZE + CUSTOM_HEADER_BYTES);

        /// <summary>
        /// The ID of this Packet.
        /// </summary>
        public abstract int ID { get; }
        /// <summary>
        /// The payload of this Packet.
        /// <br/><br/><b>Note</b>: Data should not be directly written to this Property. <see cref="Packet.SetPayload(byte[])"/> should be used for that instead.
        /// </summary>
        public byte[] Payload { get; private set; } = Array.Empty<byte>(); // BF-08-02-2024: Prevent Data from defaulting to null in packets that don't pass any data
        /// <inheritdoc cref="Payload"/>
        [Obsolete("This Property is deprecated and will be removed in a future update. Use Payload instead.")]
        public byte[] Data { get { return this.Payload; } }
        /// <summary>
        /// This <see cref="Packet"/>'s Custom Header Bytes.
        /// </summary>
        /// <remarks>Added in 1.1.0</remarks>
        public byte[] CustomHeaderBytes { get; private set; } = new byte[CUSTOM_HEADER_BYTES];
        /// <summary>
        /// The Header bytes for this Packet.
        /// <br/><br/><b>Note</b>: Modifying this value directly should be avoided. See <see cref="SetCustomHeaderByte(byte, int)"/> and <see cref="SetCustomHeaderBytes(byte[], int)"/> instead.
        /// </summary>
        public byte[] Header { get; private set; } = new byte[HEADER_SIZE];
        /// <summary>
        /// Returns the length of this packet's header. This is usually the same as <see cref="Packet.HEADER_SIZE"/>.
        /// </summary>
        public int HeaderLength => this.Header.Length;
        /// <summary>
        /// Returns the length this Packet's <see cref="Payload"/> (payload).
        /// </summary>
        [Obsolete("This Property is deprecated and will be removed in a future update. Use PayloadLength instead")]
        public int DataLength => this.Payload.Length;
        /// <summary>
        /// Returns the length this Packet's <see cref="Payload"/> (payload).
        /// </summary>
        public int PayloadLength => this.Payload.Length;
        /// <summary>
        /// Returns the combined length of this Packet's header and payload.
        /// </summary>
        public int TotalLength => this.Payload.Length + this.Header.Length;

        /// <summary>
        /// The originating library version of this packet.
        /// </summary>
        public Version Version { get; private set; } = Assembly.GetExecutingAssembly().GetName().Version;

        /// <summary>
        /// If <b>true</b>, calling <see cref="SetPayload(byte[])"/> will not automatically update the packet's header, requiring manual calls to <see cref="UpdateHeader()"/> instead.
        /// <br/>This Property can be configured via <see cref="SetAutomaticHeaderUpdates(bool)"/>.
        /// </summary>
        public bool AutomaticHeaderUpdatesDisabled { get; private set; }

        /// <summary>
        /// Determines whether this packet is automatically has <see cref="Packet.Dispose()"/> called after being sent to the opposing endpoint.
        /// <br />If this is disabled (set to <c>false</c>), the packet will need to be manually disposed of after use.
        /// <br />This Property can be configured via <see cref="SetAutoDispose(bool)"/>.
        /// </summary>
        public bool AutoDispose { get; private set; } = true;

        /// <summary>
        /// Returns the maximum supported payload length for this particular Packet.
        /// <br />This is set via the class' constructor, or <see cref="SetMaxLength(int)"/>.
        /// </summary>
        public int MaxPayloadLength { get; private set; } = int.MaxValue;
        /// <inheritdoc cref="MaxPayloadLength"/>
        [Obsolete("This Property is deprecated and will be removed in a future update. Use MaxPayloadLength instead.")]
        public int MaxDataLength {  get { return this.MaxPayloadLength; } }

        /// <summary>
        /// Creates a new instance of this <see cref="Packet"/> with no predfined restrictions or content.
        /// </summary>
        public Packet()
        {
            this.Init();
            this.UpdateHeader();
        }
        /// <summary>
        /// Create a new instance of this <see cref="Packet"/> with a pre-defined <see cref="Payload"/>.
        /// </summary>
        /// <param name="data">The data to initialise this packet with.</param>
        /// <param name="maxPayloadLength">The maximum supported data length for this packet.</param>
        /// <param name="autoDispose">Should this packet be automatically disposed after it is sent to the opposing endpoint?</param>
        public Packet(byte[] data, int maxPayloadLength = int.MaxValue, bool autoDispose = true)
        {
            this.Init();
            this.MaxPayloadLength = maxPayloadLength;
            this.AutoDispose = autoDispose;
            this.SetPayload(data);
        }
        /// <summary>
        /// Create a new instance of this <see cref="Packet"/> with no pre-defined <see cref="Payload"/>.
        /// </summary>
        /// <param name="maxPayloadLength">The maximum supported data length for this packet.</param>
        /// <param name="autoDispose"><see langword="true"/> if this Packet should be automatically disposed after being sent, otherwise <see langword="false"/>.</param>
        public Packet(int maxPayloadLength = int.MaxValue, bool autoDispose = true)
        {
            this.Init();
            this.MaxPayloadLength = maxPayloadLength;
            this.AutoDispose = autoDispose;
            this.UpdateHeader(); // BF-28-02-2024: Ensure headers are updated on new packets to prevent their values being nulls
        }

        /// <summary>
        /// This method is called immediately after a packet is constructed, and allows packets to enforce limitations such as max payload size and AutoDispose state and/or set default values to their properties at initialisation time.
        /// <br /><br /><b>Note</b>: At the time this mmethod is called, packets do not have access to their <see cref="Header"/> or <see cref="Payload"/>.
        /// </summary>
        /// <remarks>Added in 1.1.0</remarks>
        public virtual void Init() { }

        /// <summary>
        /// When overridden, allows the packet to set up data after being received, so that it may be accessed later, for example through Properties.
        /// <br />If not overridden, the packet will not be populated, however its raw data will still be available via <see cref="Payload"/>.
        /// <br />For examples of this method's usage, see <seealso href="https://github.com/iPeer/Akiyama.IPC/blob/master/Akiyama.IPC/Shared/Network/Packets/TestPacket.cs"/>.
        /// <br /><br />
        /// <b>This method is called immediately after the packet is received.</b>
        /// </summary>
        public virtual void Populate() { }

        /// <summary>
        /// When overridden, allows the packet to prepare data before being sent, such as setting its payload to be set using its internal properties.
        /// <br />If not overridden, the packet will not prepare anything before transmission, and any data not already added to <see cref="Payload"/> is lost.
        /// <br />For examples of this method's usage, see <seealso href="https://github.com/iPeer/Akiyama.IPC/blob/master/Akiyama.IPC/Shared/Network/Packets/TestPacket.cs"/>.
        /// <br /><br />
        /// <b>This method is called immediately before the packet is sent.</b>
        /// <br />
        /// <br /><b>Note</b>: When setting the packet's data, it should be done via <see cref="SetPayload(byte[])"/>, as assigning it directly to the <see cref="Payload"/> property will not update the packet's header without <see cref="UpdateHeader()"/> being called.
        /// <br />If <c><see cref="AutomaticHeaderUpdatesDisabled"/></c> is set to <b>true</b>, <see cref="SetPayload(byte[])"/> <b>will not automatically update the packet's headers</b> and the user <b>must</b> call <see cref="UpdateHeader()"/> manually.
        /// <br /><br /><b>WARNING</b>: This method can be destructive to data already contained within <see cref="Payload"/> if the overriding method is not written in a way that preserves it.
        /// </summary>
        public virtual void Prepare() { }

        /// <summary>
        /// Sets the <see cref="Version"/> for this packet.
        /// <br /><br /><b>Note</b>: This refers to the version of the library used to SEND this packet and is set automatically by the <see cref="PacketConstructor"/>.
        /// </summary>
        /// <param name="major">The major version number</param>
        /// <param name="minor">The minor version number</param>
        /// <param name="patch">The patch version number (referred to as "build" in VS)</param>
        /// <remarks>Added in 1.1.0</remarks>
        internal void SetVersion(byte major, byte minor, byte patch)
        {
            this.Version = new Version(major, minor, patch); 
        }

        /// <summary>
        /// Sets the payload for this packet.
        /// <br /><b>Note</b>: Larger packets will take longer to send, take longer to be read, and will use more RAM (on both ends) while being processed. Consider splitting larger packets to make them more efficient.
        /// <br/><br/>Throws <see cref="InvalidOperationException"/> if the length of <paramref name="data"/> exceeds this Packet's <see cref="MaxPayloadLength"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="InvalidOperationException"></exception>"
        public void SetPayload(byte[] data)
        {
            if (data.Length > this.MaxPayloadLength)
            {
                throw new InvalidOperationException($"Payload is too big for this packet's configuration. Max length is {this.MaxPayloadLength}, but given data was {data.Length}.");
            }
            this.Payload = data;
            if (!this.AutomaticHeaderUpdatesDisabled) { this.UpdateHeader(); }
        }
        /// <inheritdoc cref="SetPayload(byte[])"/>
        [Obsolete("This method is deprecated and will be removed in a future update. Use SetPayload(byte[]) instead.")]
        public void SetData(byte[] data) => SetPayload(data);

        /// <summary>
        /// Appends <paramref name="bytes"/> to this <see cref="Packet"/>'s current Payload.
        /// </summary>
        /// <param name="bytes">The bytes to append.</param>
        /// <remarks>Added in 1.1.0</remarks>
        public void AppendPayload(byte[] bytes) // Add a prepend too??
        {
            byte[] newPayload = new byte[this.PayloadLength + bytes.Length];
            Array.Copy(this.Payload, newPayload, this.PayloadLength);
            Array.Copy(bytes, 0, newPayload, this.PayloadLength, bytes.Length);
            this.SetPayload(newPayload);
        }

        /// <summary>
        /// Sets the Header for this packet. <b>This function should only be used internally.</b>
        /// </summary>
        /// <param name="header">The bytes to set thi Packet's header to.</param>
        private void SetHeader(byte[] header)
        {
            this.Header = header;
        }

        /// <summary>
        /// Allows the setting of the custom packet header byte at offset <paramref name="offset"/>. For retreiving this data later, see <see cref="GetCustomHeaderByte(int)"/>.
        /// <br />Throws <see cref="ArgumentOutOfRangeException"/> if the specified index is outside the range of editable bytes.
        /// <br /><br /><b>Note</b>: The offset is in relation to the custom data itself, not the packet header as a whole, ie. an index of 0 is the first byte of the custom data.
        /// </summary>
        /// <param name="value">The value to set the <paramref name="offset"/> byte to.</param>
        /// <param name="offset">The zero-based offset at which to write <paramref name="value"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetCustomHeaderByte(byte value, int offset)
        {
            if (offset < 0 || offset >= CUSTOM_HEADER_BYTES) { throw new ArgumentOutOfRangeException("offset"); }
            this.CustomHeaderBytes[offset] = value;
            this.UpdateCustomHeaderBytes();
        }

        /// <summary>
        /// Allows the setting of a range of custom packet header bytes from <paramref name="data"/> starting at offset <paramref name="offset"/>. For retreiving this data later, see <see cref="GetCustomHeaderBytes(byte[], int)"/> and <see cref="GetCustomHeaderBytes(int, int)"/>.
        /// <br />Throws <see cref="ArgumentException"/> if <paramref name="data"/> is null.
        /// <br />Throws <see cref="ArgumentException"/> if, given <paramref name="offset"/>, <paramref name="data"/> will not fit within the custom header range, or if <paramref name="offset"/> is &lt; 0.
        /// <br /><br /><b>Note</b>: The offset is in relation to the custom data itself, not the packet header as a whole, ie. an index of 0 is the first byte of the custom data.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="offset">The zero-based offset at which to begin writing <paramref name="data"/>.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void SetCustomHeaderBytes(byte[] data, int offset)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            if (data.Length == 0) { return; }
            if (offset < 0 || offset >= CUSTOM_HEADER_BYTES) { throw new ArgumentException("Supplied data falls outside of available space.", "data"); }
            Array.Copy(data, 0, this.CustomHeaderBytes, offset, data.Length);
            this.UpdateCustomHeaderBytes();
        }

        /// <summary>
        /// Gets the byte at <paramref name="index"/> from the packet's custom header bytes. For writing this data, see <see cref="SetCustomHeaderByte(byte, int)"/>.
        /// <br />Throws <see cref="ArgumentOutOfRangeException"/> if the specified index is outside the range of editable bytes.
        /// </summary>
        /// <param name="index">The index at which to get the byte from</param>
        /// <returns>A byte from the packet's custom header bytes at <paramref name="index"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public byte GetCustomHeaderByte(int index)
        {
            if (index < 0 || index >= CUSTOM_HEADER_BYTES) { throw new ArgumentOutOfRangeException("offset"); }
            return this.CustomHeaderBytes[index];
        }

        /// <summary>
        /// Returns a byte array filled with the bytes starting at <paramref name="offset"/> and going for <paramref name="length"/> bytes. For writing this data, see <see cref="SetCustomHeaderBytes(byte[], int)"/>.
        /// </summary>
        /// <param name="offset">The zero-based offset at which to begin reading bytes.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns></returns>
        public byte[] GetCustomHeaderBytes(int offset, int length)
        {
            byte[] buffer = new byte[length];
            this.GetCustomHeaderBytes(buffer, offset);
            return buffer;
        }

        /// <summary>
        /// Fills <paramref name="buffer"/> with bytes from this packet's custom header bytes, starting at offset <paramref name="offset"/>. The number of bytes read is based on <paramref name="buffer"/>'s length.
        /// <br/>Throws <see cref="ArgumentException"/> if <paramref name="buffer"/> is <c>null</c>.
        /// <br/>Throws <see cref="InvalidOperationException"/> if the length of <paramref name="buffer"/> is 0.
        /// <br/>Throws <see cref="ArgumentException"/> if the offset and/or length of <paramref name="buffer"/> would result in reading data outside of this packet's custom header bytes, or if <paramref name="offset"/> is &lt; 0.
        /// </summary>
        /// <param name="buffer">The buffer in which to write the bytes to. The number of bytes read is based off of this parameter's length.</param>
        /// <param name="offset">The zero-based offset at which to begin reading bytes.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void GetCustomHeaderBytes(byte[] buffer, int offset)
        {
            if (buffer == null) { throw new ArgumentNullException("buffer"); }
            if (buffer.Length == 0) { throw new InvalidOperationException("Cannot read into 0-length buffer"); }
            if (offset < 0 || offset >= CUSTOM_HEADER_BYTES || (offset + buffer.Length) >= CUSTOM_HEADER_BYTES) { throw new ArgumentException("Supplied buffer falls outside of available space.", "buffer"); }
            Array.Copy(this.Header, offset, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Forces this <see cref="Packet"/> to update its header after <see cref="CustomHeaderBytes"/> has been changed.
        /// </summary>
        /// <remarks>Added in 1.1.0</remarks>
        private void UpdateCustomHeaderBytes()
        {
            byte[] headerBytes = this.Header;
            Array.Copy(this.CustomHeaderBytes, 0, headerBytes, HEADER_SIZE - CUSTOM_HEADER_BYTES, CUSTOM_HEADER_BYTES);
            this.SetHeader(headerBytes);
        }

        /// <summary>
        /// Update this Packet's header immediately, regardless of the <see cref="AutomaticHeaderUpdatesDisabled"/> property.
        /// </summary>
        public void UpdateHeader()
        {
            /*
             * CURRENT HEADER STRUCTURE:
             * [Bytes 0-3]: int32 indicating the TYPE of this packet
             * [Bytes 4-15]: Customisable header data that the user can set to convey additional data
             * [Bytes 16-19]: int32 indicating the LENGTH of the DATA in this packet
             */
            int _dLen = this.Payload.Length;
            byte[] type = BitConverter.GetBytes(this.ID);
            byte[] dLen = BitConverter.GetBytes(_dLen);

            if (!BitConverter.IsLittleEndian) // I honestly have no idea if this is correct, how I think it works:
            {                                 // Ex. Our end IS little, so no reversal, the receiving in IS NOT, so later (in Populate()), it is reversed, and it SHOULD work??
                type.Reverse();
                dLen.Reverse();
            }


            byte[] header = this.Header;
            // Copy the ID bytes to the header
            Array.Copy(type, 0, header, 0, type.Length);
            // Copy the length bytes to the header
            Array.Copy(dLen, 0, header, 4, dLen.Length);
            // Add the current assembly's version to the packet header
            byte[] versionBytes = new byte[3] { (byte)this.Version.Major, (byte)this.Version.Minor, (byte)this.Version.Build };
            Array.Copy(versionBytes, 0, header, 8, versionBytes.Length);
            // Write the Custom Header Bytes to the header
            Array.Copy(this.CustomHeaderBytes, 0, header, BASE_HEADER_SIZE, CUSTOM_HEADER_BYTES);

            this.SetHeader(header);
        }

        /// <summary>
        /// Enable or disable automatic header updates for this specific Packet.
        /// <br /><br /><b>Note</b>: If this setting is disabled (<c>false</c>), then packets will require manual header updates via <see cref="UpdateHeader()"/>.
        /// </summary>
        /// <param name="enabled">The state for this setting. <see langword="true"/> to enable (default), <see langword="false"/> to disable.</param>
        public void SetAutomaticHeaderUpdates(bool enabled)
        {
            this.AutomaticHeaderUpdatesDisabled = !enabled;
        }

        /// <summary>
        /// Changes whether this particular packet is automatically disposed after being sent to the opposing endpoint.
        /// <br /><br /><b>Note</b>: If this setting is disabled (<c>false</c>), then packets will require manual disposal via calling <see cref="Dispose()"/>.
        /// </summary>
        /// <param name="enabled">The state for this setting; <see langword="true"/> to enable (default), <see langword="false"/> to disable.</param>
        public void SetAutoDispose(bool enabled)
        {
            this.AutoDispose = enabled;
        }

        /// <summary>
        /// Sets the maximum data (payload) length for this particular Packet.
        /// <br />Throws <see cref="ArgumentOutOfRangeException"/> if <paramref name="length"/> is less than 0.
        /// <br />Throws <see cref="InvalidOperationException"/> if the Packet's current length already exceeds <paramref name="length"/>.
        /// </summary>
        /// <param name="length">The length to set the limit to</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void SetMaxLength(int length)
        {
            if (length < 0) { throw new ArgumentOutOfRangeException("length"); }
            if (this.PayloadLength > length) { throw new InvalidOperationException("The packet's current payload length exceeds 'length'."); }
            this.MaxPayloadLength = length;
        }

        /// <summary>
        /// Releases all resources used by this packet so that they may be cleaned up by Garbage Collection.
        /// </summary>
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
                this.Payload = null;
            }
            this._disposed = true;
        }
    }
}
