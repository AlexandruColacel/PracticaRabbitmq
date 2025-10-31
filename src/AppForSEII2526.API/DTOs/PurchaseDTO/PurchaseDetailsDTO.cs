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

    }//De clase PurchaseDetailsDTO
}
