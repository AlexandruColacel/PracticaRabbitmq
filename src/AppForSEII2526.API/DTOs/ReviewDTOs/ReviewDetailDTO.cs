using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AppForSEII2526.API.DTOs.ReviewDTOs
{
    public class ReviewDetailDTO 
    {
        //el título de la reseña y la fecha en que se realizó así como de cada dispositivo su nombre, modelo, año, puntuación y el comentario, indicando los datos del cliente (nombre y país)

        public ReviewDetailDTO(int id, DateTime dateOfReview, string reviewTitle, string nombreCliente, int paisCliente, IList<ReviewItemDTO> reviewItems)
            
        {
            Id = id;
            DateOfReview = dateOfReview;
            ReviewTitle = reviewTitle;
            NombreCliente = nombreCliente;
            PaisCliente = paisCliente;
            ReviewItems = reviewItems;
        }

        public string ReviewTitle { get; set; }
        public string NombreCliente { get; set; }
        public int PaisCliente { get; set; }
        public IList<ReviewItemDTO> ReviewItems { get; set; }

        public int Id { get; set; }
        public DateTime DateOfReview { get; set; }
       


    }
}
