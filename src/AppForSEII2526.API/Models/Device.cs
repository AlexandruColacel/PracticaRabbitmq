using Microsoft.AspNetCore.Identity;

namespace AppForSEII2526.API.Models
{

    public class Device //voy a crear la clase Device 
    {
        //constructores
        public Device() {

        }

        public Device(string color, string brand, string name, double priceForRent, double priceForPurchase, QualityType quality, int year, int quantityForPurchase, int quantityForRent, IList<ReviewItem> reviewItems, Model model, string? description, IList<PurchaseItem> purchaseItems, IList<RentDevice> deviceItems) {
            Color = color;
            Brand = brand;
            Name = name;
            PriceForRent = priceForRent;
            PriceForPurchase = priceForPurchase;
            Quality = quality;
            Year = year;
            QuantityForPurchase = quantityForPurchase;
            QuantityForRent = quantityForRent;
            ReviewItems = reviewItems;
            Model = model;
            Description = description;
            PurchaseItems = purchaseItems;
            DeviceItems = deviceItems;
        }

        [Key]
        public int id { get; set; } //identificador unico del dispositivo

        [StringLength(20, ErrorMessage = "Un color no puede tener mas de 20 caracteres")]
        public string Color { get; set; } //color del dispositivo considero que debe tener como MAXIMO 20 caracteres mas de eso considero que es exagerado

        [Required(AllowEmptyStrings = false, ErrorMessage = "Porfa introduce una marca")]
        public string Brand { get; set; } //considero que la marca si que debe estar obligatoriamente

        [StringLength(35, ErrorMessage = "El nombre del dispositivo no puede tener mas de 35 caracteres")]
        public string Name { get; set; } //el nombre del dispositivo considero que no podra tener mas de 35 caracteres

        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Range(5, 250, ErrorMessage = "Minimo es de 5 y el maximo 250")]
        [Display(Name = "Precio Alquiler")]
        public double PriceForRent { get; set; } //en los atributos que tengan precio de aqui en adelante les pondre que sean DataType.Currency

        [DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
        [Range(5, 250, ErrorMessage = "Minimo es de 5 y el maximo 250")]
        [Display(Name = "Precio Compra")]
        public double PriceForPurchase { get; set; }

        [Display(Name = "Calidad del dispositivo")]
        public QualityType Quality { get; set; }//creo una enumeracion con las calidades que existen de los productos

        [Display(Name = "Año de salida al mercado del telefono")]
        public int Year { get; set; }

        [Display(Name = "Cantidad de dispositivos a comprar")]
        [Range(0, int.MaxValue, ErrorMessage = "La compra minima es de 1 dispositivo")]
        public int QuantityForPurchase { get; set; }//Creo el atributo con la restriccion de que como minimo se compre 1 articulo


        [Display(Name = "Cantidad de dispositivos a alquilar")]
        [Range(0, int.MaxValue, ErrorMessage = "El alquiler  minimo es de 1 dispositivo")]
        public int QuantityForRent { get; set; }//Creo el atributo con la restriccion de que como minimo se compre 1 

        public IList<ReviewItem> ReviewItems { get; set; }

        [Required]
        public Model Model { get; set; }

        [StringLength(500, ErrorMessage = "La descripcion es demasiado larga")]
        public string? Description { get; set; }//Creo el atributo description para que el usuario pueda explicar porque compra el dispositivo con un maximo de 500 caracteres

        //Relacion N:N con PurchaseItem
        public IList<PurchaseItem> PurchaseItems { get; set; } //confirmación de cambios

        public IList<RentDevice> DeviceItems { get; set; } //Segunda parte de la clave foranea con RentDevice
    }
    public enum QualityType
    {
        Excelente, Bueno, Normal, Mala, Seminuevo
    }
}