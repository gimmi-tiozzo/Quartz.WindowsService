using Quartz.WindowsService.Model;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Quartz.WindowsService.Database
{
    /// <summary>
    /// Repository per accesso al database QuartzSample
    /// </summary>
    public class BatchScheduleRepository : BaseRepository
    {
        /// <summary>
        /// Ottieni le schedulazione di un batch
        /// </summary>
        /// <param name="batchName">Nome batch</param>
        /// <param name="serverName">Server name</param>
        /// <returns>Schedulazione di un batch</returns>
        public List<BatchScheduleConfiguration> GetScheduleConfiguration(string batchName, string serverName)
        {
            List<BatchScheduleConfiguration> schedules = new List<BatchScheduleConfiguration>();

            using (SqlConnection cnn = GetConnection())
            {
                cnn.Open();
                SqlCommand cmd = GetCommand("GET_SCHEDULAZIONI", cnn);
                cmd.Parameters.Add("@nomeBatch", SqlDbType.VarChar).Value = batchName;
                cmd.Parameters.Add("@nomeServer", SqlDbType.VarChar).Value = serverName;

                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        schedules.Add(new BatchScheduleConfiguration()
                        {
                            IdRule = IsDBNullValueForString(reader["ID_REGOLA"]),
                            CronExpression = IsDBNullValueForString(reader["ESPRESSIONE_CRON"]),
                            BatchName = IsDBNullValueForString(reader["NOME_BATCH"]),
                            ServerName = IsDBNullValueForString(reader["NOME_SERVER"]),
                            ProcessPath = IsDBNullValueForString(reader["PATH_PROCESSO"]),
                            ProcessParameters = IsDBNullValueForString(reader["PARAMETRI_PROCESSO"])
                        });
                    }
                }
            }

            return schedules;
        }
    }
}
