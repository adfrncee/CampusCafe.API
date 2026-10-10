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
    public class ProductRepository
    {
        private readonly string _connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + 
            Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\", "Physical Data Model (GILO, PAMATIAN, VILLAFOR, VELASCO).accdb")) + ";";

        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(ILogger<ProductRepository> logger)
        {
            _logger = logger;
        }

        public List<Product> GetProducts()
        {
            var list = new List<Product>();
            try
            {
                using var conn = new OleDbConnection(_connectionString);
                using var cmd = new OleDbCommand("SELECT ProductID, ProductName, Description, Price, Category, AvailabilityStatus FROM [Product]", conn);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    object rawStatus = reader.IsDBNull(5) ? false : reader.GetValue(5);
                    bool isAvailable = rawStatus switch
                    {
                        bool b => b,
                        string s => s.Equals("Available", StringComparison.OrdinalIgnoreCase) || s.Equals("True", StringComparison.OrdinalIgnoreCase) || s == "1",
                        int i => i == 1,
                        _ => false
                    };

                    list.Add(new Product
                    {
                        ProductID = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0)),
                        ProductName = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString()!,
                        Description = reader.IsDBNull(2) ? "" : reader.GetValue(2).ToString()!,
                        Price = reader.IsDBNull(3) ? 0 : Convert.ToDecimal(reader.GetValue(3)),
                        Category = reader.IsDBNull(4) ? "" : reader.GetValue(4).ToString()!,
                        AvailabilityStatus = isAvailable
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products.");
            }
            return list;
        }

        public List<Product> GetAllProducts() => GetProducts();

        public bool AddProduct(Product prod)
        {
            try
            {
                using var conn = new OleDbConnection(_connectionString);
                conn.Open();

                int nextId = 1;
                using (var checkCmd = new OleDbCommand("SELECT IIF(ISNULL(MAX(ProductID)), 0, MAX(ProductID)) FROM [Product]", conn))
                {
                    var res = checkCmd.ExecuteScalar();
                    if (res != null && res != DBNull.Value)
                    {
                        nextId = Convert.ToInt32(res) + 1;
                    }
                }

                string sql = "INSERT INTO [Product] ([ProductID], [ProductName], [Description], [Price], [Category], [AvailabilityStatus]) VALUES (?, ?, ?, ?, ?, ?)";
                using var cmd = new OleDbCommand(sql, conn);

                string availabilityStr = prod.AvailabilityStatus ? "Available" : "Unavailable";

                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = nextId });
                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = prod.ProductName ?? "" });
                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = prod.Description ?? "" });
                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.Currency) { Value = prod.Price });
                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = prod.Category ?? "General" });
                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = availabilityStr });

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product: {Message}", ex.Message);
                return false;
            }
        }

        public bool DeleteProduct(int id)
        {
            try
            {
                using var conn = new OleDbConnection(_connectionString);
                string sql = "DELETE FROM [Product] WHERE ProductID = ?";
                using var cmd = new OleDbCommand(sql, conn);

                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = id });

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product.");
                return false;
            }
        }
    }
}