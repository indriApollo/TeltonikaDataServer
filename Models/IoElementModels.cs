namespace TeltonikaDataServer.Models;

public enum FmbIoElementId : byte
{
    IGNITION = 239,
    MOVEMENT = 240,
    GSM_SIGNAL = 21,
    SLEEP_MODE = 200,
    GNSS_STATUS = 69,
    GNSS_HDOP = 182,
    EXTERNAL_VOLTAGE = 66,
    SPEED = 24,
    TRIP_ODOMETER = 199,
    TOTAL_ODOMETER = 16,
}

public enum IgnitionState : byte
{
    OFF = 0,
    ON = 1
}

public enum MovementSate : byte
{
    OFF = 0,
    ON = 1
}
