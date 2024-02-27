using Akiyama.IPC.Shared.Network;
using Akiyama.IPC.Shared.Typers;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;

namespace Akiyama.IPC.Server
{
    public class PipeServer : PipeEndpoint, IDisposable
    {


        public PipeServer()
        {

            this.Name = this.GenerateNameHash();
            this.PacketConstructor = new PacketConstructor(new DefaultPacketTyper());
            this.IsServer = true;

        }

        public PipeServer(string pipeName) : this(pipeName, new DefaultPacketTyper()) { }

        public PipeServer(string pipeName, PacketTyper typer)
        {
            this.PacketConstructor = new PacketConstructor(typer);
            this.Name = pipeName.Replace(" ", "_");
            this.IsServer = true;
        }

        public override void Start()
        {

            this.Thread = new Thread(new ThreadStart(this.RunThread))
            {
                Name = $"Akiyama.IPC PipeServer Thread: {this.Name}",
                IsBackground = false
            };
            this.IsRunning = true;
            this.Thread.Start();

        }

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
