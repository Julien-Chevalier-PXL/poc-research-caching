namespace pocCachingApi.BusinessLayer.SQLiteLayer
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Data.Sqlite;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using pocCachingApi.BusinessLayer.SQLiteLayer.Models;

    public class SqliteService
    {
        private SqliteConnection _conn;

        public SqliteService()
        {
            _conn = new SqliteConnection("Data Source=C:\\PocCaching.db;");
        }

        public async Task<SqliteResponse<VisuCalcul>> GetVisuCalculByVisuId(string id)
        {
            var response = new SqliteResponse<VisuCalcul>();
            string statment = $"SELECT * FROM visuCalcul WHERE visu_id LIKE '{id}'";

            try
            {
                _conn.Open();
                SqliteCommand command = new SqliteCommand(statment, _conn);
                var result = await command.ExecuteReaderAsync();
                if (result.HasRows)
                {
                    result.Read();
                    response.Result = new VisuCalcul
                    {
                        Id = result.GetInt32(0),
                        Visu_Id = result.GetString(1),
                        Calc_Sum = result.GetBoolean(2),
                        Calc_Average = result.GetBoolean(3)
                    };
                }
                else
                {
                    response.Message = $"No visualization found for the id {id}";
                }

                response.isValid = true;
                return response;
            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] SqliteService.GetVisuCalculByVisuId has thrown an exception:");
                Console.WriteLine(e);
                response.Message = "An error has occured during the SELECT on Sqlite";
                return response;
            }
            finally
            {
                _conn.Close();
            }
        }
    }
}