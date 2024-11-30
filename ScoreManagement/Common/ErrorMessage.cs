using Microsoft.Data.SqlClient;
using ScoreManagement.Model.Table.WebEvent;
using Serilog;
using System.Text;

namespace ScoreManagement.Common
{
    public class ErrorMessage
    {
        private List<string> _errMsgs;
        private StringBuilder _errTxt;
        private bool _IsError { get; set; }
        private bool _manualAddError = false;

        public ErrorMessage()
        {
            //_errMsgs = new List<string>();
            _errMsgs = _errMsgs == null ? new List<string>() : _errMsgs;
        }

        //public ErrorMessage()
        //{
        //    _errMsgs = _errMsgs == null ? new List<string>() : _errMsgs;
        //}

        //public List<string> ErrorMessages { get { return _errMsgs; } set { _errMsgs = value; } }
        public List<string> ErrorMessages
        {
            get { return _errMsgs; }
            set { _errMsgs = value; }
        }

        public bool IsError
        {
            get
            {
                return _IsError;  // Return the value of the private isError field
            }
        }
        // Add message directly (old way)

        public void Add(string message, bool isError = true)
        {
            _IsError = isError;
            _errMsgs.Add(message);
            //SetErrorText(); // Rebuild the ErrorText when a new message is added
        }

        // Add multiple messages from another ErrorMessage object
        public void Add(ErrorMessage errorMsgs)
        {
            errorMsgs.ErrorMessages.ForEach(c => _errMsgs.Add(c));
            //SetErrorText(); // Rebuild ErrorText after adding messages
        }

        // Method to add messages from StatusCode and store both English and Thai messages
        //public void AddMessageFromStatusCode(int code)
        //{
        //    var statusMessage = GetStatusMessageByCode(code);

        //    if (statusMessage != null)
        //    {
        //        // Add both English and Thai descriptions to the error list
        //        _errMsgs.Add(statusMessage.Description_En);
        //        _errMsgs.Add(statusMessage.Description_Th);

        //        bool isErrorFromDb = statusMessage.Type.ToUpper() == "ERROR";
        //        _IsError = isErrorFromDb;
        //    }
        //}

        // Return the combined error text
        public string ErrorText
        {
            get
            {
                if (_errTxt == null)
                    _errTxt = new StringBuilder();

                if (!_manualAddError) // ถ้ายังไม่มีการเพิ่มแบบ manual ให้ใช้ SetErrorText()
                {
                    SetErrorText();
                }
                return _errTxt.ToString();
            }
            set
            {
                if (_errTxt == null)
                    _errTxt = new StringBuilder();

                _errTxt.Clear();
                _errTxt.Append(value);
                _manualAddError = false; // รีเซ็ต flag เมื่อมีการกำหนดค่าใหม่
            }
        }

        // Override ToString to return ErrorText
        public override string ToString()
        {
            return _errTxt.ToString();
        }

        // Build the combined error text from the list of messages
        private void SetErrorText()
        {
            if (_manualAddError) return; // ไม่ลบ _errTxt ถ้ามีการเพิ่ม error แบบ manual แล้ว
            _errTxt = new StringBuilder();
            if (_errMsgs.Count != 0)
            {
                var last = _errMsgs.Last();
                _errMsgs.ForEach(c =>
                {
                    if (c.Equals(last))
                    {
                        _errTxt.AppendFormat("{0}", c);
                    }
                    else
                    {
                        _errTxt.AppendFormat("{0},", c);
                    }
                });
            }
        }

        public string AddError(string additionalError)
        {
            _manualAddError = true; // ตั้ง flag ว่ามีการเพิ่ม error แบบ manual แล้ว
            if (_errTxt.Length > 0)
            {
                _errTxt.Append(","); // เพิ่ม comma ถ้าค่ามีอยู่แล้ว
            }
            return _errTxt.Append(additionalError).ToString(); ;
        }


        // This is a placeholder for actual data retrieval logic from StatusCodeMessagesSystem
        //private MasterMessageSystem GetStatusMessageByCode(int code)
        //{
        //    MasterMessageSystem statusMessage = null;

        //    try
        //    {
        //        // สร้างการเชื่อมต่อฐานข้อมูลจาก connection string
        //        var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
        //        IConfigurationRoot configuration = builder.Build();

        //        string connectionString = configuration.GetConnectionString("FMISDb");

        //        using (SqlConnection connection = new SqlConnection(connectionString))
        //        {
        //            // เปิดการเชื่อมต่อ
        //            connection.Open();

