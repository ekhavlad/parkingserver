using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using Hermes.Core.Interfaces;
using Hermes.Parking.Server.DataService;
using System.Threading;

namespace Hermes.Parking.Server.ReportService
{
    public class ReportDataProvider : BaseDataProvider
    {
        private ILogger Logger;
        private IDataBaseService db;

        public ReportDataProvider(IContext Context)
        {
            this.Logger = Context.GetService<ILogger>("Logger");
            this.db = Context.GetService<IDataBaseService>(Hermes.Parking.Server.DataService.Constants.DATABASE_SERVICE_NAME);
        }

        public override string GetName()
        {
            return Constants.REPORT_DATA_PROVIDER_NAME;
        }

        public override void Create(string ObjectType, object InitialObject, ref object Output)
        {
            switch (ObjectType)
            {
                case Constants.REPORT_OBJECT_TYPE_NAME:
                    Output = CreateReport(InitialObject);
                    break;
            }
        }

        public override void GetList(string ObjectType, IDictionary<string, object> Filter, ref IEnumerable<object> Output)
        {
            switch (ObjectType)
            {
                case Constants.REPORT_OBJECT_TYPE_NAME:
                    Output = GetAllReports();
                    break;
            }
        }

        public override void Save(string ObjectType, object Object)
        {
            switch (ObjectType)
            {
                case Constants.REPORT_OBJECT_TYPE_NAME:
                    SaveReport((Report)Object);
                    break;
            }
        }

        public override void Delete(string ObjectType, object Object)
        {
            switch (ObjectType)
            {
                case Constants.REPORT_OBJECT_TYPE_NAME:
                    DeleteReport((Report)Object);
                    break;
            }
        }



        private Report CreateReport(object InitialObject)
        {
            Report report = (Report)InitialObject;

            SqlCommand cmd = new SqlCommand(@"INSERT INTO Report (Name, Description, ReportXML) OUTPUT inserted.* VALUES (@Name, @Description, @ReportXML);");
            cmd.Parameters.Add("@Name", SqlDbType.VarChar).Value = report.Name;
            cmd.Parameters.Add("@Description", SqlDbType.VarChar).Value = (report.Description == null) ? (object)DBNull.Value : report.Description;
            cmd.Parameters.Add("@ReportXML", SqlDbType.VarChar).Value = (report.ReportXML == null) ? (object)DBNull.Value : report.ReportXML;

            DataSet sqlResult = db.GetDataSet(cmd);
            Report result = ParseReport(sqlResult.Tables[0].Rows[0]);
            return result;
        }

        private IEnumerable<Report> GetAllReports()
        {
            SqlCommand cmd = new SqlCommand(@"SELECT * FROM Report");
            DataSet sqlResult = db.GetDataSet(cmd);

            IList<Report> result = new List<Report>();
            foreach (DataRow row in sqlResult.Tables[0].Rows)
            {
                Report zone = ParseReport(row);
                result.Add(zone);
            }

            return result;
        }

        private void SaveReport(Report Report)
        {
            SqlCommand cmd = new SqlCommand(@"UPDATE Report SET Name = @Name, Description = @Description, ReportXML = @ReportXML WHERE Id = @ReportId");
            cmd.Parameters.Add("@ReportId", SqlDbType.Int).Value = Report.Id;
            cmd.Parameters.Add("@Name", SqlDbType.VarChar).Value = Report.Name;
            cmd.Parameters.Add("@Description", SqlDbType.VarChar).Value = (Report.Description == null) ? (object)DBNull.Value : Report.Description;
            cmd.Parameters.Add("@ReportXML", SqlDbType.VarChar).Value = (Report.ReportXML == null) ? (object)DBNull.Value : Report.ReportXML;
            db.Execute(cmd);
        }

        private void DeleteReport(Report Report)
        {
            SqlCommand cmd = new SqlCommand(@"DELETE FROM Report WHERE Id = @ReportId");
            cmd.Parameters.Add("@ReportId", SqlDbType.Int).Value = Report.Id;
            db.Execute(cmd);
        }

        private Report ParseReport(DataRow Row)
        {
            int id = (int)Row["Id"];
            string name = (string)Row["Name"];
            string desc = (Row["Description"] == DBNull.Value) ? null : (string)Row["Description"];
            string xml = (Row["ReportXML"] == DBNull.Value) ? null : (string)Row["ReportXML"];

            Report result = new Report() { Id = id, Name = name, Description = desc, ReportXML = xml };
            return result;
        }

    }
}
