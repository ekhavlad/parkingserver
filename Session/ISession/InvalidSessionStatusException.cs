using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermes.Parking.Server.Session
{
    public class InvalidSessionStatusException : Exception
    {
        public InvalidSessionStatusException(SessionStatus OriginalStatus, SessionStatus NewStatus)
            : base(string.Format("Cannot change session status from '{0}' to '{1}'", OriginalStatus, NewStatus))
        {
        }

        public InvalidSessionStatusException(SessionStatus Status)
            : base(string.Format("Invalid session status '{0}'", Status))
        {
        }
    }
}
