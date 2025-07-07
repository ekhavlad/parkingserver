using System;

namespace Hermes.Parking.Server.RequestProcessor
{
    /// <summary>
    /// Формат поля неправильный - например, не удалось распарсить.
    /// </summary>
    public class InvalidFormatForInputParameterException : Exception
    {
        public InvalidFormatForInputParameterException(string InputParameterName, string Details)
            : base(string.Format("{0} ({1})", InputParameterName, Details))
        {
        }
    }
}
