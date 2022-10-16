using TeltonikaDataServer.Services;
using TeltonikaDataServer.Models;
using Dapper;
using Microsoft.Extensions.Caching.Memory;

namespace TeltonikaDataServer;

public class TelematicsDataRepository
{
    private readonly Database _db;
    private readonly MemoryCache _cache;

    public TelematicsDataRepository(Database db, IConfiguration config)
    {
        _db = db;
        long sizeLimit = config.GetValue<long?>("DbCacheSize") ?? 1000;
        _cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = sizeLimit });
    }

    public async Task StoreGpsPosition(GpsPositionIn gpsPositionIn)
    {
        const string query = @"
        INSERT INTO telematics.gps_position (
            gps_latitude,
            gps_longitude,
            gps_altitude,
            gps_heading,
            gps_speed,
            gps_timestamp_utc,
            gps_dev_id
        ) VALUES (
            @Latitude,
            @Longitude,
            @Altitude,
            @Heading,
            @Speed,
            @TimeStampUtc,
            @DevId
        )";

        using (var conn = _db.GetConnection())
        {
            await conn.ExecuteAsync(query, gpsPositionIn);
        }
    }

    public async Task<int?> GetDeviceId(string imei)
    {
        const string query = @"
        SELECT dev_id
        FROM telematics.device
        WHERE dev_imei = @imei
        LIMIT 1";

        using (var conn = _db.GetConnection())
        {
            return await conn.QuerySingleOrDefaultAsync<int?>(query, new { imei });
        }
    }

    public async Task<int?> GetDeviceIdFromCache(string imei)
    {
        string key = $"devId_{imei}";
        return await _cache.GetOrCreateAsync<int?>(key, cacheEntry => {
            cacheEntry.Size = 1;
            return GetDeviceId(imei);
        });
    }
}
