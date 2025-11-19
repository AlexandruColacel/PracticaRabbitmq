using System.Drawing;

namespace AppForSEII2526.API.DTOs.DeviceDTO
{
    public class DeviceParaRentDTO
    {
        // DTO de Salida: Muestra la información resumida de un dispositivo en el catálogo de alquiler.
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

        //equals
        // Método Equals añadido durante el Sprint:
        // Fundamental para el "Data Driven Testing" (Test con varios casos).
        // Permite verificar que el filtro por Marca o Precio devuelve EXACTAMENTE el objeto DTO esperado.
        public override bool Equals(object? obj) {
            return obj is DeviceParaRentDTO dTO &&
                   Id == dTO.Id &&
                   Name == dTO.Name &&
                   Model == dTO.Model &&
                   Brand == dTO.Brand &&
                   Year == dTO.Year &&
                   Color == dTO.Color &&
                   RentPrice == dTO.RentPrice;
        }

        public override int GetHashCode() {
            return HashCode.Combine(Id, Name, Model, Brand, Year, Color, RentPrice);
        }



    }
}
