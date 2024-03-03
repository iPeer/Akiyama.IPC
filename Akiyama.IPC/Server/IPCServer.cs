using Akiyama.IPC.Shared.Network;
using Akiyama.IPC.Shared.Network.Packets;
using Akiyama.IPC.Shared.Typers;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;

namespace Akiyama.IPC.Server
{
    /// <summary>
    /// A wrapper class for an <see cref="IPCEndpoint"/> that behaves as an IPC server.
    /// </summary>
    public class IPCServer : IPCEndpoint, IDisposable
    {


        public IPCServer()
        {

            this.PipeName = this.GenerateNameHash();
            this.PacketConstructor = new PacketConstructor(new DefaultPacketTyper());
            this.IsServer = true;

        }

        /// <summary>
        /// Constructs an instance of an <see cref="IPCServer"/> using the default <see cref="PacketTyper"/>.
        /// </summary>
        /// <param name="pipeName">The pipe/server name this instance will connect to.</param>
        public IPCServer(string pipeName) : this(pipeName, new DefaultPacketTyper()) { }

        /// <summary>
        /// Constructs an instance of an <see cref="IPCServer"/> using a custom <see cref="PacketTyper"/>.
        /// </summary>
        /// <param name="pipeName">The pipe/server name this instance will connect to.</param>
        /// <param name="typer">An instance of a custom <see cref="PacketTyper"/> that this connection will use for parsing incoming <see cref="Packet"/>s.</param>
        public IPCServer(string pipeName, PacketTyper typer)
        {
            this.PacketConstructor = new PacketConstructor(typer);
            this.PipeName = pipeName.Replace(" ", "_");
            this.IsServer = true;
        }

        public override void Start()
        {

            this.Thread = new Thread(new ThreadStart(this.RunThread))
            {
                Name = $"Akiyama.IPC PipeServer Thread: {this.PipeName}",
                IsBackground = false
            };
            this.IsRunning = true;
            this.Thread.Start();

        }

        /// <summary>
        /// Generates a pseudo-random string for this <see cref="IPCEndpoint"/> to use as its pipe name, if none was provided at creation.
        /// </summary>
        /// <returns>A psuedo-random string of characters.</returns>
        public string GenerateNameHash()
        {
            // We can safely use something like MD5 here because collisions do not matter.
            using (MD5 md5 = MD5.Create())
            {
                return BitConverter.ToString(md5.ComputeHash(Guid.NewGuid().ToByteArray())).Replace("-", "").ToLower();
            }
        }

#if DEBUG
        public override void Log(string str)
        {
            Debug.WriteLine($"[Server] {str}");
        }
#endif
    }
}
