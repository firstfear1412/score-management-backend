using Microsoft.Data.SqlClient;
using ScoreManagement.Model.Table;
using Serilog;
using System.Text;

namespace ScoreManagement.Common
{
    public class WebEvent
    {
        //for failed
        public void WriteLogInfo(string user, string msg, HttpContext httpContext)
        {
            WebEvent_Logs md = new WebEvent_Logs();
            md.event_type = string.Format("{0} : {1}", httpContext.Request.Method, httpContext.Request.Path);
            md.message = string.Format("{0}", msg.Trim());
            md.machine_name = httpContext.Request.Host.Value;
            md.request_url = string.Format("{0}{1}", httpContext.Request.Host.Value, httpContext.Request.Path);
            md.application_path = httpContext.Request.Path;
            md.user_id = string.IsNullOrEmpty(user) ? "API" : user;
            md.event_code = httpContext.GetEndpoint()!.DisplayName;
            md.ip_address = httpContext.Connection.RemoteIpAddress?.ToString();
            Log.Error(string.Format("application_path : {0} message : {1}", md.application_path, msg.Trim()));
            InsertIntoWriteLog(md);
        }
        //for Exception
        public void WriteLogException(string user, string msg, Exception ex, HttpContext httpContext)
        {
            WebEvent_Logs md = new WebEvent_Logs();
            md.event_type = string.Format("{0} : {1}", httpContext.Request.Method, httpContext.Request.Path);
            md.message = string.Format("{0}", msg.Trim());
            //Exception message
            md.event_detail_code = string.Format("Exception StackTrace : {0}", ex.StackTrace);
            md.details = string.Format("Exception Message : {0}", ex.Message);
            md.machine_name = httpContext.Request.Host.Value;
            md.request_url = string.Format("{0}{1}", httpContext.Request.Host.Value, httpContext.Request.Path);
            md.application_path = httpContext.Request.Path;
            md.user_id = string.IsNullOrEmpty(user) ? "API" : user;
            md.event_code = httpContext.GetEndpoint()!.DisplayName;
            md.ip_address = httpContext.Connection.RemoteIpAddress?.ToString();
            Log.Error(string.Format("application_path : {0} message : {1}", md.application_path, msg.Trim()));
            InsertIntoWriteLog(md);
        }

        private async void InsertIntoWriteLog(WebEvent_Logs item)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();
            try
            {
                string connectionString = configuration.GetConnectionString("scoreDb")!;
                using (SqlConnection mConnection = new SqlConnection(connectionString))
                {
                    bool flg = false;
                    mConnection.Open();
                    using (var transaction = mConnection.BeginTransaction())
                    {
                        StringBuilder sbSQL = new StringBuilder();
                        sbSQL.Append("INSERT INTO [dbo].[WebEvent_Logs] ");
                        sbSQL.Append("           ([event_time] ");
                        sbSQL.Append("           ,[event_type] ");
                        sbSQL.Append("           ,[event_code] ");
                        sbSQL.Append("           ,[event_detail_code] ");
                        sbSQL.Append("           ,[message] ");
                        sbSQL.Append("           ,[application_path] ");
                        sbSQL.Append("           ,[machine_name] ");
                        sbSQL.Append("           ,[ip_address] ");
                        sbSQL.Append("           ,[request_url] ");
                        sbSQL.Append("           ,[details] ");
                        sbSQL.Append("           ,[user_id]) ");
                        sbSQL.Append("     VALUES ");
                        sbSQL.Append("     ( ");
                        sbSQL.Append(" ");
                        sbSQL.Append("@event_time, ");
                        sbSQL.Append("@event_type, ");
                        sbSQL.Append("@event_code, ");
                        sbSQL.Append("@event_detail_code, ");
                        sbSQL.Append("@message, ");
                        sbSQL.Append("@application_path, ");
                        sbSQL.Append("@machine_name, ");
                        sbSQL.Append("@ip_address, ");
                        sbSQL.Append("@request_url, ");
                        sbSQL.Append("@details, ");
                        sbSQL.Append("@user_id ");
                        sbSQL.Append("     ) ");
                        using (SqlCommand cmd = new SqlCommand(sbSQL.ToString(), mConnection, transaction))
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add(new SqlParameter("@event_time", item.event_time ?? (object)DateTime.Now));
                            cmd.Parameters.Add(new SqlParameter("@event_type", item.event_type ?? (object)DBNull.Value));
                            cmd.Parameters.Add(new SqlParameter("@event_code", item.event_code ?? (object)DBNull.Value));
                            cmd.Parameters.Add(new SqlParameter("@event_detail_code", item.event_detail_code ?? (object)DBNull.Value));
                            cmd.Parameters.Add(new SqlParameter("@message", item.message ?? (object)DBNull.Value));
                            cmd.Parameters.Add(new SqlParameter("@application_path", item.application_path ?? (object)DBNull.Value));
                            cmd.Parameters.Add(new SqlParameter("@machine_name", item.machine_name ?? (object)DBNull.Value));
                            cmd.Parameters.Add(new SqlParameter("@ip_address", item.ip_address ?? (object)DBNull.Value));
                            cmd.Parameters.Add(new SqlParameter("@request_url", item.request_url ?? (object)DBNull.Value));
                            cmd.Parameters.Add(new SqlParameter("@details", item.details ?? (object)DBNull.Value));
                            cmd.Parameters.Add(new SqlParameter("@user_id", item.user_id ?? (object)DBNull.Value));
                            int i = 0;
                            i = await cmd.ExecuteNonQueryAsync();
                            flg = i == 0 ? false : true;
                        }
                        if (flg)
                        {
                            transaction.Commit();
                        }
                        else
                        {
                            transaction.Rollback();
                        }
                    }
                    mConnection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
