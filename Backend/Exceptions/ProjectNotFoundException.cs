namespace Backend.Exceptions;

public class ProjectNotFoundException : Exception
{
    public ProjectNotFoundException()
        : base("Project not found.") { }

    public ProjectNotFoundException(string message)
        : base(message) { }
}
