using System;

namespace Hermes.Parking.Server.RequestProcessor
{
    /// <summary>
    /// В запросе отсутствуют некоторые необходимые поля.
    /// </summary>
    public class MissedInputParameterException : Exception
    {
        public MissedInputParameterException(string InputParameterName)
            : base(InputParameterName)
        {
        }
    }
}
