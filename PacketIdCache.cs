using Microsoft.Extensions.Caching.Memory;

namespace TeltonikaDataServer;

public class PacketIdCache
{
    private readonly MemoryCache _cache;

    public PacketIdCache(long sizeLimit)
    {
        _cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = sizeLimit });
    }

    public byte? GetLatestPacketId(byte[] imei)
    {
        return _cache.Get<byte?>(Convert.ToHexString(imei));
    }

    public void SaveLatestPacketId(byte[] imei, byte packetId)
    {
        _cache.Set<byte>(Convert.ToHexString(imei), packetId, new MemoryCacheEntryOptions { Size = 1 });
    }
}
