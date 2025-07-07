using System;

namespace Hermes.Parking.Server.RequestProcessor
{
    /// <summary>
    /// Некоторые необходимые поля запроса имеют некорректные значения.
    /// </summary>
    public class InvalidInputParameterException : Exception
    {
        public InvalidInputParameterException(string InputParameterName, string Details)
            : base(string.Format("{0} ({1})", InputParameterName, Details))
        {
        }
    }
}
