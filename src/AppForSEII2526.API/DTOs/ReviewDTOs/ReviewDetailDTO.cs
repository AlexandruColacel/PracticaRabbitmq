using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AppForSEII2526.API.DTOs.RentalDTOs
{
    public class ReviewDetailDTO : ReviewForCreateDTO
    {
        //el título de la reseña y la fecha en que se realizó así como de cada dispositivo su nombre, modelo, año, puntuación y el comentario, indicando los datos del cliente (nombre y país)

        public ReviewDetailDTO(int id, DateTime reviewDate, string reviewTitle, string nombreCliente, string paisCliente,IList<ReviewItemDTO> reviewItems)
            : base( reviewTitle, reviewItems, nombreCliente, paisCliente)
        {
            Id = id;
            ReviewDate= reviewDate;
        }

        public int Id { get; set;}
        public DateTime ReviewDate { get; set; }
        public override bool Equals(object? obj)
        {
            return obj is ReviewDetailDTO dTO &&
                   base.Equals(obj) &&
                   Id == dTO.Id &&
                   CompareDate(ReviewDate, dTO.ReviewDate);
        }


    }
