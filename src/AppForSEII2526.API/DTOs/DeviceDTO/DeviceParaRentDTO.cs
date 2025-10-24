using System.Drawing;

namespace AppForSEII2526.API.DTOs.DeviceDTO
{
    public class DeviceParaRentDTO
    {
        public DeviceParaRentDTO(int id, string name, string model, string brand, int year, string color, double rentPrice)
        {
            Id = id;
            Name = name;
            Model = model;
            Brand = brand;
            Year = year;
            Color = color;
            RentPrice = rentPrice;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public string Brand { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }

        public double RentPrice { get; set; }

    
          
    }
}
