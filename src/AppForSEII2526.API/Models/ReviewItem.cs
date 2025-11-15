using System;

namespace AppForSEII2526.API.Models
{
    [PrimaryKey(nameof(DeviceId), nameof(ReviewId))]
    public class ReviewItem
    {   
        public ReviewItem() { }
        public ReviewItem(int deviceId, int reviewId, string comments, int id, int rating, Review review)
        {
            DeviceId = deviceId;
            ReviewId = reviewId;
            Comments = comments;
            Id = id;
            Rating = rating;
            Review = review;
        }
        public ReviewItem(int deviceId, string comments, int rating, Review review)
        {
            DeviceId = deviceId;
            Comments = comments;
            Rating = rating;
            Review = review;
        }


        public int DeviceId { get; set; }//Identificador del dispositivo al que se le hace el review


        public int ReviewId { get; set; }//Identificador del review al que pertenece el review item


        [Required(AllowEmptyStrings = false, ErrorMessage = "El comentario no puede estar vacío")]
        public string Comments { get; set; }//Comentario del review item


        public Device Device { get; set; }//Dispositivo al que se le hace el review


        public int Id { get; set; }//Identificador único del review item


        [Range(1, 5, ErrorMessage = "La calificación tiene que estar entre 1 y 5")]
        [Required]
        public int Rating { get; set; }//Calificación del dispositivo


        public Review Review { get; set; }//Review a la que pertenece el review item

    }
}
