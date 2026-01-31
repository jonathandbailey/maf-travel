namespace Travel.Application.Exceptions;

public class FlightSearchException : Exception
{
    public FlightSearchException(string message) : base(message)
    {
    }

    public FlightSearchException(string message, Exception innerException) : base(message, innerException)
    {
    }
}