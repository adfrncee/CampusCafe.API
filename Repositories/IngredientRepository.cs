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
    public class IngredientRepository
    {
        private readonly string _connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + 
            Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\", "Physical Data Model (GILO, PAMATIAN, VILLAFOR, VELASCO).accdb")) + ";";

        private readonly ILogger<IngredientRepository> _logger;

        public IngredientRepository(ILogger<IngredientRepository> logger)
        {
            _logger = logger;
        }

        public List<Ingredient> GetIngredients()
        {
            var list = new List<Ingredient>();
            try
            {
                using var conn = new OleDbConnection(_connectionString);
                using var cmd = new OleDbCommand("SELECT IngredientID, IngredientName, UnitOfMeasurement, CurrentQuantity, MinimumStockLevel FROM [Ingredient]", conn);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Ingredient
                    {
                        IngredientID = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0)),
                        IngredientName = reader.IsDBNull(1) ? "" : reader.GetValue(1).ToString()!,
                        UnitOfMeasurement = reader.IsDBNull(2) ? "" : reader.GetValue(2).ToString()!,
                        CurrentQuantity = reader.IsDBNull(3) ? 0 : Convert.ToDouble(reader.GetValue(3)),
                        MinimumStockLevel = reader.IsDBNull(4) ? 0 : Convert.ToDouble(reader.GetValue(4))
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ingredients.");
            }
            return list;
        }

        public List<Ingredient> GetAllIngredients() => GetIngredients();

        public bool AddIngredient(Ingredient ing)
        {
            try
            {
                using var conn = new OleDbConnection(_connectionString);
                conn.Open();

                int nextId = 1;
                using (var checkCmd = new OleDbCommand("SELECT IIF(ISNULL(MAX(IngredientID)), 0, MAX(IngredientID)) FROM [Ingredient]", conn))
                {
                    var res = checkCmd.ExecuteScalar();
                    if (res != null && res != DBNull.Value)
                    {
                        nextId = Convert.ToInt32(res) + 1;
                    }
                }

                string sql = "INSERT INTO [Ingredient] ([IngredientID], [IngredientName], [UnitOfMeasurement], [CurrentQuantity], [MinimumStockLevel]) VALUES (?, ?, ?, ?, ?)";
                using var cmd = new OleDbCommand(sql, conn);

                string safeUnit = string.IsNullOrWhiteSpace(ing.UnitOfMeasurement) ? "pcs" : ing.UnitOfMeasurement;

                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = nextId });
                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = ing.IngredientName ?? "" });
                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.VarWChar) { Value = safeUnit });
                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.Double) { Value = ing.CurrentQuantity });
                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.Double) { Value = ing.MinimumStockLevel > 0 ? ing.MinimumStockLevel : 1 });

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding ingredient: {Message}", ex.Message);
                return false;
            }
        }

        public bool AddIngredient(IngredientItemDto dto)
        {
            if (dto == null) return false;
            return AddIngredient(new Ingredient
            {
                IngredientName = dto.IngredientName,
                UnitOfMeasurement = dto.UnitOfMeasurement,
                CurrentQuantity = dto.CurrentQuantity,
                MinimumStockLevel = dto.MinimumStockLevel
            });
        }

        public bool UpdateIngredientStock(int id, double newQuantity)
        {
            try
            {
                using var conn = new OleDbConnection(_connectionString);
                string sql = "UPDATE [Ingredient] SET CurrentQuantity = ? WHERE IngredientID = ?";
                using var cmd = new OleDbCommand(sql, conn);

                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.Double) { Value = newQuantity });
                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = id });

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ingredient stock.");
                return false;
            }
        }

        public bool DeleteIngredient(int id)
        {
            try
            {
                using var conn = new OleDbConnection(_connectionString);
                string sql = "DELETE FROM [Ingredient] WHERE IngredientID = ?";
                using var cmd = new OleDbCommand(sql, conn);

                cmd.Parameters.Add(new OleDbParameter("?", OleDbType.Integer) { Value = id });

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ingredient.");
                return false;
            }
        }
    }
}