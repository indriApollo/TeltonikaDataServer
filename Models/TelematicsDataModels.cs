namespace TeltonikaDataServer.Models;

public class GpsPositionIn
{
    public int Latitude { get; set; }
    public int Longitude { get; set; }
    public short Altitude { get; set; }
    public short Heading { get; set; }
    public short Speed { get; set; }
    public DateTime TimestampUtc { get; set; }
    public int DevId { get; set; }
}
