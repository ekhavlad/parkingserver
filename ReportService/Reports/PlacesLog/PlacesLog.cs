using System;
using System.Runtime.Serialization;

namespace Hermes.Parking.Server.Reports.PlacesLog
{
    public class PlacesLog
    {
        public long Id;
        public int ZoneId;
        public int PlacesCount;
        public int PlacesOccupied;
        public DateTime InsertStamp;
    }
}
