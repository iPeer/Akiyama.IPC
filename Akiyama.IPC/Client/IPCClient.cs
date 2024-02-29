using Akiyama.IPC.Shared.Network;
using Akiyama.IPC.Shared.Typers;
using System;
using System.Diagnostics;
using System.Threading;

namespace Akiyama.IPC.Client
{
    public class IPCClient : IPCEndpoint, IDisposable
    {

        public IPCClient(string pipeName) : this(pipeName, new DefaultPacketTyper()) { }
        public IPCClient(string pipeName, PacketTyper typer)
        {
            this.PacketConstructor = new PacketConstructor(typer);
            this.Name = pipeName.Replace(" ", "_");
        }

        public override void Start()
        {

            this.Thread = new Thread(new ThreadStart(this.RunThread))
            {
                Name = $"Akiyama.IPC PipeClient Thread: {this.Name}",
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
