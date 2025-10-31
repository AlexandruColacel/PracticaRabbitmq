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

        [EmailAddress]
        [Required]
        public string CustomerUserName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please, set your Name and Surname")]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "Name and Surname must have at least 10 characters")]
        public string CustomerUserSurname { get; set; }

        [DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        [Display(Name = "Delivery Address")]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "Delivery address must have at least 10 characters")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please, set your address for delivery")]
        public string DeliveryAddress { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        public List<PurchaseItemDTO> PurchaseItems { get; set; }

        //Metodos aparte


    }//De class PurchaseForCreateDTO
}//De namespace AppForSEII2526.API.DTOs.PurchaseDTO
