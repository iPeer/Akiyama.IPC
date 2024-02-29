using System.IO;
using System.IO.Compression;

namespace Akiyama.IPC.Shared.Network.Packets
{
    public class GZipPacket : Packet
    {
        public override int ID => (int)PacketType.GZIP;

        public byte[] DataBytes { get; set; }

        public override void Populate()
        {
            this.DataBytes = this.Decompress(this.Data);
        }

        public override void Prepare()
        {
            this.SetData(this.Compress(this.DataBytes));
        }

        private byte[] Compress(byte[] data)
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

        private byte[] Decompress(byte[] data)
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
