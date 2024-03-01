namespace Akiyama.IPC.Shared.Network
{
    public enum PacketType
    {

        STRING = 1,
        INT = 2,
        SHORT = 3,
        LONG = 4,
        UINT = 5,
        USHORT = 6,
        ULONG = 7,
        GENERIC_DATA = 8,
        GZIP = 100,
        TEST_PACKET = 255,

    }
}
