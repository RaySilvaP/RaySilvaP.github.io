namespace Backend.Exceptions;

public class AdminNotFoundException : Exception
{
    public AdminNotFoundException()
        :base("Admin not found.") {}

    public AdminNotFoundException(string message)
        :base(message) {}
}
