using System.IO;
using System.IO.Compression;

namespace Akiyama.IPC.Shared.Network.Packets
{
    /// <summary>
    /// A packet used for transferring compressed data.
    /// <br /><br />
    /// <b>Note</b>: Packets with small amounts of data may result in diminishing or even negative returns when using this packet type.
    /// <br />If the size of the data being transferred by this packet is small, it may be better to use a <see cref="GenericDataPacket"/> instead.
    /// <br />The performance of your application(s) may also be impacted due to the overhead from (de)compressing data.
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

        protected byte[] Decompress(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    zip.Write(data,0, data.Length);
                    zip.Close();
                    return ms.ToArray();
                }
            }
        }

    }
}
