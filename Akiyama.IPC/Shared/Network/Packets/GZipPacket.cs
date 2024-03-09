using System;
using System.IO;
using System.IO.Compression;

namespace Akiyama.IPC.Shared.Network.Packets
{
    /// <summary>
    /// A packet used for transferring compressed data.
    /// <br /><br />This packet implements both <see cref="Packet.Prepare"/> and <see cref="Packet.Populate"/> to automatically process its Properties into/from its payload when send or received respectively.
    /// <br /><br />
    /// <b>Note</b>: Packets with small amounts of data may result in diminishing or even negative returns when using this packet type.
    /// <br />If the size of the data being transferred by this packet is small, it may be better to use a <see cref="GenericDataPacket"/> instead.
    /// <br />The performance of your application(s) may also be impacted due to the overhead from (de)compressing the data.
    /// </summary>
    public class GZipPacket : Packet
    {
        public override int ID => (int)PacketType.GZIP;

        public byte[] DataBytes { get; set; }

        public override void Populate()
        {
            this.DataBytes = this.Decompress(this.Payload);
        }

        public override void Prepare()
        {
            this.SetPayload(this.Compress(this.DataBytes));
        }

        /// <summary>
        /// Compresses the data within <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data to compress</param>
        /// <returns>A byte array containing the compressed data, created from the data within <paramref name="data"/>.</returns>
        protected byte[] Compress(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress))
                {
                    zip.Write(data, 0, data.Length);
                    zip.Close();
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Decompresses the data within <paramref name="data"/> into its decompressed form.
        /// </summary>
        /// <param name="data">The data to decompress</param>
        /// <returns>A byte array containing the decompressed data, created from the compressed data in <paramref name="data"/>.</returns>
        protected byte[] Decompress(byte[] data)
        {

            using (MemoryStream cs = new MemoryStream(data))
            {
                using (GZipStream zip = new GZipStream(cs, CompressionMode.Decompress))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        zip.CopyTo(ms);
                        return ms.ToArray();
                    }
                }
            }

        }

        protected override void Dispose(bool disposing)
        {
            if (this._disposed) return;
            if (disposing)
            {
                this.DataBytes = Array.Empty<byte>();
            }
            base.Dispose(disposing);

        }

    }
}
