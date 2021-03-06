﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM_Skills.Scripts
{
    class MSSQL// : IDatabase
    {

        private string _prefix;
        public string Prefix { get { return _prefix; } }

        private bool _isConnected = false;
        public bool IsConnected { get { return _isConnected; } }

        private string _lastQuery;
        public string LastQuery { get { return _lastQuery; } }

        public bool UseDistinct { get; set; }

        private  SqlConnection sql_conn;
        private  SqlCommand sql_cmd;



        public void Connect(string connectionString, string prefix = "")
        {
            try
            {
                sql_conn = new SqlConnection(connectionString);
                sql_conn.Open();

                UseDistinct = false;
                sql_cmd = sql_conn.CreateCommand();

                _prefix = prefix;

                _isConnected = true;
            }
            catch (Exception)
            {
                _isConnected = false;
            }
        }

        public void Disconnect()
        {
            sql_cmd.Cancel();
            sql_conn.Close();

            _isConnected = false;
        }

        public void Drop(string table)
        {
            string tableWithPrefix = Prefix + table;

            string command = string.Format("DROP TABLE [{0}];", tableWithPrefix);
            ExecuteQuery(command);
        }

        public string EscapeString(string value)
        {
            return value.Replace("'", "");
        }

        public string BuildGetRowsCMD(string table, string[] columns, string more = "")
        {
            string[] sqlMethod = { "*", "COUNT" };

            string tableWithPrefix = Prefix + table;


            for (int i = 0; i < columns.Length; i++)
            {

                bool isMethod = false;
                foreach (string method in sqlMethod)
                {
                    if (columns[i].ToUpper().StartsWith(method))
                    {
                        isMethod = true;
                        break;
                    }
                }

                if (isMethod)
                    continue;
                if (columns[i].StartsWith("[") && columns[i].EndsWith("]"))
                    continue;

                columns[i] = "[" + columns[i] + "]";
            }

            return string.Format("SELECT{3} {1} FROM [{0}]{2};", tableWithPrefix, string.Join(", ", columns), " " + more, UseDistinct ?" DISTINCT": "");
        }

        public bool Exist(string table)
        {
            string tableWithPrefix = Prefix + table;

            string command = string.Format("SELECT * FROM information_schema.tables where [TABLE_NAME]='{0}';", tableWithPrefix);


            var result = ExecuteQuery(command);

            return result.Count > 0;
        }

        public bool Exist(string table, string[] columns, object[] values)
        {
            string tableWithPrefix = Prefix + table;

            List<string> statements = new List<string>();

            values = StringsSQLready(values);


            for (int i = 0; i < columns.Length; i++)
            {
                statements.Add(string.Format("[{0}] = {1}", columns[i], values[i]));
            }

            string statementsCMD = string.Join(" AND ", statements.ToArray());
            string command = string.Format("SELECT * FROM [{0}] WHERE {1};", tableWithPrefix, statementsCMD);
            var result = ExecuteQuery(command);

            return result.Count > 0; ;
        }

        public bool Exist(string table, string column, object value)
        {
            return Exist(table, new string[] { column }, new object[] { value });
        }

        public string GetPrimaryKeyName(string table)
        {
            string tableWithPrefix = Prefix + table;
            var result = ExecuteQuery(string.Format("SELECT u.COLUMN_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS c INNER JOIN " +
                "INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS u ON c.CONSTRAINT_NAME = u.CONSTRAINT_NAME " +
                "where u.TABLE_NAME = '{0}' AND c.TABLE_NAME = '{0}' and c.CONSTRAINT_TYPE = 'PRIMARY KEY'", tableWithPrefix));

            if (result.Count > 0 && result[0].Count > 0)
            {
                return result[0][0].ToString();
            }
            else
            {
                return null;
            }
        }


        public List<object> GetRow(string table, string column, string format = "", params object[] arg)
        {
            return GetRow(table, new string[] { column }, format, arg);
        }

        public List<object> GetRow(string table, string[] columns, string format = "", params object[] arg)
        {
            string query = BuildGetRowsCMD(table, columns, string.Format(format, arg));

            var result = ExecuteQuery(query);

            if (result.Count == 0) return null;
            else return result[0];
        }


        public List<List<object>> GetRows(string table, string column, string format = "", params object[] arg)
        {
            return GetRows(table, new string[] { column }, format, arg);
        }

        public List<List<object>> GetRows(string table, string[] columns, string format = "", params object[] arg)
        {
            string query = BuildGetRowsCMD(table, columns, string.Format(format, arg));

            var result = ExecuteQuery(query);

            if (result.Count == 0) return null;
            else return result;
        }

        public object Insert(string table, string column, object value)
        {
            return Insert(table, new string[] { column }, new object[] { value });
        }

        public object Insert(string table, string[] columns, object[] values)
        {
            var result = Insert(table, columns, new List<object[]>() { values });
            if (result != null)
            {
                return ((List<object>)result)[0];
            }
            return null;
        }

        public object Insert(string table, string[] columns, List<object[]> values)
        {
            string tableWithPrefix = Prefix + table;
            List<string> lValues = new List<string>();
            
            foreach (var item in values)
            {
                var rVal = StringsSQLready(item).ToArray();
                string valueCMD = string.Join(", ", rVal);
                lValues.Add(valueCMD);
            }

            string columnsCMD = string.Format("[{0}]", string.Join("], [", columns.ToArray()));
            string valuesCMD = "(" + string.Join("), (", lValues) + ")";

            ExecuteQuery(string.Format("INSERT INTO [{0}]({1}) VALUES {2};", tableWithPrefix, columnsCMD, valuesCMD));

            //Get ID
            var pKey = GetPrimaryKeyName(table);
            if (pKey != null)
            {

                List<string> whrArr = new List<string>();
                foreach (var item in values)
                {
                    var whrArr2 = new List<string>();
                    for (int i = 0; i < columns.Length; i++)
                        whrArr2.Add(string.Format("[{0}] = {1}", columns[i], item[i]));
                    
                    whrArr.Add("(" +string.Join(" AND ", whrArr2.ToArray()) + ")");
                }


                


                string whrCMD = "WHERE " + string.Join(" OR ", whrArr.ToArray());

                var result = GetRow(table, pKey, whrCMD);
                if (result.Count > 0) return result;
            }

            return null;
        }

        //public void List<ExecuteQuery>(string cmd)
        //{
        //    throw new NotImplementedException();
        //}

        public object[] StringsSQLready(object[] values)
        {
            for (int i = 0; i < values.Length; i++)
                if (values[i] is string)
                    values[i] = string.Format("'{0}'", values[i]);

            return values;
        }

        public List<List<object>> ExecuteQuery(string cmd)
        {
            if (!IsConnected)
            {
                return null;
            }

            UseDistinct = false;
            SqlDataReader sql_reader;

            var result = new List<List<object>>();
            bool isRead = false;
            //Metoder der vil hente data
            string[] readMethods = { "SELECT", "PRAGMA" };

            //Find ud af om vi skal hente data
            foreach (var item in readMethods)
            {
                if (cmd.ToUpper().StartsWith(item))
                {
                    isRead = true;
                    break;
                }
            }

            //SQLite_LIB.SQLiteDataAdapter da = new System.Data.SQLite.SQLiteDataAdapter();

            _lastQuery = cmd;
            sql_cmd.CommandText = cmd;

            //udfør kommando hvor man ikke venter på data
            if (!isRead)
            {
                sql_cmd.ExecuteNonQuery();
                return null;
            }

            //Udfør kommando hvor man skal have data tilbage
            sql_reader = sql_cmd.ExecuteReader();
            while (sql_reader.Read())
            {
                var row = new List<object>();

                for (int i = 0; i < sql_reader.FieldCount; i++)
                {
                    row.Add(sql_reader[i]);
                }
                result.Add(row);
            }
            sql_reader.Close();
            return result;
        }

        public void Create(string table, params IColumn[] columns)
        {
            string tableWithPrefix = Prefix + table;

            List<string> columnCMD = new List<string>();
            List<string> foreignCMD = new List<string>();


            foreach (var column in columns)
            {
                string columnCmd = GetColumn(column);

                if (GetForeignKey(column) != null)
                {
                    column.ForeignKeyReferences = Prefix + column.ForeignKeyReferences;
                    columnCmd += GetForeignKey(column);
                }
                    

                columnCMD.Add(columnCmd);

            }

            string columnPart = string.Join(",\n ", columnCMD.ToArray());
            //string foreignKeyPart = foreignCMD.Count > 0 ? ",\n " + string.Join(",\n ", foreignCMD.ToArray()) : "";

            string command = string.Format("CREATE TABLE [{0}]\n(\n {1}\n);", tableWithPrefix, columnPart);
            ExecuteQuery(command);
        }

        public string GetTableName(string name)
        {
            return Prefix + name;

        }

        public string GetColumn(IColumn col)
        {
            throw new NotImplementedException();


            //type = string.Format(type, length);
            //string query = string.Format("[{0}] {1}", name, type);

            //if (isUniqueKey) query += " UNIQUE";
            //if (isAutoIncrement) query += " IDENTITY(1,1)";
            //if (isNotNull) query += " NOT NULL";

            //if (defaultValue != null)
            //{
            //    if (type.ToUpper() == TYPE_INT)
            //        query += string.Format(" DEFAULT {0}", defaultValue);
            //    else
            //        query += string.Format(" DEFAULT '{0}'", defaultValue);
            //}

            //if (isPrimaryKey) query += string.Format(", PRIMARY KEY([{0}])", name);

            //return query;
        }

        public string GetForeignKey(IColumn col)
        {
            throw new NotImplementedException();


            //if (foreignKeyReferences == null) return null;
            //return string.Format(" FOREIGN KEY REFERENCES {0}", foreignKeyReferences);
        }

        public void Update(string table, string column, object value, string format = "", params object[] arg)
        {
            throw new NotImplementedException();
        }

        public void Update(string table, string[] columns, object[] values, string format = "", params object[] arg)
        {
            throw new NotImplementedException();
        }

        public void Update(string table, string[] columns, object[] values, object pValue)
        {
            throw new NotImplementedException();
        }

        public void Update(string table, string column, object value, object pValue)
        {
            throw new NotImplementedException();
        }
    }
}
