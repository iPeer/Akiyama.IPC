using Akiyama.IPC.Shared.Events;
using Akiyama.IPC.Shared.Helpers;
using Akiyama.IPC.Shared.Network.Packets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        /// <summary>
        /// The name of this <see cref="IPCEndpoint"/> pipe.
        /// </summary>
        public string PipeName { get; protected set; }

        /// <summary>
        /// Whether this <see cref="IPCEndpoint"/> has started and is currently running.
        /// </summary>
        public bool IsRunning { get; protected set; }
        /// <summary>
        /// Whether this <see cref="IPCEndpoint"/> is in the process of shutting down (terminating).
        /// </summary>
        public bool IsShuttingDown { get; protected set; }
        /// <summary>
        /// If <see langword="true"/>, indicates that both <see cref="IPCEndpoint"/>s have fully completed connecting to each other and are ready to send/receive data.
        /// </summary>
        public bool CompletedConnections { get; protected set; }

        /// <summary>
        /// If <see langword="true"/>, indicates that this <see cref="IPCEndpoint"/> is considered the server.
        /// </summary>
        public bool IsServer { get; protected set; }

        /// <summary>
        /// The instance of the <see cref="Akiyama.IPC.Shared.Network.PacketConstructor"/> used by this <see cref="IPCEndpoint"/>.
        /// </summary>
        public PacketConstructor PacketConstructor { get; protected set; }

        /// <summary>
        /// Contains all currently operational <see cref="SplitPacketContainer"/>s for this <see cref="IPCEndpoint"/>.
        /// </summary>
        /// <remarks>Added in 1.2.0</remarks>
        readonly Dictionary<string, SplitPacketContainer> SplitPacketContainers = new Dictionary<string, SplitPacketContainer>();

        /// <summary>
        /// The queue of packets waiting to be sent by this <see cref="IPCEndpoint"/>.
        /// </summary>
        private readonly List<Packet> PacketQueue = new List<Packet>();

        /// <summary>
        /// This <see cref="IPCEndpoint"/>'s outbound network stream.
        /// </summary>
        protected NamedPipeServerStream OUT_STREAM;
        /// <summary>
        /// This <see cref="IPCEndpoint"/>'s inbound network stream.
        /// </summary>
        protected NamedPipeClientStream IN_STREAM;

        /// <summary>
        /// In <see langword="true"/>, indicates that this <see cref="IPCEndpoint"/> has been disposed of.
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// If <see langword="true"/>, indicates that this <see cref="IPCEndpoint"/> is currently in the process of sending its queue of packets.
        /// </summary>
        private bool QueueSendInProgress = false;

        /// <summary>
        /// The <see cref="Thread"/> this <see cref="IPCEndpoint"/> is running on.
        /// </summary>
        protected Thread Thread;

        /// <summary>
        /// The object used to lock the queue from being modified while the <see cref="IPCEndpoint"/> is currently processing its packet queue. Specifically if accessed from multiple threads at once.
        /// </summary>
        private readonly object threadLock = new object();

        /// <summary>
        /// A cancellation token used to cancel this <see cref="IPCEndpoint"/>'s wait <see cref="Task"/> for waiting for the opposing IPCEndpoint to read all the bytes in the network stream.
        /// </summary>
        private readonly CancellationTokenSource pipeDrainCancellationToken = new CancellationTokenSource();

        /// <summary>
        /// If <see langword="true"/>, indicates that this <see cref="IPCEndpoint"/> should terminate when the opposing side disconnects instead of going back to waiting for connections.
        /// </summary>
        public bool TerminateOnDisconnect { get; set; } = false;

        /// <summary>
        /// Indicates whether this <see cref="IPCEndpoint"/> should automatically handle merging of split packets.
        /// <br /><b>Note</b>: If this property is <see langword="true"/>, received packets that are split will not raise the <see cref="PacketReceived"/> event. Instead, the <see cref="SplitPacketsReceived"/> will be raised when all pieces of the <see cref="Packet"/> have been received.
        /// <br />If this property is <see langword="false"/>, <see cref="PacketReceived"/> will be raised for each individual packet.
        /// </summary>
        public bool AutoHandleSplitPackets { get; set; } = true;

        /// <summary>
        /// Internal value storage for <see cref="RunAsBackgroundThread"/>.
        /// </summary>
        private bool _runAsBrackgroundThread = true;
        /// <summary>
        /// If <see langword="true"/>, indicates that <see cref="Thread"/> should be ran, or transition to running as a background thread.
        /// <br />This value can be changed while this <see cref="IPCEndpoint"/> is currently running, and will indicate to the thread that it should transition to the respective state for the given value.
        /// </summary>
        public bool RunAsBackgroundThread
        {
            get { return this._runAsBrackgroundThread; }
            set { 
                this._runAsBrackgroundThread = value;
                if (this.IsRunning) { this.Thread.IsBackground = value; }
            }
        }

        /* EVENTS */

        /// <summary>
        /// Occurs when both the server and client have fully connected to each other and are ready to send or recieve data.
        /// </summary>
        public event EventHandler<EventArgs> ConnectionsEstablished;
        /// <summary>
        /// Occurs when the opposing endpoint disconnects - regardless of whether the disconnect was expected or not.
        /// </summary>
        public event EventHandler<EventArgs> EndpointDisconnected;
        /// <summary>
        /// Occurs when a packet is done being fully received by this endpoint and after it is constructed by <see cref="PacketConstructor"/>.
        /// </summary>
        public event EventHandler<OnPacketReceivedEventArgs> PacketReceived;

        /// <summary>
        /// Occurs when this <see cref="IPCEndpoint"/> has fully received a split packet via its <see cref="SplitPacketContainer"/>.
        /// </summary>
        /// <remarks>Added in 1.2.0</remarks>
        public event EventHandler<OnAllSplitPacketsReceivedEventArgs> SplitPacketsReceived;

        // ————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————

        /* Event handling methods */

        /// <summary>
        /// Method used to invoke the <see cref="ConnectionsEstablished"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> to pass with this event.</param>
        protected virtual void OnConnectionsEstablished(EventArgs e)
        {
            this.ConnectionsEstablished?.Invoke(this, e);
        }

        /// <summary>
        /// Method used to invoke the <see cref="PacketReceived"/> event.
        /// </summary>
        /// <param name="e">The <see cref="OnPacketReceivedEventArgs"/> to pass with this event.</param>
        protected virtual void OnPacketReceived(OnPacketReceivedEventArgs e)
        {
            this.PacketReceived?.Invoke(this, e);
        }

        /// <summary>
        /// Method used to invoke the <see cref="EndpointDisconnected"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> to pass with this event.</param>
        protected virtual void OnEndpointDisconnected(EventArgs e)
        {
            this.EndpointDisconnected?.Invoke(this, e);
        }

        /// <summary>
        /// Method used to invoke the <see cref="SplitPacketsReceived"/> event.
        /// </summary>
        /// <param name="sender">The event's sender</param>
        /// <param name="e">The <see cref="OnAllSplitPacketsReceivedEventArgs"/> to pass with this event.</param>
        /// <remarks>Added in 1.2.0</remarks>
        protected virtual void OnAllSplitPacketsReceived(object sender, OnAllSplitPacketsReceivedEventArgs e)
        {
            // Is this even legal?
            string splitPacketKey = $"{e.Packet.ID}_{e.Packet.SplitID}";
            this.SplitPacketContainers[splitPacketKey].SplitPacketCompletelyReceived -= this.OnAllSplitPacketsReceived;
            this.SplitPacketContainers[splitPacketKey]?.Dispose();
            this.SplitPacketContainers.Remove(splitPacketKey);
            this.SplitPacketsReceived?.Invoke(sender, e);
        }

        /* Other methods */

        /// <summary>
        /// Used to clean up remaining instances of this <see cref="IPCEndpoint"/>'s streams if they weren't already cleaned up.
        /// </summary>
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

        /// <summary>
        /// Creates the correct directional network streams for this <see cref="IPCEndpoint"/>.
        /// </summary>
        public virtual void Create()
        {
            if (this.IsServer)
            {
                this.OUT_STREAM = new NamedPipeServerStream($"{this.PipeName}.OUT", PipeDirection.Out, 1, transmissionMode: PipeTransmissionMode.Message, options: PipeOptions.Asynchronous);
                this.IN_STREAM = new NamedPipeClientStream(".", $"{this.PipeName}.IN", PipeDirection.In, PipeOptions.Asynchronous);
            }
            else
            {
                this.OUT_STREAM = new NamedPipeServerStream($"{this.PipeName}.IN", PipeDirection.Out, 1, transmissionMode: PipeTransmissionMode.Message, options: PipeOptions.Asynchronous);
                this.IN_STREAM = new NamedPipeClientStream(".", $"{this.PipeName}.OUT", PipeDirection.In, PipeOptions.Asynchronous);
            }
        }

        /// <summary>
        /// Starts this <see cref="IPCEndpoint"/> instance. This method is <see langword="virtual"/>, and can be overridden.
        /// <br />Overriding classes must handle the threading and set <see cref="IsRunning"/> to <see langword="true"/> themselves.
        /// </summary>
        public virtual void Start()
        {
            this.Thread = new Thread(new ThreadStart(this.RunThread))
            {
                Name = $"Akiyama.IPC IPCEndPoint Thread: {this.PipeName}",
                IsBackground = this.RunAsBackgroundThread
            };
            this.IsRunning = true;
            this.Thread.Start();
        }

        /// <summary>
        /// Stops this <see cref="IPCEndpoint"/> instance. This method is <see langword="virtual"/>, and can be overridden.
        /// <br />Overriding classes should either call <c>base.Stop()</c> to ensure all procedures are completed, or make sure to handle it themselves.
        /// </summary>
        public virtual void Stop()
        {
            this.IsRunning = false;
            this.IsShuttingDown = true;
            if (this.CompletedConnections)
            {
                this.pipeDrainCancellationToken.Cancel();
                this.OUT_STREAM?.Disconnect();
            }
            this.CleanupStreams();
        }

        /// <summary>
        /// Sends the specified bytes to this <see cref="IPCEndpoint"/>'s outbound network stream.
        /// </summary>
        /// <param name="bytes">The bytes to send</param>
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

        /// <summary>
        /// Adds <paramref name="packets"/> to the queue of <see cref="Packet"/>s waiting to be sent by this <see cref="IPCEndpoint"/>, and initiates the process of sending all the packets currently in the queue.
        /// </summary>
        /// <param name="packets"></param>
        public void SendPacket(IEnumerable<Packet> packets) => QueuePackets(packets);

        /// <inheritdoc cref="QueuePacket(Packet)"/>
        public void SendPacket(Packet packet)
        {
            this.QueuePacket(packet);
        }
        /// <inheritdoc cref="SendPacket(IEnumerable{Packet})"/>
        public void SendPackets(IEnumerable<Packet> packets) => QueuePackets(packets);
        /// <inheritdoc cref="SendPacket(IEnumerable{Packet})"/>
        public void QueuePacket(IEnumerable<Packet> packets) => QueuePackets(packets);
        /// <inheritdoc cref="SendPacket(IEnumerable{Packet})"/>
        public void QueuePackets(IEnumerable<Packet> packets)
        {
            lock (threadLock)
            {
                this.PacketQueue.AddRange(packets);
                this.SendQueuedPackets();
            }
        }

        /// <summary>
        /// Adds <paramref name="packet"/> to the queue of <see cref="Packet"/>s waiting to be seny by this <see cref="IPCEndpoint"/>, and initiates the process of sending all the packets currently in the queue.
        /// </summary>
        /// <param name="packet">The <see cref="Packet"/> to be queued.</param>
        public void QueuePacket(Packet packet)
        {
            lock (threadLock)
            {
                this.PacketQueue.Add(packet);
                this.SendQueuedPackets();
            }
        }

        /// <summary>
        /// Sends all <see cref="Packet"/>s currently in <see cref="PacketQueue"/> sequentially via this <see cref="IPCEndpoint"/>'s outbound network stream.
        /// <br /><br /><b>Note</b>: While this method can be called manually, generally it is not required as adding packets to the queue will also initiate the process of sending the queue as soon as it is possible.
        /// </summary>
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
        /// Prepares <paramref name="packet"/> to be sent to this <see cref="IPCEndpoint"/>'s outbound network stream. Once prepared, it is immediately sent via <see cref="SendBytes(byte[])"/>.
        /// </summary>
        /// <param name="packet">The <see cref="Packet"/> being sent.</param>
        private void SendPacketToStream(Packet packet)
        {
            if (!packet.IsSplit) { packet.Prepare(); } // v1.2 - 07/03/24 -- Don't run Prepare() on split packets
            byte[] pBytes = new byte[packet.TotalLength + 1];
            pBytes[0] = PacketConstructor.PRE_PACKET_BYTE;
            Array.Copy(packet.Header, 0, pBytes, 1, packet.HeaderLength);
            Array.Copy(packet.Payload, 0, pBytes, packet.HeaderLength + 1, packet.PayloadLength);
            if (packet.AutoDispose)
            {
                packet.Dispose();
            }
            this.SendBytes(pBytes);
        }

        /// <summary>
        /// This method contains the logic for this <see cref="IPCEndpoint"/> that is ran via <see cref="Thread"/>. This method is <see langword="virtual"/>, and can be overridden.
        /// <br />Overriding classes will need to handle the entire network logic themselves, or be written in a way that calling <c>base.RunThread()</c> is possible.
        /// </summary>
        public virtual void RunThread()
        {
            while (this.IsRunning && !this.IsShuttingDown)
            {
                this.Create();
                if (this.IsServer)
                {
                    try { this.OUT_STREAM.WaitForConnection(); }
                    catch (Exception e) {
                        if (this.IsShuttingDown && (e is IOException || e is ObjectDisposedException))
                        {
                            break;
                        }
                        throw;
                    }
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
                    if (rBytes == PacketConstructor.PRE_PACKET_BYTE) // If the data starts with this magic byte, we got a packet - Later make this configurable??? (Also yes, I used the funny number)
                    {
                        Packet packet = this.PacketConstructor.CreateFromStream(this.IN_STREAM);
                        // If the packet IS split, call or create its SplitPacketContainer, and add the packet to it
                        if (packet.IsSplit && this.AutoHandleSplitPackets)
                        {
                            string splitPacketKey = $"{packet.ID}_{packet.SplitID}";
                            if (!this.SplitPacketContainers.ContainsKey(splitPacketKey))
                            {
                                SplitPacketContainer packetContainer = new SplitPacketContainer(endpoint: this, packetId: packet.ID, expectedTotalPackets: (packet.GetCustomHeaderByte(1) + 1));
                                packetContainer.SplitPacketCompletelyReceived += this.OnAllSplitPacketsReceived;
                                this.SplitPacketContainers.Add(splitPacketKey, packetContainer);
                            }
                            this.SplitPacketContainers[splitPacketKey].ReceivePacket(packet);
                            // Packets are NOT disposed here as we need to keep them around until we receive all of the splits
                            // They are later disposed when we have recieved ALL of them and raused the OnAllSplitPacketsReceived event
                        }
                        else // Otherwise, treat the packet as a normal one
                        {
                            // When we receive a split packet, its Populate() method is never called on creation, if the packet is split but failed the check above, call its Populate method
                            if (packet.IsSplit) { packet.Populate(); }
                            this.OnPacketReceived(new OnPacketReceivedEventArgs(packet));
                            packet.Dispose();
                        }
                    }
                }

                // If we exited the loop, the client has disconnected, reset and wait for another
                this.OnEndpointDisconnected(new EventArgs());
                this.CompletedConnections = false;
                this.CleanupStreams();
                if (this.TerminateOnDisconnect)
                {
                    this.IsRunning = false;
                    this.IsShuttingDown = true;
                }
                else
                {
                    Thread.Sleep(500);                                     // BF 29-02-2024: Introduce small delay before (re)starting the thread here to prevent
                    if (!this.IsRunning || this.IsShuttingDown) { break; } // race conditions in configurations that request clients disconnect with special packets
                }
            }
        }

        /// <summary>
        /// Releases all resources used by this <see cref="IPCEndpoint"/>, so that they can be reused or garbage collected.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The logical method for releasing this <see cref="IPCEndpoint"/>'s resources. This method is <see langword="virtual"/>, and can be overridden.
        /// <br />Overriding classes should call <c>base.Dispose(bool)</c> to ensure all resources are freed.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if we are actively disposing of this instances resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed) return;
            if (disposing)
            {
                this.CleanupStreams();
            }
            this._disposed = true;
        }

        /// <summary>
        /// Used to log basic information to the console when running in a debug environment. This method is <see langword="virtual"/>, and can be overridden.
        /// </summary>
        /// <param name="str">The <see cref="string"/> to log.</param>
        [Conditional("DEBUG")]
        public virtual void Log(string str)
        {
            Debug.WriteLine($"[{(this.IsServer ? "SERVER" : "CLIENT")}] {str}");
        }
    }
}