        //            // คำสั่ง SQL เพื่อดึงข้อมูลตาม Code
        //            string query = "SELECT Code, Type, Description_En, Description_Th FROM MasterMessageSystem WHERE Code = @Code";

        //            // สร้างคำสั่ง SQL
        //            using (SqlCommand command = new SqlCommand(query, connection))
        //            {
        //                // กำหนดพารามิเตอร์สำหรับคำสั่ง SQL
        //                command.Parameters.AddWithValue("@Code", code);

        //                // ทำการอ่านข้อมูล
        //                using (SqlDataReader reader = command.ExecuteReader())
        //                {
        //                    // ถ้ามีข้อมูลที่ดึงออกมา
        //                    if (reader.Read())
        //                    {
        //                        int col = 0;
        //                        int colNull = 0;
        //                        statusMessage = new MasterMessageSystem();

        //                        statusMessage.Code = !reader.IsDBNull(colNull++) ? reader.GetInt32(col) : default; ; col++;
        //                        statusMessage.Type = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; ; col++;
        //                        statusMessage.Description_En = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; ; col++;
        //                        statusMessage.Description_Th = !reader.IsDBNull(colNull++) ? reader.GetString(col) : default; ; col++;
        //                        colNull = 0;
        //                    }
        //                }
        //            }
        //            connection.Close();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }

        //    return statusMessage;
        //}



        //public void WritelogInfo(string user, string msg, HttpContext httpContext)
        //{

        //    WebEvent_Logs md = new WebEvent_Logs();
        //    md.event_type = string.Format("{0} : {1}", httpContext.Request.Method, httpContext.Request.Path);
        //    //md.event_detail_code = string.Format("Exception StackTrace : {0}", ex.StackTrace);
        //    md.message = string.Format("{0}", msg.Trim());
        //    //md.details = string.Format("Exception Message : {0}", ex.Message);
        //    md.details = "";
        //    md.machine_name = httpContext.Request.Host.Value;
        //    md.request_url = string.Format("{0}{1}", httpContext.Request.Host.Value, httpContext.Request.Path);
        //    md.application_path = httpContext.Request.Path;
        //    md.user_id = string.IsNullOrEmpty(user) ? "API" : user;
        //    md.event_code = httpContext.GetEndpoint().DisplayName;
        //    var remoteIpAddress = httpContext.Connection.RemoteIpAddress;
        //    md.ip_address = remoteIpAddress != null && remoteIpAddress.Equals(System.Net.IPAddress.IPv6Loopback)
        //                    ? "127.0.0.1"
        //                    : remoteIpAddress?.ToString();
        //    //md.ip_address = httpContext.
        //    Log.Error(string.Format("application_path : {0} message : {1}", md.application_path, msg.Trim()));
        //    InsertIntoWriteLog(md);


        //}

        public void WriteLog(string user, string msg, Exception ex, HttpContext httpContext)
        {
            WebEvent_Logs md = new WebEvent_Logs();
            md.event_type = string.Format("{0} : {1}", httpContext.Request.Method, httpContext.Request.Path);
            md.event_detail_code = string.Format("Exception StackTrace : {0}", ex.StackTrace);
            md.message = string.Format("{0}", msg.Trim());
            md.details = string.Format("Exception Message : {0}", ex.Message);
            md.machine_name = httpContext.Request.Host.Value;
            md.request_url = string.Format("{0}{1}", httpContext.Request.Host.Value, httpContext.Request.Path);
            md.application_path = httpContext.Request.Path;
            md.user_id = string.IsNullOrEmpty(user) ? "API" : user;
            md.event_code = httpContext.GetEndpoint().DisplayName;
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
                string connectionString = configuration.GetConnectionString("demoDB");
                using (SqlConnection mConnection = new SqlConnection(connectionString))
                {
                    bool flg = false;
                    mConnection.Open();
                    using (var transaction = mConnection.BeginTransaction())
                    {
                        StringBuilder sbSQL = new StringBuilder();
                        sbSQL.Append("INSERT INTO [dbo].[WebEvent_Logs] ");
                        sbSQL.Append("           ,[event_time] ");
                        sbSQL.Append("           ,[event_type] ");
                        sbSQL.Append("           ,[event_code] ");
                        sbSQL.Append("           ,[event_detail_code] ");
                        sbSQL.Append("           ,[message] ");
                        sbSQL.Append("           ,[application_path] ");
                        sbSQL.Append("           ,[machine_name] ");
                        sbSQL.Append("           ,[ip_address] ");
                        sbSQL.Append("           ,[request_url] ");
                        sbSQL.Append("           ,[exception_type] ");
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
                        sbSQL.Append("@exception_type, ");
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
