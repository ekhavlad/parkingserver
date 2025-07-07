using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes.Parking.Server.RequestProcessor;
using Hermes.Core.Interfaces;

namespace Hermes.Parking.Server.ReportService
{
    public class ReportRequestProcessor : IRequestProcessor
    {
        private IContext context;
        private ILogger Logger;
        private IReportManager reportManager;
        private Hermes.Parking.Server.SerializationService.ISerializationService serializationService;

        public ReportRequestProcessor(IContext Context)
        {
            this.context = Context;
            this.Logger = Context.GetService<ILogger>("Logger");
            this.reportManager = Context.GetService<IReportManager>(Constants.REPORT_MANAGER_NAME);
            this.serializationService = Context.GetService<Hermes.Parking.Server.SerializationService.ISerializationService>(Hermes.Parking.Server.SerializationService.Constants.SERIALIZATION_SERVICE_NAME);
        }

        public string GetName() { return Constants.REPORT_REQUEST_PROCESSOR_NAME; }

        public bool ProcessRequest(User User, Request Request, Response Response)
        {
            switch (Request.RequestName)
            {
                case "CreateReport":
                    return CreateReport(Request.Data, Response.Data);
                case "GetAllReports":
                    return GetAllReports(Request.Data, Response.Data);
                case "UpdateReport":
                    return UpdateReport(Request.Data, Response.Data);
                case "DeleteReport":
                    return DeleteReport(Request.Data, Response.Data);

                default:
                    return true;
            }
        }


        private Report ParseReport(string JSON)
        {
            try
            {
                Report result = serializationService.Deserialize<Report>(JSON);
                if (result == null)
                    throw new ServerDefinedException("Неправильный формат входных данных отчета");
                return result;
            }
            catch (ServerDefinedException ex)
            {
                Logger.WarnFormat("Не удалось распарсить отчет: {0}", ex.Message);
                throw ex;
            }
            catch (Exception ex)
            {
                Logger.WarnFormat("Не удалось распарсить отчет", ex);
                throw new ServerDefinedException("Неправильный формат входных данных отчета");
            }
        }

        public bool CreateReport(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            if (InputData == null) throw new ServerDefinedException("Входные данные пусты");
            if (!InputData.ContainsKey("Report")) throw new ServerDefinedException("Нет поля Report");
            Report report = reportManager.CreateReport(ParseReport(InputData["Report"].ToString()));
            OutputData["Report"] = serializationService.Serialize(report);
            return true;
        }

        public bool GetAllReports(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            IEnumerable<Report> reports = reportManager.GetAllReports();
            OutputData["Reports"] = serializationService.Serialize(reports);
            return true;
        }

        public bool UpdateReport(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            if (InputData == null) throw new ServerDefinedException("Входные данные пусты");
            if (!InputData.ContainsKey("Report")) throw new ServerDefinedException("Нет поля Report");
            Report report = ParseReport(InputData["Report"].ToString());
            reportManager.SaveReport(report);
            OutputData["Report"] = serializationService.Serialize(report);
            return true;
        }

        public bool DeleteReport(IDictionary<string, object> InputData, IDictionary<string, object> OutputData)
        {
            if (InputData == null) throw new ServerDefinedException("Входные данные пусты");
            if (!InputData.ContainsKey("Report")) throw new ServerDefinedException("Нет поля Report");
            Report report = ParseReport(InputData["Report"].ToString());
            reportManager.DeleteReport(report);
            return true;
        }
    }
}
