using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Runtime.Versioning;
using CampusCafe.API.Models;
using Microsoft.Extensions.Logging;

namespace CampusCafe.API.Repositories
{
    [SupportedOSPlatform("windows")]
    public class EmployeeRepository
    {
        private readonly string _connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + 
            Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\", "Physical Data Model (GILO, PAMATIAN, VILLAFOR, VELASCO).accdb")) + ";";

        private readonly ILogger<EmployeeRepository> _logger;

        public EmployeeRepository(ILogger<EmployeeRepository> logger)
        {
            _logger = logger;
        }

        public List<Employee> GetAllEmployees()
        {
            var list = new List<Employee>();
            try
            {
                using var conn = new OleDbConnection(_connectionString);
                using var cmd = new OleDbCommand("SELECT EmployeeID, Name, ContactNumber, WorkSchedule FROM [Employee]", conn);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Employee
                    {
                        EmployeeID = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0)),
                        Name = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString()!,
                        ContactNumber = reader.IsDBNull(2) ? "" : reader.GetValue(2).ToString()!,
                        WorkSchedule = reader.IsDBNull(3) ? "" : reader.GetValue(3).ToString()!,
                        Position = ""
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employees.");
            }
            return list;
        }

        public bool AddEmployee(Employee emp)
        {
            try
            {
                using var conn = new OleDbConnection(_connectionString);
                conn.Open();

                int nextId = 1;
                using (var checkCmd = new OleDbCommand("SELECT IIF(ISNULL(MAX(EmployeeID)), 0, MAX(EmployeeID)) FROM [Employee]", conn))
                {
                    var res = checkCmd.ExecuteScalar();
                    if (res != null && res != DBNull.Value)
                    {
                        nextId = Convert.ToInt32(res) + 1;
                    }
                }

                string fullName = string.IsNullOrWhiteSpace(emp.Position) 
                    ? emp.Name 
                    : $"{emp.Name} ({emp.Position})";

                string safeName = fullName.Length > 100 ? fullName.Substring(0, 100) : fullName;
                string safeContact = (emp.ContactNumber ?? "").Length > 20 ? emp.ContactNumber!.Substring(0, 20) : (emp.ContactNumber ?? "");
                string safeSchedule = (emp.WorkSchedule ?? "").Length > 20 ? emp.WorkSchedule!.Substring(0, 20) : (emp.WorkSchedule ?? "");

                string sql = "INSERT INTO [Employee] ([EmployeeID], [Name], [ContactNumber], [WorkSchedule]) VALUES (?, ?, ?, ?)";
                using var cmd = new OleDbCommand(sql, conn);
                
                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = nextId });
                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = safeName });
                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = safeContact });
                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = safeSchedule });

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding employee.");
                return false;
            }
        }

        public bool DeleteEmployee(int id)
        {
            try
            {
                using var conn = new OleDbConnection(_connectionString);
                string sql = "DELETE FROM [Employee] WHERE EmployeeID = ?";
                using var cmd = new OleDbCommand(sql, conn);

                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = id });

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee.");
                return false;
            }
        }
    }
}