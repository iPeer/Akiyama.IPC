using Akiyama.IPC.Shared.Events;
using Akiyama.IPC.Shared.Network;
using Akiyama.IPC.Shared.Network.Packets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Akiyama.IPC.Shared.Helpers
{
    /// <summary>
    /// A class used to facilitate automatically concatinating packets that have been split back together once all pieces have been received
    /// </summary>
    /// <remarks>Added in 1.2.0</remarks>
    public class SplitPacketContainer : IDisposable
    {
        // TODO: Finish docstrings
        private readonly List<Packet> IncomingPackets = new List<Packet>();
        private readonly int PacketID;
        private readonly IPCEndpoint Endpoint;
        private byte[] ReceivedPackets = Array.Empty<byte>();
        public int ExpectedPacketCount { get; private set; }

        // ———————————————— EVENTS ————————————————

        public event EventHandler<OnAllSplitPacketsReceivedEventArgs> SplitPacketCompletelyReceived;

        // ————————————————————————————————————————

        private bool _disposed = false;

        public SplitPacketContainer(IPCEndpoint endpoint, int packetId, int expectedTotalPackets)
        {
            this.Endpoint = endpoint;
            this.PacketID = packetId;
            this.ExpectedPacketCount = expectedTotalPackets;
            this.ReceivedPackets = new byte[expectedTotalPackets];
        }

        public void ReceivePacket(Packet packet)
        {
            int packetIndex = packet.GetCustomHeaderByte(0);

            this.ReceivedPackets[packetIndex] = 1;
            this.IncomingPackets.Add(packet);
            if (!this.ReceivedPackets.Any(a => a == 0)) // Abuse the fact that bytes default to 0x0 to check if all of the parts of the packets have been received since we set received to 1
            {
                // I fear no man. But this code? ... it scares me — Akiyama

                // Sort the packets into the right oder since we cannot guarantee they appear in order
                this.IncomingPackets.Sort((a, b) => { return a.GetCustomHeaderByte(0).CompareTo(b.GetCustomHeaderByte(0)); });

                List<byte[]> payloadDatas = new List<byte[]>();
                foreach (Packet p in this.IncomingPackets)
                {
                    payloadDatas.Add(p.Payload);
                }
                List<byte> _payload = new List<byte>();
                foreach (byte[] bytes in payloadDatas)
                {
                    _payload.AddRange(bytes);
                }
                payloadDatas.Clear();
                byte[] payload = _payload.ToArray();
                _payload.Clear();
                using (Packet _packet = this.Endpoint.PacketConstructor.packetTyper.GetPacketObjectFromId(this.PacketID))
                {
                    _packet.SetMaxLength(payload.Length);
                    _packet.SetPayload(payload);
                    _packet.SetSplitId(this.IncomingPackets.First().SplitID);
                    _packet.Populate();
                    this.SplitPacketCompletelyReceived?.Invoke(this, new OnAllSplitPacketsReceivedEventArgs(_packet));
                }
            }
        }

        /// <summary>
        /// Releases all resources used by this object so that they may be cleaned up by Garbage Collection.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (this._disposed) { return; }
            if (disposing)
            {
                foreach (Packet p in this.IncomingPackets)
                {
                    p.Dispose();
                }
                this.IncomingPackets.Clear();
                this.ReceivedPackets = Array.Empty<byte>();
            }
            this._disposed = true;
        }

    }
}
