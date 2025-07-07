using System;
using System.Collections.Generic;
using System.Linq;
using Hermes.Parking.Server;
using Hermes.Core.Interfaces;
using Hermes.Parking.Server.DataService;
using System.Data;
using Hermes.Parking.Server.EventService;
using Hermes.Parking.Server.RequestProcessor;

namespace Hermes.Parking.Server.ReportService
{
    public class ReportManager : BaseService, IReportManager
    {
        private IDataService dataService;
        private IEventService eventService;

        public override string GetName() { return Constants.REPORT_MANAGER_NAME; }

        public override void OnCreate(IContext Context)
        {
            base.OnCreate(Context);
            this.dataService = Context.GetService<IDataService>(DataService.Constants.DATA_SERVICE_NAME);
            this.eventService = Context.GetService<IEventService>(Hermes.Parking.Server.EventService.Constants.EVENT_SERVICE_NAME);
        }

        public Report CreateReport(Report Report)
        {
            try
            {
                Logger.DebugFormat("Создаем отчет {0}", Report);

                Report result = dataService.Create<Report>(Constants.REPORT_OBJECT_TYPE_NAME, Report);

                Logger.InfoFormat("Создан новый отчет {0}", result);

                Dictionary<string, object> eventData = new Dictionary<string, object>();
                eventData["Report"] = result;
                eventService.EvokeEvent(Constants.REPORT_CREATED_EVENT_NAME, eventData);

                return result;
            }
            catch (ServerDefinedException ex1)
            {
                Logger.WarnFormat("Ошибка при создании отчета: {0}", ex1.Message);
                throw ex1;
            }
            catch (Exception ex2)
            {
                Logger.ErrorFormat("Ошибка при создании отчета", ex2);
                throw new Exception("Ошибка при создании отчета");
            }
        }

        public IEnumerable<Report> GetAllReports()
        {
            return dataService.GetList<Report>(Constants.REPORT_OBJECT_TYPE_NAME, null);
        }

        public void SaveReport(Report Report)
        {
            try
            {
                Logger.DebugFormat("Сохраняем отчет {0}", Report);

                dataService.Save(Constants.REPORT_OBJECT_TYPE_NAME, Report);

                Logger.InfoFormat("Отчет сохранен {0}", Report);

                Dictionary<string, object> eventData = new Dictionary<string, object>();
                eventData["Report"] = Report;
                eventService.EvokeEvent(Constants.REPORT_CHANGED_EVENT_NAME, eventData);
            }
            catch (ServerDefinedException ex1)
            {
                Logger.WarnFormat("Ошибка при сохранении отчета: {0}", ex1.Message);
                throw ex1;
            }
            catch (Exception ex2)
            {
                Logger.ErrorFormat("Ошибка при сохранении отчета", ex2);
                throw new Exception("Ошибка при сохранении отчета");
            }
        }

        public void DeleteReport(Report Report)
        {
            try
            {
                Logger.DebugFormat("Удаляем отчет {0}", Report);
                dataService.Delete(Constants.REPORT_OBJECT_TYPE_NAME, Report);
                Logger.InfoFormat("Отчет {0} удален", Report.Id);

                Dictionary<string, object> eventData = new Dictionary<string, object>();
                eventData["ReportId"] = Report.Id;
                eventService.EvokeEvent(Constants.REPORT_DELETED_EVENT_NAME, eventData);
            }
            catch (Exception ex2)
            {
                Logger.ErrorFormat("Ошибка при удалении отчета", ex2);
                throw new ServerDefinedException("Ошибка при удалении отчета");
            }
        }
    }
}
