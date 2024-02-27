using System;

namespace Akiyama.IPC.Shared.Network
{
    public interface IPacket : IDisposable
    {
        // TODO: docstrings
        int ID { get; }
        byte[] Data { get; }
        byte[] Header { get; }

        int HeaderLength { get; }
        int DataLength { get; }
        int TotalLength { get; }

        bool AutomaticHeaderUpdatesDisabled { get; }

        void Populate();

        void UpdateHeader();

        void SetData(byte[] data);
        //void SetHeader(byte[] header);

        void SetAutomaticHeaderUpdates(bool enabled);

    }
}
