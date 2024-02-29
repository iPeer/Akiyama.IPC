using Akiyama.IPC.Shared.Events;
using Akiyama.IPC.Shared.Network.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Akiyama.IPC.Shared.Network
{
    /// <summary>
    /// The base class for IPC Endpoints. This class is <see langword="abstract"/>.
    /// </summary>
    public abstract class IPCEndpoint : IDisposable
    {

        public string Name { get; protected set; }

        public bool IsRunning { get; protected set; }
        public bool IsShuttingDown { get; protected set; }
        public bool CompletedConnections { get; protected set; }

        public bool IsServer { get; protected set; }

        public PacketConstructor PacketConstructor { get; protected set; }

        private readonly List<Packet> PacketQueue = new List<Packet>();

        protected NamedPipeServerStream OUT_STREAM;
        protected NamedPipeClientStream IN_STREAM;

        private bool _disposed = false;

        private bool QueueSendInProgress = false;

        protected Thread Thread;

        private object threadLock = new object();

        private CancellationTokenSource pipeDrainCancellationToken = new CancellationTokenSource();

        /* EVENTS */

        public event EventHandler<EventArgs> ConnectionsEstablished;
        public event EventHandler<EventArgs> EndpointDisconnected;
        public event EventHandler<OnPacketReceivedEventArgs> PacketReceived;

        // ————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————

        /* Event handling methods */

        protected virtual void OnConnectionsEstablished(EventArgs e)
        {
            this.ConnectionsEstablished?.Invoke(this, e);
        }

        protected virtual void OnPacketReceived(OnPacketReceivedEventArgs e)
        {
            this.PacketReceived?.Invoke(this, e);
        }

        protected virtual void OnEndpointDisconnected(EventArgs e)
        {
            this.EndpointDisconnected?.Invoke(this, e);
        }

        /* Other methods */

        protected void CleanupStreams()
        {
            if (this.OUT_STREAM != null)
            {
                this.OUT_STREAM.Dispose();
                this.OUT_STREAM = null;
            }
            if (this.IN_STREAM != null)
            {
                this.IN_STREAM.Dispose();
                this.IN_STREAM = null;
            }
        }

        public virtual void Create()
        {
            if (this.IsServer)
            {
                this.OUT_STREAM = new NamedPipeServerStream($"{this.Name}.OUT", PipeDirection.Out, 1, transmissionMode: PipeTransmissionMode.Message, options: PipeOptions.Asynchronous);
                this.IN_STREAM = new NamedPipeClientStream(".", $"{this.Name}.IN", PipeDirection.In, PipeOptions.Asynchronous);
            }
            else
            {
                this.OUT_STREAM = new NamedPipeServerStream($"{this.Name}.IN", PipeDirection.Out, 1, transmissionMode: PipeTransmissionMode.Message, options: PipeOptions.Asynchronous);
                this.IN_STREAM = new NamedPipeClientStream(".", $"{this.Name}.OUT", PipeDirection.In, PipeOptions.Asynchronous);
            }
        }

        public virtual void Start()
        {
            this.Thread = new Thread(new ThreadStart(this.RunThread))
            {
                Name = $"Akiyama.IPC PipeEndPoint Thread: {this.Name}",
                IsBackground = false
            };
            this.IsRunning = true;
            this.Thread.Start();
        }

        public virtual void Stop()
        {
            this.IsRunning = false;
            this.IsShuttingDown = true;
            if (this.CompletedConnections)
            {
                this.pipeDrainCancellationToken.Cancel();
                this.OUT_STREAM.Disconnect();
                this.IN_STREAM.Close();
            }
        }

        private void SendBytes(byte[] bytes)
        {
            // TODO: Maybe rewrite this part to use 'using'?
            // The send task cannot (theoretically) hang forever, so we don't need to be able to cancel it during shutdown as it will presumably finish eventually
            Task send = Task.Run(() => this.OUT_STREAM.Write(bytes, 0, bytes.Length));
            send.Wait();
            // The drain task, however...
            Task drain = Task.Run(() => this.OUT_STREAM.WaitForPipeDrain());
            try
            {
                drain.Wait(cancellationToken: this.pipeDrainCancellationToken.Token);
            }
            catch (OperationCanceledException) { } // The only time the task is ever cancelled is during a shutdown, so ignore the exception.

            send.Dispose();
            drain.Dispose();
        }

        public void SendPacket(IEnumerable<Packet> packets) => QueuePackets(packets);
        public void SendPackets(IEnumerable<Packet> packets) => QueuePackets(packets);
        public void QueuePacket(IEnumerable<Packet> packets) => QueuePackets(packets);
        public void QueuePackets(IEnumerable<Packet> packets)
        {
            lock (threadLock)
            {
                this.PacketQueue.AddRange(packets);
                this.SendQueuedPackets();
            }
        }

        public void QueuePacket(Packet packet)
        {
            lock (threadLock)
            {
                this.PacketQueue.Add(packet);
                this.SendQueuedPackets();
            }
        }

        public void SendQueuedPackets()
        {
            if (this.QueueSendInProgress) { return; } 
            this.QueueSendInProgress = true;
            List<Packet> lCopy = this.PacketQueue.ToList();
            this.PacketQueue.Clear();
            foreach (Packet p in lCopy)
            {
                this.SendPacketToStream(p);
            }
            this.QueueSendInProgress = false;
            if (this.PacketQueue.Count > 0) { this.SendQueuedPackets(); }
        }

        /// <summary>
        /// Sends a <see cref="Packet"/> to this endpoint's <see cref="OUT_STREAM"/>. The packet will be added to the <see cref="IPCEndpoint"/>'s queue.
        /// </summary>
        /// <param name="packet">The <see cref="Packet"/> to be sent.</param>
        private void SendPacketToStream(Packet packet)
        {
            byte[] pBytes = new byte[packet.TotalLength + 1];
            pBytes[0] = this.PacketConstructor.PRE_PACKET_BYTE;
            Array.Copy(packet.Header, 0, pBytes, 1, packet.HeaderLength);
            Array.Copy(packet.Data, 0, pBytes, packet.HeaderLength + 1, packet.DataLength);
            if (packet.AutoDispose)
            {
                packet.Dispose();
            }
            this.SendBytes(pBytes);
        }

        /// <inheritdoc cref="SendPacketToStream(Packet)"/>
        public void SendPacket(Packet packet)
        {
            this.QueuePacket(packet);
        }

        public virtual void RunThread()
        {
            this.Create();
            while (this.IsRunning && !this.IsShuttingDown)
            {
                if (this.IsServer)
                {
                    try { this.OUT_STREAM.WaitForConnection(); }
                    catch { if (!this.IsShuttingDown) { continue; }; break; }
                    this.Log("Server has received client connection!");
                    // If we reach here, a client has connected.

                    // If a client has connected, attempt to connect to its output stream
                    this.Log("Connecting to client's OUT...");
                    this.IN_STREAM.Connect();
                    this.Log("Connected!");

                    this.CompletedConnections = true;

                    this.OnConnectionsEstablished(new EventArgs());
                }
                else
                {
                    this.IN_STREAM.Connect();
                    this.Log("Connected to server, waiting for server to connect to OUT...");
                    // If we conneted to the server, wait for it to connect to the client's OUT stream
                    try { this.OUT_STREAM.WaitForConnection(); }
                    catch { if (!this.IsShuttingDown) { continue; }; break; }
                    this.Log("Server has connected to OUT");
                    // If we reach here, a client has connected.
                    this.CompletedConnections = true;
                    this.OnConnectionsEstablished(new EventArgs());
                }

                // If the client disconnects, handle that before any stream logic
                if (!this.OUT_STREAM.IsConnected)
                {
                    this.OUT_STREAM.Disconnect();
                    break;
                }

                // This loop reads IN_STREAM to check if we have any data on it
                int _byte;
                while (this.IN_STREAM.IsConnected && (_byte = this.IN_STREAM.ReadByte()) != -1 && !this.IsShuttingDown && this.IsRunning)
                {
                    byte rBytes = (byte)_byte; // Just to make things easier
                    if (rBytes == this.PacketConstructor.PRE_PACKET_BYTE) // If the data starts with this magic byte, we got a packet - Later make this configurable??? (Also yes, I used the funny number)
                    {
                        using (Packet packet = this.PacketConstructor.CreateFromStream(this.IN_STREAM))
                        {
                            this.OnPacketReceived(new OnPacketReceivedEventArgs(packet));
                        }
                    }
                }

                // If we exited the loop, the client has disconnected, reset and wait for another
                this.OnEndpointDisconnected(new EventArgs());
                this.CompletedConnections = false;
                this.CleanupStreams();
                if (this.IsRunning)
                {
                    this.Create();
                }
            }
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
                this.CleanupStreams();
            }
            this._disposed = true;
        }

        [Conditional("DEBUG")]
        public virtual void Log(string str)
        {
            Debug.WriteLine($"[{(this.IsServer ? "SERVER" : "CLIENT")}] {str}");
        }
    }
}
