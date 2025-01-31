using System.Text.RegularExpressions;

namespace ScoreManagement.Services
{
    public class UtilityService : IUtilityService
    {
        public bool IsValidEmail(string email)
        {
            string emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$"; // รูปแบบของอีเมลที่ถูกต้อง
            return Regex.IsMatch(email, emailRegex);
        }
    }
}