using System.Collections.Generic;

namespace Hermes.Parking.Server.ReportService
{
    public interface IReportManager
    {
        Report CreateReport(Report Report);
        IEnumerable<Report> GetAllReports();
        void SaveReport(Report Report);
        void DeleteReport(Report Report);
    }
}
