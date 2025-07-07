using System;
using System.Runtime.Serialization;

namespace Hermes.Parking.Server.FinanceService
{
    public class Payment : ISerializable
    {
        public long Id;
        public int UserId;
        public long? SessionId;
        public int Summ;
        public DateTime Date;

        public Payment() { }

        public Payment(SerializationInfo info, StreamingContext context)
        {
            foreach (SerializationEntry v in info)
                switch (v.Name)
                {
                    case "Id":
                        Id = info.GetInt64("Id"); break;
                    case "UserId":
                        UserId = info.GetInt32("UserId"); break;
                    case "SessionId":
                        SessionId = (long?)info.GetValue("SessionId", typeof(long?)); break;
                    case "Summ":
                        Summ = info.GetInt32("Summ"); break;
                    case "Date":
                        Date = info.GetDateTime("Date"); break;
                }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id, typeof(long));
            info.AddValue("UserId", UserId, typeof(int));
            info.AddValue("SessionId", UserId, typeof(long));
            info.AddValue("Summ", Summ, typeof(int));
            info.AddValue("Date", Date, typeof(DateTime));
        }

        public override string ToString()
        {
            return string.Format("Id:{0}/SessionId:{1}/Summ:{2}/Date:{3:yyyy-MM-ddTHH:mm:ss}", Id, SessionId, Summ, Date);
        }
    }
}
