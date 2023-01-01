using System.Data;
using System.Data.SqlClient;

namespace SqlHelper.Helpers
{
    public interface IDbConnectionFactory
    {
        public IDbConnection Create();
    }

    public class SqlDbConnectionFactory: IDbConnectionFactory
    {
        public IDbConnection Create() => new SqlConnection();
    }
}
