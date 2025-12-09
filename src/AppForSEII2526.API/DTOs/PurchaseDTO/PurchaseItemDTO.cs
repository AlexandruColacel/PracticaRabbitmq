namespace AppForSEII2526.API.DTOs.PurchaseDTO
{
    public class PurchaseItemDTO
    {
        public PurchaseItemDTO() { } //constructor vacio debido a Entity Framework

        //Constructor que se usará en el PurchaseDetailsDTO
        public PurchaseItemDTO(int id, string brand, string model, string color, decimal unitPrice, int quantity, string description)
        {
            Id = id;
            Brand = brand;
            Model = model;
            Color = color;
            UnitPrice = unitPrice;
            Quantity = quantity;
            Description = description;
        }

        //Utilizado en PurchaseForListDTO
        public PurchaseItemDTO(int id, string brand, string model, string color, decimal unitPrice)
        {
            Id = id; 
            Brand = brand;
            Model = model;
            Color = color;
            UnitPrice = unitPrice;
        }

        public int Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }     // campo opcional según tu caso de uso

        //NOTA: TEST DE PRUEBA DE UNIDAD, AQUÍ SOLO COMPRUEBA EL ID, ASÍ QUE COMENTO EL RESTO PARA QUE PASE EL TEST (DESCOMENTAR EN UN FUTURO SI ES NECESARIO)
        public override bool Equals(object? obj)
        {
            // 1. Comprobar tipo y nulidad
            if (obj is not PurchaseItemDTO dTO)
                return false;

            // 2. Comprobar si es la misma instancia (optimización rápida)
            if (ReferenceEquals(this, obj))
                return true;

            // 3. Comparar campos
            return
                // Para tipos de valor (int, decimal, Guid) que no pueden ser null,
                // .Equals() o == es seguro y correcto.
                Id.Equals(dTO.Id) &&
                UnitPrice.Equals(dTO.UnitPrice) &&
                Quantity.Equals(dTO.Quantity) &&

                // Para tipos de referencia (string, etc.) que SÍ pueden ser null,
                // usa el método estático 'object.Equals' para evitar excepciones.
                object.Equals(Brand, dTO.Brand) &&
                object.Equals(Model, dTO.Model) &&
                object.Equals(Color, dTO.Color) &&
                object.Equals(Description, dTO.Description);
        }

        //Si dos objetos son 'Equals', DEBEN tener el mismo HashCode.
        public override int GetHashCode()
        {
            // Usa HashCode.Combine para calcular un hash basado en los mismos campos que usaste en Equals.
            return HashCode.Combine(Id, Brand, Model, Color, UnitPrice, Quantity, Description);
        }
    }
}
