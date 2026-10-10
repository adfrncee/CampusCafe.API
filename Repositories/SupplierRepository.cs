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
    public class SupplierRepository
    {
        private readonly string _connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + 
            Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\", "Physical Data Model (GILO, PAMATIAN, VILLAFOR, VELASCO).accdb")) + ";";

        private readonly ILogger<SupplierRepository> _logger;

        public SupplierRepository(ILogger<SupplierRepository> logger)
        {
            _logger = logger;
        }

        public List<SupplierDto> GetAllSuppliers()
        {
            var list = new List<SupplierDto>();
            try
            {
                using var conn = new OleDbConnection(_connectionString);
                using var cmd = new OleDbCommand("SELECT SupplierID, CompanyName, ContactPerson, TelephoneNumber, EmailAddress, BusinessAddress FROM [Supplier]", conn);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new SupplierDto
                    {
                        SupplierID = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0)),
                        CompanyName = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString()!,
                        ContactPerson = reader.IsDBNull(2) ? "" : reader.GetValue(2).ToString()!,
                        TelephoneNumber = reader.IsDBNull(3) ? "" : reader.GetValue(3).ToString()!,
                        EmailAddress = reader.IsDBNull(4) ? "" : reader.GetValue(4).ToString()!,
                        BusinessAddress = reader.IsDBNull(5) ? "" : reader.GetValue(5).ToString()!
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching suppliers.");
            }
            return list;
        }

        public bool AddSupplier(SupplierDto sup)
        {
            try
            {
                using var conn = new OleDbConnection(_connectionString);
                conn.Open();

                int nextId = 1;
                using (var checkCmd = new OleDbCommand("SELECT IIF(ISNULL(MAX(SupplierID)), 0, MAX(SupplierID)) FROM [Supplier]", conn))
                {
                    var res = checkCmd.ExecuteScalar();
                    if (res != DBNull.Value && res != null)
                    {
                        nextId = Convert.ToInt32(res) + 1;
                    }
                }

                string sql = "INSERT INTO [Supplier] ([SupplierID], [CompanyName], [ContactPerson], [TelephoneNumber], [EmailAddress], [BusinessAddress]) VALUES (?, ?, ?, ?, ?, ?)";
                using var cmd = new OleDbCommand(sql, conn);
                
                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = nextId });
                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = sup.CompanyName ?? "" });
                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = sup.ContactPerson ?? "" });
                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = sup.TelephoneNumber ?? "" });
                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = sup.EmailAddress ?? "" });
                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = sup.BusinessAddress ?? "" });

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding supplier: {Message}", ex.Message);
                return false;
            }
        }

        public List<DeliveryOverviewDto> GetDeliveries()
        {
            var list = new List<DeliveryOverviewDto>();
            try
            {
                using var conn = new OleDbConnection(_connectionString);
                string sql = @"SELECT r.DeliveryID, r.SupplierID, s.CompanyName, r.DeliveryDate, 
                                      i.IngredientName, di.QuantityReceived, dp.AmountPaid, dp.PaymentMethod
                               FROM ((([Delivery_Receipt] r 
                               INNER JOIN [Supplier] s ON r.SupplierID = s.SupplierID)
                               LEFT JOIN [Delivery_Item] di ON r.DeliveryID = di.DeliveryID)
                               LEFT JOIN [Ingredient] i ON di.IngredientID = i.IngredientID)
                               LEFT JOIN [Delivery_Payment] dp ON r.DeliveryID = dp.DeliveryID";

                using var cmd = new OleDbCommand(sql, conn);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new DeliveryOverviewDto
                    {
                        DeliveryID = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0)),
                        SupplierID = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1)),
                        SupplierName = reader.IsDBNull(2) ? "N/A" : reader.GetValue(2).ToString()!,
                        DeliveryDate = reader.IsDBNull(3) ? DateTime.Now : Convert.ToDateTime(reader.GetValue(3)),
                        IngredientName = reader.IsDBNull(4) ? "General Supply" : reader.GetValue(4).ToString()!,
                        QuantityReceived = reader.IsDBNull(5) ? 0 : Convert.ToDouble(reader.GetValue(5)),
                        AmountPaid = reader.IsDBNull(6) ? 0 : Convert.ToDecimal(reader.GetValue(6)),
                        PaymentMethod = reader.IsDBNull(7) ? "Cash" : reader.GetValue(7).ToString()!
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching deliveries.");
            }
            return list;
        }
    }
}