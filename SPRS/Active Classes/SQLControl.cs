﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace SPRS
{
    public class SQLControl
    {
        public MySqlConnection SQLCon = new MySqlConnection("server=localhost;database=bookstore;uid=root;pwd=root;");

        private MySqlCommand SQLCmd;

        // DB DATA
        public MySqlDataAdapter SQLDA;
        public DataSet SQLDS;

        // QUERY PARAMETERS
        public List<MySqlParameter> Params = new List<MySqlParameter>();

        // QUERY STATISTICS
        public int RecordCount { get; private set; }
        public string Exception { get; private set; }

        public SQLControl() { }

        // ALLOW CONNECTION STRING OVERRIDE
        public SQLControl(string connectionString)
        {
            SQLCon = new MySqlConnection(connectionString);
        }

        // EXECUTE QUERY METHOD
        public void ExecQuery(string query)
        {
            // RESET QUERY STATS
            RecordCount = 0;
            Exception = "";

            try
            {
                SQLCon.Open();

                // CREATE DB COMMAND
                SQLCmd = new MySqlCommand(query, SQLCon);

                // LOAD PARAMETERS INTO COMMAND
                Params.ForEach(param => SQLCmd.Parameters.Add(param));

                // CLEAR PARAMETER LIST
                Params.Clear();

                // EXECUTE COMMAND AND FILL DATASET
                SQLDS = new DataSet();
                SQLDA = new MySqlDataAdapter(SQLCmd);
                RecordCount = SQLDA.Fill(SQLDS);
            }
            catch (Exception ex)
            {
                // CAPTURE ERRORS
                Exception = ex.Message;
            }
            finally
            {
                // CLOSE CONNECTION
                if (SQLCon.State == ConnectionState.Open)
                    SQLCon.Close();
            }
        }

        // ADD PARAMETER METHOD
        public void AddParam(string name, object value)
        {
            MySqlParameter newParam = new MySqlParameter(name, value);
            Params.Add(newParam);
        }

        // Method to get the Last Inserted ID after an INSERT operation
        public int GetLastInsertId()
        {
            int lastInsertId = -1;

            try
            {
                SQLCon.Open();

                // Query to get the last inserted ID
                string query = "SELECT LAST_INSERT_ID();";

                // Create command to execute the query
                SQLCmd = new MySqlCommand(query, SQLCon);

                // Execute the query and get the result
                lastInsertId = Convert.ToInt32(SQLCmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Exception = ex.Message;
            }
            finally
            {
                if (SQLCon.State == ConnectionState.Open)
                    SQLCon.Close();
            }

            return lastInsertId;
        }
    }
}
