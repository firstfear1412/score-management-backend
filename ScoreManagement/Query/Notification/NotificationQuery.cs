using Microsoft.Data.SqlClient;
using ScoreManagement.Interfaces;
using ScoreManagement.Model;
namespace ScoreManagement.Query
{
    public class NotificationQuery : INotificationQuery
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public NotificationQuery(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("scoreDb")!;
        }

        public async Task<List<NotificationResponse<string>>> GetNotifications(string username)
        {
            var notifications = new List<NotificationResponse<string>>();
            const string query = @"
                                    SELECT TOP 50
                                        n.notification_id,
                                        n.template_id,
                                        n.data AS Data,
                                        n.create_date
                                    FROM 
                                        Notification n
                                    INNER JOIN 
                                        NotificationTemplate t 
                                    ON 
                                        n.template_id = t.template_id
                                    WHERE 
                                        n.username = @Username
                                        AND DATEDIFF(DAY, n.create_date, GETDATE()) <= 30
                                    ORDER BY create_date DESC";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int col = 0;

                                var data = new NotificationResponse<string>
                                {
                                    notificationId = reader.IsDBNull(col) ? 0 : reader.GetInt32(col++),
                                    templateId = reader.IsDBNull(col) ? 0 : reader.GetInt32(col++),
                                    data = reader.IsDBNull(col) ? null : reader.GetString(col++),
                                    createDate = reader.IsDBNull(col) ? (DateTime?)null : reader.GetDateTime(col++)
                                };

                                notifications.Add(data);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle exception as required
                throw new Exception("An error occurred while fetching SystemParam data.", ex);
            }

            return notifications;
        }
    }
}
