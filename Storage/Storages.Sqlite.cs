using ExcelDataReader;
using Microsoft.Office.Interop.Excel;
using MyWpfApp.Extinsion;
using MyWpfApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DataTable = System.Data.DataTable;

namespace MyWpfApp.Storage
{
	public partial class Storages
	{
        private const string CONN_STRING = "Data Source=D:\\ProgrammFile\\Projects\\MyWpfApp\\database\\FileDb.db;";

        public void Dispose(IDbConnection connection)
        {
            if (connection.State != ConnectionState.Closed) connection.Close();
            GC.SuppressFinalize(this);
        }

        private IDbConnection CreateConnection()
        {
            return new SQLiteConnection(CONN_STRING);
        }
        public int ExecuteNonQuery(string sqlQuery, CommandType cmdType = CommandType.Text, IDbConnection connection = null)
        {
            try
            {
                if (connection == null)
                    connection = CreateConnection();

                if (connection.State != ConnectionState.Open)
                    connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = sqlQuery;
                command.CommandType = cmdType;

                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            finally
            {
                Dispose(connection);
            }
        }
        public IDataReader ExecuteReader(string sqlQuery, CommandType cmdType = CommandType.Text, IDbConnection connection = null)
        {
            try
            {
                if (connection == null)
                    connection = CreateConnection();

                if (connection.State != ConnectionState.Open)
                    connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = sqlQuery;
                command.CommandType = cmdType;

                return command.ExecuteReader();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }


        public DataTable GetDataTable(string tableName)
		{
            var dataTable = new DataTable();
            var connection = CreateConnection();
         
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                var query = $"SELECT * FROM {tableName};";
                var reader = ExecuteReader(query, connection:connection);

                dataTable.Load(reader);

                return dataTable;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            finally
            {
                Dispose(connection);
            }

            return dataTable;
		}
		private void ImportExcelToSQLite(Stream stream, DriveFile file)
		{

            var connection = CreateConnection();
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                stream.Position = 0;

                IExcelDataReader reader = null;

                if(file.FileType == "xlsx")
                    reader = ExcelReaderFactory.CreateReader(stream);
                else
                    reader = ExcelReaderFactory.CreateCsvReader(stream);

                var tableQuery = new StringBuilder();

                tableQuery.AppendFormat("CREATE TABLE IF NOT EXISTS {0} ( \n", file.Name.ReplaceSpaceWithCharacter('_'));

                var colCount = reader.FieldCount;
                var colIndex = 0;

                // create columns
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (colIndex < colCount - 1)
                        tableQuery.AppendFormat("Column{0} TEXT,\n", i);
                    else
                        tableQuery.AppendFormat("Column{0} TEXT\n", i);
                    colIndex++;
                }

                tableQuery.AppendLine(");");

                ExecuteNonQuery(tableQuery.ToString(), connection: connection);

                var insertQuery = new StringBuilder();
                insertQuery.AppendFormat("INSERT INTO \"{0}\" VALUES \n", file.Name.ReplaceSpaceWithCharacter('_'));

                var rowCount = reader.RowCount;

                var rowIndex = 0;
                while (reader.Read())
                {
                    insertQuery.AppendLine("(");
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader.GetValue(i);
                        if (i < reader.FieldCount - 1)
                            insertQuery.AppendFormat("\"{0}\", ", value);
                        else
                            insertQuery.AppendFormat("\"{0}\" ", value);
                    }

                    if (rowIndex < rowCount - 1)
                        insertQuery.AppendLine("),\n");
                    else
                        insertQuery.AppendLine(");\n");

                    rowIndex++;
                }

                ExecuteNonQuery(insertQuery.ToString(), connection: connection);

            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                stream.Close();
                Dispose(connection);
            }


		}
	}
}
