using System;
using System.Runtime.Serialization;

namespace Hermes.Parking.Server.Session
{
    public class Session : ISerializable
    {
        public long Id;

        public SessionStatus Status;

        public TicketType TicketType;
        public long TicketNumber;

        public int RateId;

        public DateTime? ArriveTime;
        public DateTime? DepartTime;
        public DateTime? CancelTime;

        public int? GateInId;
        public int? GateOutId;

        public DateTime GraceTime;

        public DateTime CreateTime;
        public DateTime LastChangeTime;


        public Session() { }

        public Session(SerializationInfo info, StreamingContext context)
        {
            foreach (SerializationEntry v in info)
                switch (v.Name)
                {
                    case "Id":
                        Id = info.GetInt64("Id"); break;

                    case "StatusId":
                        Status = (SessionStatus)info.GetInt32("StatusId"); break;

                    case "TicketType":
                        TicketType = (TicketType)info.GetInt32("TicketTypeId"); break;
                    case "TicketNumber":
                        TicketNumber = info.GetInt64("TicketNumber"); break;

                    case "RateId":
                        RateId = info.GetInt32("RateId"); break;


                    case "ArriveTime":
                        ArriveTime = info.GetDateTime("ArriveTime"); break;
                    case "DepartTime":
                        DepartTime = info.GetDateTime("DepartTime"); break;
                    case "CancelTime":
                        CancelTime = info.GetDateTime("CancelTime"); break;

                    case "GateInId":
                        GateInId = info.GetInt32("GateInId"); break;
                    case "GateOutId":
                        GateOutId = info.GetInt32("GateOutId"); break;

                    case "GraceTime":
                        GraceTime = info.GetDateTime("GraceTime"); break;

                    case "CreateTime":
                        CreateTime = info.GetDateTime("CreateTime"); break;
                    case "LastChangeTime":
                        LastChangeTime = info.GetDateTime("LastChangeTime"); break;
                }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id, typeof(long));

            info.AddValue("StatusId", (int)Status, typeof(int));

            info.AddValue("TicketTypeId", (int)TicketType, typeof(int));
            info.AddValue("TicketNumber", (long)TicketNumber, typeof(long));

            info.AddValue("RateId", RateId, typeof(int));


            info.AddValue("ArriveTime", ArriveTime, typeof(DateTime?));
            info.AddValue("DepartTime", DepartTime, typeof(DateTime?));
            info.AddValue("CancelTime", CancelTime, typeof(DateTime?));

            info.AddValue("GateInId", GateInId, typeof(int?));
            info.AddValue("GateOutId", GateOutId, typeof(int?));

            info.AddValue("GraceTime", GraceTime, typeof(DateTime));

            info.AddValue("CreateTime", CreateTime, typeof(DateTime));
            info.AddValue("LastChangeTime", LastChangeTime, typeof(DateTime));
        }

        public override string ToString()
        {
            return string.Format("Id:{0}/Status:{1}/TicketType:{2}/TicketNumber:{3}/Rate:{4}/CreateTime:{4}", Id, Status, TicketType, TicketNumber, RateId, CreateTime);
        }
    }
}
