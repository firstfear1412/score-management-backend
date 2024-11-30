namespace ScoreManagement.Services.Encrypt
{
    public interface IEncryptService
    {
        bool VerifyHashedPassword(string hashedPassword, string providedPassword);
        string Hash(string input, string algorithm);
        string EncryptPassword(string password);
    }
}
