namespace DoxmandAPI
{
    public interface IJwtAuthenticationManager
    {
        string Authenticate(string email, string password);
    }
}