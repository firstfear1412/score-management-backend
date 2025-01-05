using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ScoreManagement.Interfaces;
using ScoreManagement.Model;

namespace ScoreManagement.Query
{
    public class SystemParamQeury : ISystemParamQuery
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public SystemParamQeury(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("scoreDb")!;
        }

        public async Task<List<SystemParamResource>> GetSysbyteDesc()
        {
            const string query = @"
                                    SELECT 
                                        row_id, 
                                        byte_reference, 
                                        byte_code, 
                                        byte_desc_th, 
                                        byte_desc_en, 
                                        active_status, 
                                        create_date, 
                                        create_by, 
                                        update_date, 
                                        update_by 
                                    FROM SystemParam
                                    WHERE 1=1
                                    ORDER BY CAST(byte_code AS INT) ASC";

            var systemParams = new List<SystemParamResource>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int col = 0;

                                var param = new SystemParamResource
                                {
                                    row_id = reader.IsDBNull(col) ? 0 : reader.GetInt32(col++),
                                    byte_reference = reader.IsDBNull(col) ? null : reader.GetString(col++),
                                    byte_code = reader.IsDBNull(col) ? null : reader.GetString(col++),
                                    byte_desc_th = reader.IsDBNull(col) ? null : reader.GetString(col++),
                                    byte_desc_en = reader.IsDBNull(col) ? null : reader.GetString(col++),
                                    active_status = reader.IsDBNull(col) ? null : reader.GetString(col++),
                                    create_date = reader.IsDBNull(col) ? (DateTime?)null : reader.GetDateTime(col++),
                                    create_by = reader.IsDBNull(col) ? null : reader.GetString(col++),
                                    update_date = reader.IsDBNull(col) ? (DateTime?)null : reader.GetDateTime(col++),
                                    update_by = reader.IsDBNull(col) ? null : reader.GetString(col++),
                                };

                                systemParams.Add(param);
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

            return systemParams;
        }

        public async Task<bool> UpdateSystemParam(SystemParamResource param)
        {
            const string checkQuery = @"
                                        SELECT COUNT(*) 
                                        FROM SystemParam 
                                        WHERE (byte_desc_th = @byte_desc_th OR byte_desc_en = @byte_desc_en)
                                          AND byte_reference = @byte_reference
                                          AND byte_code != @byte_code";

            const string updateQuery = @"
                                        UPDATE SystemParam
                                        SET byte_desc_th = @byte_desc_th,
                                            byte_desc_en = @byte_desc_en,
                                            active_status = @active_status,
                                            update_date = GETDATE(),
                                            update_by = @update_by
                                        WHERE byte_code = @byte_code
          AND byte_reference = @byte_reference";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Check for duplicate values within the same byte_reference
                    using (var checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@byte_desc_th", param.byte_desc_th ?? (object)DBNull.Value);
                        checkCommand.Parameters.AddWithValue("@byte_desc_en", param.byte_desc_en ?? (object)DBNull.Value);
                        checkCommand.Parameters.AddWithValue("@byte_code", param.byte_code);
                        checkCommand.Parameters.AddWithValue("@byte_reference", param.byte_reference);

                        int count = (int)await checkCommand.ExecuteScalarAsync();
                        if (count > 0)
                        {
                            return false; // Indicate that the update should not proceed due to duplicates
                        }
                    }

                    // Proceed to update if no duplicates are found
                    using (var command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@byte_code", param.byte_code);
                        command.Parameters.AddWithValue("@byte_reference", param.byte_reference);
                        command.Parameters.AddWithValue("@byte_desc_th", param.byte_desc_th ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@byte_desc_en", param.byte_desc_en ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@active_status", param.active_status ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@update_by", param.update_by ?? (object)DBNull.Value);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0; // Return true if update was successful
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating SystemParam.", ex);
            }
        }


        public async Task<(bool IsSuccess, string Message)> InsertSystemParam(SystemParamResource param)
        {
            const string insertQuery = @"
                                        IF EXISTS (SELECT 1 FROM SystemParam WHERE byte_desc_th = @byte_desc_th OR byte_desc_en = @byte_desc_en)
                                        BEGIN
                                            SELECT 0 AS IsSuccess, 'Duplicate description found.' AS Message;
                                        END
                                        ELSE IF EXISTS (SELECT 1 FROM SystemParam WHERE byte_reference = @byte_reference AND byte_code = @byte_code)
                                        BEGIN
                                            SELECT 0 AS IsSuccess, 'The byte_code already exists.' AS Message;
                                        END
                                        ELSE
                                        BEGIN
                                            DECLARE @nextByteCode INT;

                                            SELECT @nextByteCode = ISNULL(MAX(CAST(byte_code AS INT)), 0) + 1
                                            FROM SystemParam
                                            WHERE byte_reference = @byte_reference;

                                            INSERT INTO SystemParam (byte_reference, byte_code, byte_desc_th, byte_desc_en, active_status, create_date, create_by)
                                            VALUES (@byte_reference, @nextByteCode, @byte_desc_th, @byte_desc_en, 'active', GETDATE(), @create_by);

                                            SELECT 1 AS IsSuccess, 'System parameter inserted successfully.' AS Message;
                                        END";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Execute the combined query
                    using (var command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@byte_desc_th", param.byte_desc_th ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@byte_desc_en", param.byte_desc_en ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@byte_reference", param.byte_reference);
                        command.Parameters.AddWithValue("@byte_code", param.byte_code);
                        command.Parameters.AddWithValue("@create_by", param.create_by ?? (object)DBNull.Value);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                bool isSuccess = reader.GetInt32(0) == 1;
                                string message = reader.GetString(1);
                                return (isSuccess, message);
                            }
                            else
                            {
                                return (false, "Unexpected error occurred.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}");
            }
        }


    }
}