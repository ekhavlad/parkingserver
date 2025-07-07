using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes.Parking.Server.Session;

namespace Hermes.Parking.Server.OneUseTicket
{
    public class OneUseTicket : IOneUseTicket
    {
        private long id;
        public long Id { get { return id; } }

        private ISession session;
        public ISession Session { get { return session; } }

        private DateTime createTime;
        public DateTime CreateTime { get { return createTime; } }

        public OneUseTicket(long Id, DateTime CreateTime, ISession Session)
        {
            this.id = Id;
            this.session = Session;
            this.createTime = CreateTime;
        }
    }
}
