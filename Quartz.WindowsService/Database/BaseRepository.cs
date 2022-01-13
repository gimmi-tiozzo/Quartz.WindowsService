using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Quartz.WindowsService.Database
{
    /// <summary>
    /// Classe base per l'accesso al Database
    /// </summary>
    public class BaseRepository
    {
        /// <summary>
        /// verifica se il campo è nullo, e converti eventualmente nel suo tipo o nel suo default
        /// </summary>
        /// <param name="val">Oggetto da verificare</param>
        /// <returns>Risultato della conversione</returns>
        protected string IsDBNullValueForString(object val)
        {
            return val is DBNull || val == null ? null : Convert.ToString(val);
        }

        /// <summary>
        /// Ottieni una connessione al DB QuatzSample
        /// </summary>
        /// <returns>Connessione al DB QuatzSample</returns>
        protected SqlConnection GetConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["QuartzDatabase"].ConnectionString);
        }

        /// <summary>
        /// Ottieni il comando per la chiamata di una store procedure
        /// </summary>
        /// <param name="storeprocedure">nome store procedure</param>
        /// <param name="connection">Connessione al database</param>
        /// <returns>Comando per la chiamata alla store procedure</returns>
        protected SqlCommand GetCommand(string storeprocedure, SqlConnection connection)
        {
            SqlCommand cmd = new SqlCommand(storeprocedure, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            return cmd;
        }
    }
}
