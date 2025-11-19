namespace AppForSEII2526.API.DTOs.RentalDTOs
{
    public class RentalItemDTO
    {
        // DTO que representa un ítem individual dentro de un pedido de alquiler.
        // Ejemplo: "1x iPhone 15 Pro a 50€"
        public RentalItemDTO(int id,string model, string brand, double rentPrice, int quantity)
        {
            Id = id;
            Model = model;
            Brand = brand;
            RentPrice = rentPrice;
            Quantity = quantity;
        }
        // Constructor: Inicializa los datos básicos del ítem.
        public int Id { get; set; }
        public string Model { get; set; }
        public string Brand { get; set; }
        public double RentPrice { get; set; }
        public int Quantity { get; set; }

        // Sobrescribimos Equals para que los tests comparen el CONTENIDO del objeto
        // y no su referencia en memoria.
        public override bool Equals(object? obj) {
            return obj is RentalItemDTO item &&
                   Id == item.Id &&
                   Model == item.Model &&
                   Brand == item.Brand &&
                   RentPrice == item.RentPrice &&
                   Quantity == item.Quantity;
        }

        // Siempre que se sobrescribe Equals, se debe sobrescribir GetHashCode
        public override int GetHashCode() {
            return HashCode.Combine(Id, Model, Brand, RentPrice, Quantity);
        }
    }
}
