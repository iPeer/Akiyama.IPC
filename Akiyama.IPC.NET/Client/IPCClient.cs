using Akiyama.IPC.Shared.Network;
using Akiyama.IPC.Shared.Network.Packets;
using Akiyama.IPC.Shared.Typers;
using System;
using System.Diagnostics;
using System.Threading;

namespace Akiyama.IPC.Client
{
    /// <summary>
    /// A wrapper class for an <see cref="IPCEndpoint"/> that behaves as an IPC client.
    /// </summary>
    public class IPCClient : IPCEndpoint, IDisposable
    {

        /// <summary>
        /// Constructs an instance of an <see cref="IPCClient"/> using the default <see cref="PacketTyper"/>.
        /// </summary>
        /// <param name="pipeName">The pipe/server name this instance will connect to.</param>
        public IPCClient(string pipeName) : this(pipeName, new DefaultPacketTyper()) { }
        /// <summary>
        /// Constructs an instance of an <see cref="IPCClient"/> using a custom <see cref="PacketTyper"/>.
        /// </summary>
        /// <param name="pipeName">The pipe/server name this instance will connect to.</param>
        /// <param name="typer">An instance of a custom <see cref="PacketTyper"/> that this connection will use for parsing incoming <see cref="Packet"/>s.</param>
        public IPCClient(string pipeName, PacketTyper typer)
        {
            this.PacketConstructor = new PacketConstructor(typer);
            this.PipeName = pipeName.Replace(" ", "_");
        }

        public override void Start()
        {

            this.Thread = new Thread(new ThreadStart(this.RunThread))
            {
                Name = $"Akiyama.IPC PipeClient Thread: {this.PipeName}",
                IsBackground = false
            };
            this.IsRunning = true;
            this.Thread.Start();
        }


#if DEBUG
        public override void Log(string str)
        {
            Debug.WriteLine($"[Client] {str}");
        }
#endif

    }
}
