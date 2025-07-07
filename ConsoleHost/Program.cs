using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

using Hermes.Core.Interfaces;
using Hermes.Core.Services;

using Hermes.Parking.Server.DataService;
using Hermes.Parking.Server.FinanceService;
using Hermes.Parking.Server.RequestProcessor;
using Hermes.Parking.Server.Session;
using Hermes.Parking.Server.OneUseTicket;
using Hermes.Parking.Server.ServerAPI;
using Hermes.Parking.Server.EventService;
using Hermes.Parking.Server.ServerDictionary;
using Hermes.Parking.Server.SerializationService;
using Hermes.Parking.Server.Equipment;
using Hermes.Parking.Server.ReportService;
using Hermes.Parking.Server.Reports.PlacesLog;
using Hermes.Parking.Server.Reports.PassageLog;

using System.ComponentModel;
using System.ServiceModel;
using System.ServiceProcess;
using System.Configuration;
using System.Configuration.Install;

using System.Diagnostics.Contracts;

namespace Microsoft.ServiceModel.Samples
{
    public class TestWindowsService : ServiceBase
    {
        public IContext Context;
        public ILogger Logger;

        static void Main(string[] args)
        {
            TestWindowsService t = new TestWindowsService();
            t.OnStart(null);
        }

        
        protected override void OnStart(string[] args)
        {
            //ServiceBase.Run(new TestWindowsService());

            Context = new Context();

            Logger = new Logger();
            Context.AddService((IService)Logger);

            EventService eventService = new EventService();
            Context.AddService(eventService);
            eventService.OnEvent += es_OnEvent;

            DataBaseService dbService = new DataBaseService();
            Context.AddService(dbService);

            DataService dataService = new DataService();
            Context.AddService(dataService);

            FinanceService financeService = new FinanceService();
            Context.AddService(financeService);

            PrimaryRequestProcessor requestProcessor = new PrimaryRequestProcessor();
            Context.AddService(requestProcessor);

            SessionManager sessionManager = new SessionManager();
            Context.AddService(sessionManager);

            ServerDictionaryManager dictionaryManager = new ServerDictionaryManager();
            Context.AddService(dictionaryManager);
            
            EquipmentManager equipmentManager = new EquipmentManager();
            Context.AddService(equipmentManager);

            ReportManager reportManager = new ReportManager();
            Context.AddService(reportManager);

            SerializationService serializationService = new SerializationService();
            Context.AddService(serializationService);

            SecurityManager securityManager = new SecurityManager();
            Context.AddService(securityManager);

            RequestLogManager requestLogManager = new RequestLogManager();
            Context.AddService(requestLogManager);

            ServerAPIHost host = new ServerAPIHost();
            Context.AddService(host);

            ((Context)Context).Create();

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            OneUseTicketNumberGenerator numberGenerator = new OneUseTicketNumberGenerator(Context);
            sessionManager.AddTicketNumberGenerator(numberGenerator);

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            BaseDebtCalculator calculator = new BaseDebtCalculator();
            financeService.AddDebtCalculator(RateType.Base, calculator);

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            ServerDictionaryDataProvider sddp = new ServerDictionaryDataProvider(Context);
            dataService.AddDataProvider("ServerDictionary", sddp);

            ReportDataProvider reportDp = new ReportDataProvider(Context);
            dataService.AddDataProvider("Report", reportDp);

            SecurityDataProvider seqdp = new SecurityDataProvider(Context);
            dataService.AddDataProvider("Operation", seqdp);
            dataService.AddDataProvider("UserRole", seqdp);
            dataService.AddDataProvider("User", seqdp);

            RequestLogDataProvider rldp = new RequestLogDataProvider(Context);
            dataService.AddDataProvider("RequestLog", rldp);

            EquipmentDataProvider eqdp = new EquipmentDataProvider(Context);
            dataService.AddDataProvider("Gate", eqdp);
            dataService.AddDataProvider("Zone", eqdp);
            dataService.AddDataProvider("Device", eqdp);
            dataService.AddDataProvider("DeviceState", eqdp);
            dataService.AddDataProvider("DeviceEvent", eqdp);

            SessionDataProvider sdp = new SessionDataProvider(Context);
            dataService.AddDataProvider("Session", sdp);
            dataService.AddDataProvider("SessionBill", sdp);

            FinanceDataProvider fdp = new FinanceDataProvider(Context);
            dataService.AddDataProvider("Rate", fdp);
            dataService.AddDataProvider("Payment", fdp);
            dataService.AddDataProvider("Bill", fdp);

            OneUseTicketDataProvider outdp = new OneUseTicketDataProvider(Context);
            dataService.AddDataProvider("OneUseTicketNumber", outdp);

            PlacesLogDataProvider pldp = new PlacesLogDataProvider(Context);
            dataService.AddDataProvider("PlacesLog", pldp);

            PassageLogDataProvider passLogDp = new PassageLogDataProvider(Context);
            dataService.AddDataProvider("PassageLog", passLogDp);

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            ServerDictionaryRequestProcessor sdrp = new ServerDictionaryRequestProcessor(Context);
            requestProcessor.AddRequestProcessor("GetAllServerDictionaries", sdrp);

            SecurityRequestProcessor seqrp = new SecurityRequestProcessor(Context);
            requestProcessor.AddRequestProcessor("GetAllUsers", seqrp);
            requestProcessor.AddRequestProcessor("CreateUser", seqrp);
            requestProcessor.AddRequestProcessor("UpdateUser", seqrp);
            requestProcessor.AddRequestProcessor("DeleteUser", seqrp);
            requestProcessor.AddRequestProcessor("GetAllUserRoles", seqrp);
            requestProcessor.AddRequestProcessor("UpdateUserRole", seqrp);
            requestProcessor.AddRequestProcessor("GetAllRequests", seqrp);
            requestProcessor.AddRequestProcessor("GetAvailableRequests", seqrp);
            requestProcessor.AddRequestProcessor("SetPassword", seqrp);
            requestProcessor.AddRequestProcessor("ChangePassword", seqrp);
            requestProcessor.AddRequestProcessor("CreateUserRole", seqrp);
            requestProcessor.AddRequestProcessor("DeleteUserRole", seqrp);

            FinanceServiceRequestProcessor fsrp = new FinanceServiceRequestProcessor(Context);
            requestProcessor.AddRequestProcessor("GetAllRates", fsrp);
            requestProcessor.AddRequestProcessor("CreateRate", fsrp);
            requestProcessor.AddRequestProcessor("UpdateRate", fsrp);
            requestProcessor.AddRequestProcessor("DeleteRate", fsrp);
            requestProcessor.AddRequestProcessor("GetPayments", fsrp);
            requestProcessor.AddRequestProcessor("CreatePayment", fsrp);

            EquipmentRequestProcessor erp = new EquipmentRequestProcessor(Context);
            requestProcessor.AddRequestProcessor("RegisterDevice", erp);
            requestProcessor.AddRequestProcessor("UpdateDevice", erp);
            requestProcessor.AddRequestProcessor("DeleteDevice", erp);
            requestProcessor.AddRequestProcessor("GetAllDevices", erp);

            requestProcessor.AddRequestProcessor("AddDevicesStates", erp);
            requestProcessor.AddRequestProcessor("GetLastDevicesStates", erp);

            requestProcessor.AddRequestProcessor("AddDevicesEvents", erp);
            requestProcessor.AddRequestProcessor("GetDevicesEvents", erp);

            RequestLogRequestProcessor rlrp = new RequestLogRequestProcessor(Context);
            requestProcessor.AddRequestProcessor("GetRequestLogs", rlrp);

            ReportRequestProcessor reportRp = new ReportRequestProcessor(Context);
            requestProcessor.AddRequestProcessor("CreateReport", reportRp);
            requestProcessor.AddRequestProcessor("GetAllReports", reportRp);
            requestProcessor.AddRequestProcessor("UpdateReport", reportRp);
            requestProcessor.AddRequestProcessor("DeleteReport", reportRp);

            OneUseTicketRequestProcessor outRp = new OneUseTicketRequestProcessor(Context);
            requestProcessor.AddRequestProcessor("GetNewOneUseTicket", outRp);
            requestProcessor.AddRequestProcessor("Arrive", outRp);
            requestProcessor.AddRequestProcessor("Leave", outRp);
            requestProcessor.AddRequestProcessor("Depart", outRp);
            requestProcessor.AddRequestProcessor("AskForDepart", outRp);

            SessionRequestProcessor srp = new SessionRequestProcessor(Context);
            requestProcessor.AddRequestProcessor("GetSessions", srp);
            requestProcessor.AddRequestProcessor("SetSessionGraceTime", srp);
            requestProcessor.AddRequestProcessor("RefreshSessionGracePeriod", srp);
            requestProcessor.AddRequestProcessor("Arrive", srp);
            requestProcessor.AddRequestProcessor("Leave", srp);
            requestProcessor.AddRequestProcessor("Depart", srp);
            requestProcessor.AddRequestProcessor("AskForDepart", srp);
            requestProcessor.AddRequestProcessor("CloseSession", srp);

            PlacesLogRequestProcessor plrp = new PlacesLogRequestProcessor(Context);
            requestProcessor.AddRequestProcessor("Arrive", plrp);
            requestProcessor.AddRequestProcessor("Depart", plrp);
            requestProcessor.AddRequestProcessor("CorrectPlaces", plrp);
            requestProcessor.AddRequestProcessor("GetPlacesLogReport", plrp);

            PassageLogRequestProcessor passLogRp = new PassageLogRequestProcessor(Context);
            requestProcessor.AddRequestProcessor("Arrive", passLogRp);
            requestProcessor.AddRequestProcessor("Depart", passLogRp);
            requestProcessor.AddRequestProcessor("GetPassageLogReport", passLogRp);

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            ((Context)Context).Start();

            Console.ReadLine();
            ((Context)Context).Stop();
        }

        void es_OnEvent(string EventName, IDictionary<string, object> Data)
        {
            ISerializationService ss = Context.GetService<ISerializationService>(Hermes.Parking.Server.SerializationService.Constants.SERIALIZATION_SERVICE_NAME);
            Console.WriteLine("Event '{0}': [{1}]", EventName, ss.Serialize(Data));
        }

        protected override void OnStop()
        {
        }
    }
}
