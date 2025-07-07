using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hermes.Core.Interfaces;
using Hermes.Parking.Server.Session;
using Hermes.Parking.Server.DataService;

namespace Hermes.Parking.Server.OneUseTicket
{
    public class OneUseTicketNumberGenerator : ITicketNumberGenerator
    {
        private IDataService dataService;

        public OneUseTicketNumberGenerator(IContext Context)
        {
            this.dataService = Context.GetService<IDataService>(Hermes.Parking.Server.DataService.Constants.DATA_SERVICE_NAME);
        }

        public TicketType TicketType { get { return Session.TicketType.OneUseTicket; } }

        public long GenerateNumber()
        {
            return dataService.Get<long>(Constants.ONE_USE_TICKET_NUMBER_OBJECT_TYPE_NAME, null);
        }
    }
}
