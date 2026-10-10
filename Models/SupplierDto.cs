using System;

namespace CampusCafe.API.Models
{
    public class SupplierDto
    {
        public int SupplierID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string TelephoneNumber { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string BusinessAddress { get; set; } = string.Empty;
        public int AssociatedIngredientID { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
    }

    public class DeliveryOverviewDto
    {
        public int DeliveryID { get; set; }
        public int SupplierID { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public double QuantityReceived { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}