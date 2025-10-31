namespace AppForSEII2526.API.DTOs.PurchaseDTO
{
    public class PurchaseItemDTO
    {
        public PurchaseItemDTO() { }

        //Constructor que se usará en el PurchaseDetailsDTO
        public PurchaseItemDTO(string brand, string model, string color, decimal unitPrice, int quantity, string description)
        {
            Brand = brand;
            Model = model;
            Color = color;
            UnitPrice = unitPrice;
            Quantity = quantity;
            Description = description;
        }

        public PurchaseItemDTO(string brand, string model, string color, decimal unitPrice)
        {
            Brand = brand;
            Model = model;
            Color = color;
            UnitPrice = unitPrice;
        }

        public string Brand { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }     // campo opcional según tu caso de uso
    }
}
