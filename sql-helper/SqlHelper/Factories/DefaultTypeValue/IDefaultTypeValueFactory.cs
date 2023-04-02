namespace SqlHelper.Factories.DefaultTypeValue
{
    public interface IDefaultTypeValueFactory
    {
        public string Create(string type);
    }

    public class FirstDefaultTypeValueFactory : IDefaultTypeValueFactory
    {
        private readonly Dictionary<string, string> _default_values = new()
        {
            {   "uniqueidentifier"      ,       "'00000000-0000-0000-0000-000000000000'"    },
            {   "nvarchar"              ,       "N''"                                       },
            {   "varchar"               ,       "''"                                        },
            {   "int"                   ,       "0"                                         },
            {   "datetime2"             ,       "'0000-00-00 00:00:00'"                     },
            {   "numeric"               ,       "0.0"                                       },
            {   "char"                  ,       "'0'"                                       },
            {   "bit"                   ,       "0"                                         },
            {   "date"                  ,       "'0000-00-00'"                              },
            {   "tinyint"               ,       "0"                                         },
            {   "decimal"               ,       "0.0"                                       },
        };

        public string Create(string type)
        {
            if (_default_values.TryGetValue(type, out var return_value))
            {
                return return_value;
            }

            return "0";
        }
    }
}
