namespace JWT.Exceptions;

public class InvalidRefreshTokenException(string message) : Exception(message);