using System;
using System.Runtime.Serialization;

namespace Hermes.Parking.Server.ReportService
{
    public class Report
    {
        public int Id;
        public string Name;
        public string Description;
        public string ReportXML;

        public override string ToString()
        {
            return string.Format("Id:{0}/Name:{1}/Description:{2}", Id, Name, Description);
        }
    }
}
