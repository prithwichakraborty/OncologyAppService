using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using System.Data.Sql;
using System.Data.SqlClient;
namespace OncologyAppService
{
    public class Global
    {

        public Global()
        { }

        

        #region Constants

        public static string CONN_STRING = WebConfigurationManager.ConnectionStrings["DBConnString"].ConnectionString;
        public static string QUERY_EXECUTION_RESP = "";

        #endregion

    }
}