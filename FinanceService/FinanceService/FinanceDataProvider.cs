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

namespace Hermes.Parking.Server.FinanceService
{
    public partial class FinanceDataProvider : BaseDataProvider
    {
        private ILogger Logger;
        private IDataBaseService db;

        public FinanceDataProvider(IContext Context)
        {
            this.Logger = Context.GetService<ILogger>("Logger");
            this.db = Context.GetService<IDataBaseService>(Hermes.Parking.Server.DataService.Constants.DATABASE_SERVICE_NAME);
        }

        public override string GetName()
        {
            return Constants.FINANCE_DATA_PROVIDER_NAME;
        }

        public override void Create(string ObjectType, object InitialObject, ref object Output)
        {
            switch (ObjectType)
            {
                case Constants.RATE_OBJECT_TYPE_NAME:
                    Output = CreateRate((Rate)InitialObject);
                    break;
                case Constants.PAYMENT_OBJECT_TYPE_NAME:
                    Output = CreatePayment((Payment)InitialObject);
                    break;
            }
        }

        public override void Get(string ObjectType, long Id, ref object Output)
        {
            switch (ObjectType)
            {
                case Constants.RATE_OBJECT_TYPE_NAME:
                    Output = GetRateById((int)Id);
                    break;
                case Constants.PAYMENT_OBJECT_TYPE_NAME:
                    Output = GetPaymentById((int)Id);
                    break;
            }
        }

        public override void GetList(string ObjectType, IDictionary<string, object> Filter, ref IEnumerable<object> Output)
        {
            switch (ObjectType)
            {
                case Constants.RATE_OBJECT_TYPE_NAME:
                    Output = GetRates(Filter);
                    break;

                case Constants.PAYMENT_OBJECT_TYPE_NAME:
                    Output = GetPayments(Filter);
                    break;
            }
        }

        public override void Save(string ObjectType, object Object)
        {
            switch (ObjectType)
            {
                case Constants.RATE_OBJECT_TYPE_NAME:
                    SaveRate((Rate)Object);
                    break;
            }
        }

        public override void Delete(string ObjectType, object Object)
        {
            switch (ObjectType)
            {
                case Constants.RATE_OBJECT_TYPE_NAME:
                    DeleteRate((Rate)Object);
                    break;
            }
        }
    }
}
