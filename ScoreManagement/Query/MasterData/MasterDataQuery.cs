using Microsoft.EntityFrameworkCore;
using ScoreManagement.Entity;
using ScoreManagement.Interfaces;
using ScoreManagement.Model.Table;

namespace ScoreManagement.Query
{
    public class MasterDataQuery : IMasterDataQuery
    {
        private readonly scoreDB _context;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public MasterDataQuery(IConfiguration configuration, scoreDB context)
        {
            _context = context;
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("scoreDb")!;
        }

        public Task<List<SystemParam>> GetSystemParams(string reference)
        {
            return _context.SystemParams.Where(x => x.byte_reference!.Equals(reference)
                            ).ToListAsync();
        }

        public Task<Dictionary<string,string>> GetLanguage(string language) { 
            return _context.Languages
                .Where(l => l.language_code == language)
                .ToDictionaryAsync(l => l.message_key, l => l.message_content);
        }

        public Task<List<EmailPlaceholder>> GetEmailPlaceholder()
        {
           return _context.EmailPlaceholders.Where(x => x.active_status == "active").ToListAsync();
        }
    }
}
