using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermes.Parking.Server.RequestProcessor
{
    public enum ResultCode
    {
        Success = 0,
        UserIsNotLoggedIn = 1,
        InvalidUser = 2,
        AccessDenied = 3,
        MissedRequestData = 4,
        RequestDataFormatError = 5,
        InvalidRequestData = 6,
        InvalidSessionStatus = 7,
        UndefinedError = 8,
        InvalidOperation = 9
    }
}
