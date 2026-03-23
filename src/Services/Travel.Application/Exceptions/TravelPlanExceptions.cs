namespace Travel.Application.Exceptions;

public class TravelPlanUpdateException(string message) : Exception(message);

public class TravelPlanQueryException(string message) : Exception(message);

public class FlightSearchUpdateException(string message) : Exception(message);

public class SessionQueryException(string message) : Exception(message);
