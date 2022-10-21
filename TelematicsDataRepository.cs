using TeltonikaDataServer.Services;
using TeltonikaDataServer.Models;
using TeltonikaDataServer.Config;
using Dapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace TeltonikaDataServer;

public class TelematicsDataRepository
{
    private readonly Database _db;
    private readonly MemoryCache _cache;

    public TelematicsDataRepository(Database db, IOptions<DatabaseOptions> options)
    {
        _db = db;
        _cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = options.Value.DbCacheSize });
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

    public async Task StoreExternalVoltage(ExternalVoltageIn externalVoltageIn)
    {
        const string query = @"
        INSERT INTO telematics.external_voltage (
            exv_value,
            exv_timestamp_utc,
            exv_dev_id
        ) VALUES (
            @Value,
            @TimestampUtc,
            @DevId
        )";

        using (var conn = _db.GetConnection())
        {
            await conn.ExecuteAsync(query, externalVoltageIn);
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
