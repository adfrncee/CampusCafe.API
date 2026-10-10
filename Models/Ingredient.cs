namespace CampusCafe.API.Models
{
    public class Ingredient
    {
        public int IngredientID { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public string UnitOfMeasurement { get; set; } = string.Empty;
        public double CurrentQuantity { get; set; }
        public double MinimumStockLevel { get; set; }
    }
}