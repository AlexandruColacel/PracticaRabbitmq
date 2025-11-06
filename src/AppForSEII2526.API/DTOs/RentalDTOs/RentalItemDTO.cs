namespace AppForSEII2526.API.DTOs.RentalDTOs
{
    public class RentalItemDTO
    {
        public RentalItemDTO(int id,string model, string brand, double rentPrice, int quantity)
        {
            Id = id;
            Model = model;
            Brand = brand;
            RentPrice = rentPrice;
            Quantity = quantity;
        }

        public int Id { get; set; }
        public string Model { get; set; }
        public string Brand { get; set; }
        public double RentPrice { get; set; }
        public int Quantity { get; set; }
    }
}
