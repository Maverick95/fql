using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlHelper.Helpers
{
    public interface IDbCommandFactory
    {
        public IDbCommand Create();
    }

    public class SqlDbTextCommandFactory: IDbCommandFactory
    {
        private readonly int _timeout;

        public SqlDbTextCommandFactory(int timeout)
        {
            _timeout = timeout;
        }
        public IDbCommand Create()
        {
            var command = new SqlCommand();
            command.CommandType = CommandType.Text;
            command.CommandTimeout = _timeout;
            return command;
        }
    }
}
