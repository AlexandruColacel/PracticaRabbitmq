namespace AppForSEII2526.API.DTOs.PurchaseDTO
{
    public class PurchaseForCreateDTO
    {
        public PurchaseForCreateDTO(string customerUserName, string customerUserSurname, string deliveryAddress, PaymentMethod paymentMethod, List<PurchaseItemDTO> purchaseItems)
        {
            CustomerUserName = customerUserName;
            CustomerUserSurname = customerUserSurname;
            DeliveryAddress = deliveryAddress;
            PaymentMethod = paymentMethod;
            PurchaseItems = purchaseItems;
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please, set your Surname")]
        public string CustomerUserName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please, set your Surname")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Name and Surname must have at least 10 characters")]
        public string CustomerUserSurname { get; set; }

        [DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        [Display(Name = "Delivery Address")]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "Delivery address must have at least 10 characters")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please, set your address for delivery")]
        public string DeliveryAddress { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        public List<PurchaseItemDTO> PurchaseItems { get; set; }

        //EQUALS
        public override bool Equals(object? obj)
        {
            // 1. Comprobar tipo y nulidad
            if (obj is not PurchaseForCreateDTO dto)
                return false;

            // 2. Comprobar referencia (optimización)
            if (ReferenceEquals(this, obj))
                return true;

            // 3. Comparar propiedades
            // Usamos SequenceEqual para la lista, lo que requiere que PurchaseItemDTO tenga Equals implementado
            bool itemsEqual = (PurchaseItems == null && dto.PurchaseItems == null) ||
                              (PurchaseItems != null && dto.PurchaseItems != null && PurchaseItems.SequenceEqual(dto.PurchaseItems));

            return object.Equals(CustomerUserName, dto.CustomerUserName) &&
                   object.Equals(CustomerUserSurname, dto.CustomerUserSurname) &&
                   object.Equals(DeliveryAddress, dto.DeliveryAddress) &&
                   PaymentMethod.Equals(dto.PaymentMethod) &&
                   itemsEqual;
        }

        public override int GetHashCode()
        {
            // Es buena práctica incluir las propiedades usadas en Equals
            // Para la lista, una forma simple es combinarla si no es nula
            return HashCode.Combine(CustomerUserName, CustomerUserSurname, DeliveryAddress, PaymentMethod, PurchaseItems);
        }


    }//De class PurchaseForCreateDTO
}//De namespace AppForSEII2526.API.DTOs.PurchaseDTO
