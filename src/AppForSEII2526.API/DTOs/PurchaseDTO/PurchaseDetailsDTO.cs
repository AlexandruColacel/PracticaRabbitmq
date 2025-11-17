using Microsoft.AspNetCore.Http.Features;

namespace AppForSEII2526.API.DTOs.PurchaseDTO
{
    public class PurchaseDetailsDTO
    {
        //Constructor (recordar que la única diferencia entre detail y POST, es que detail tiene id)
        public PurchaseDetailsDTO(int id, string customerName, string customerSurname, string direccionEntrega, DateTime fechaCompra, decimal precioTotal, int cantidadTotal, IList<PurchaseItemDTO> dispositivosComprados)
        {
            Id = id;
            CustomerName = customerName;
            CustomerSurname = customerSurname;
            DeliveryAddress = direccionEntrega;
            PurchaseDate = fechaCompra;
            TotalPrice = precioTotal;
            TotalQuantity = cantidadTotal;
            Items = dispositivosComprados;
        }

        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string CustomerSurname { get; set; }
        public string DeliveryAddress { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal TotalPrice { get; set; }
        public int TotalQuantity { get; set; }

        //AÑADIR LISTA DE DISPOSITIVOS COMPRADOS
        public IList<PurchaseItemDTO> Items { get; set; }

        //CONSTANTE PRIVADA PARA CALCULAR DIFERENCIA DE TIEMPOS
        private const double ToleranceSeconds = 1.0; // Tolerancia de 1 segundo

        public override bool Equals(object? obj)
        {
            // 1. Comprobar tipo y nulidad
            if (obj is not PurchaseDetailsDTO dTO)
                return false;

            // 2. Comprobar si es la misma instancia (optimización rápida)
            if (ReferenceEquals(this, obj))
                return true;

            // 3. Comparar campos
            return
                   Id.Equals(dTO.Id) &&
                   //Para tipos de referencia (string, etc.) que SÍ pueden ser null,
                   object.Equals(CustomerName, dTO.CustomerName) && //COMENTADO PARA PRUEBAS
                   object.Equals(CustomerSurname, dTO.CustomerSurname) && //COMENTADO PARA PRUEBAS
                   object.Equals(DeliveryAddress, dTO.DeliveryAddress) && //COMENTADO PARA PRUEBAS
                   //PurchaseDate.Equals(dTO.PurchaseDate) && //uso Date.now, por lo que puede petar si tengo diferencia de milisegundos
                   //Este metodo se encarga de comparar fechas con tolerancia
                   DatesClose(PurchaseDate, dTO.PurchaseDate, ToleranceSeconds) && //COMENTADO PARA PRUEBAS
                   TotalPrice.Equals(dTO.TotalPrice) && //COMENTADO PARA PRUEBAS
                   TotalQuantity.Equals(dTO.TotalQuantity) && //COMENTADO PARA PRUEBAS
                   //EqualityComparer<IList<PurchaseItemDTO>>.Default.Equals(Items, dTO.Items);
                   Items.SequenceEqual(dTO.Items); //A revisar (TRAS COMENTAR ESTO, EL TEST PASA BIEN) ////COMENTADO PARA PRUEBAS
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, CustomerName, CustomerSurname, DeliveryAddress, PurchaseDate, TotalPrice, TotalQuantity, Items);
        }

        //Respecto al equals de fechas, he decidido crear un método aparte para comparar fechas con margen de error

        // Método privado para lógica de fechas
        private bool DatesClose(DateTime dt1, DateTime dt2, double toleranceInSeconds)
        {
            // Calcula la diferencia absoluta (sin signo)
            TimeSpan difference = (dt1 - dt2);
            return Math.Abs(difference.TotalSeconds) < toleranceInSeconds;
        }


    }//De clase PurchaseDetailsDTO
}