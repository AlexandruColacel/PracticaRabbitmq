namespace AppForSEII2526.API.DTOs.RentalDTOs
{
    public class RentalItemDTO
    {
        public RentalItemDTO(string model, string brand, double rentPrice)
        {
            Model = model;
            Brand = brand;
            RentPrice = rentPrice;
        }

        public string Model { get; set; }
        public string Brand { get; set; }
        public double RentPrice { get; set; }
    }
}
