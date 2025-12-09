namespace AppForSEII2526.API.DTOs.RentalDTOs
{
    public class RentalDetailsDTO : RentalPostDTO
    {
        // Constructor vacío para serialización
        public RentalDetailsDTO()
        {
            RentalItems = new List<RentalItemDTO>();
        }

        // Constructor con todos los parámetros necesarios según el enunciado
        public RentalDetailsDTO(int id, DateTime rentalDate, string customerUserName,
            string customerNameSurname, string deliveryAddress, PaymentMethodTypes paymentMethod,
            DateTime rentalDateFrom, DateTime rentalDateTo, IList<RentalItemDTO> rentalItems,
            double totalPrice)
        {
            Id = id;
            RentalDate = rentalDate;
            CustomerUserName = customerUserName;
            CustomerNameSurname = customerNameSurname;
            DeliveryAddress = deliveryAddress;
            PaymentMethod = paymentMethod;
            RentalDateFrom = rentalDateFrom;
            RentalDateTo = rentalDateTo;
            RentalItems = rentalItems ?? new List<RentalItemDTO>();
            TotalPrice = totalPrice;
        }

        // Identificador del alquiler
        public int Id { get; set; }

        // Datos del cliente
        [Display(Name = "Email del Cliente")]
        public string CustomerUserName { get; set; }

        [Display(Name = "Nombre y Apellidos")]
        public string CustomerNameSurname { get; set; }

        [Display(Name = "Dirección de Entrega")]
        public string DeliveryAddress { get; set; }

        // Datos del alquiler
        [Display(Name = "Fecha del Alquiler")]
       
        public DateTime RentalDate { get; set; }

        [Display(Name = "Precio Total")]
     
        public double TotalPrice { get; set; }

        // Periodo de alquiler
        [Display(Name = "Fecha Inicio Alquiler")]
     
        public DateTime RentalDateFrom { get; set; }

        [Display(Name = "Fecha Fin Alquiler")]
      
        public DateTime RentalDateTo { get; set; }

        [Display(Name = "Método de Pago")]
        public PaymentMethodTypes PaymentMethod { get; set; }

        // Lista de dispositivos alquilados (modelo, precio, cantidad)
        [Display(Name = "Dispositivos Alquilados")]
        public IList<RentalItemDTO> RentalItems { get; set; }


        public override bool Equals(object? obj) {
            return obj is RentalDetailsDTO dTO &&
                   base.Equals(obj) &&
                   TotalPrice == dTO.TotalPrice &&
                   Id == dTO.Id &&
                   CompareDate(RentalDate, dTO.RentalDate);
        }

        public override int GetHashCode() {
            return HashCode.Combine(base.GetHashCode(), Id, RentalDate);
        }
    }
}
