using System;

namespace ImpruvIT.BatteryMonitor
{
    public class CommunicationException : Exception
    {
        public CommunicationException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}
