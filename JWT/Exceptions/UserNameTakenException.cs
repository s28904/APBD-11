namespace JWT.Exceptions;

public class UserNameTakenException(string message) : Exception(message);