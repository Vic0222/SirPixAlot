using Libsql.Client;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SirPixAlot.Core.Infrastructure
{
    public interface ISqliteConnectionProvider
    {
        SqliteConnection GetConnection();
    }

    public class SqliteConnectionProvider(string connectionString) : ISqliteConnectionProvider
    {
        public SqliteConnection GetConnection()
        {
            return new SqliteConnection(connectionString);
        }
    }
}
