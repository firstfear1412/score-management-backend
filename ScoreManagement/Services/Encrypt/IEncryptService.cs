namespace ScoreManagement.Services
{
    public interface IEncryptService
    {
        bool VerifyHashedPassword(string hashedPassword, string providedPassword);
        string EncryptPassword(string password);
    }
}
