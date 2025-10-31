using Humanizer.Localisation;

namespace AppForSEII2526.API.DTOs.RentalDTOs
{
    public class ReviewItemDTO
    {
        public ReviewItemDTO(int deviceId, string deviceName,  string modelName, int deviceYear,int rating, string comments)//puntuación y comentario
        {
            DeviceId = deviceId;
            DeviceName = deviceName;
            ModelName = modelName;
            DeviceYear = deviceYear;
            Rating = rating;
            Comments = comments;

        }
        public int DeviceId { get; set; }//Identificador del dispositivo al que se le hace el review
        public string DeviceName { get; set; }//Nombre del dispositivo
        public string ModelName { get; set; }//Modelo del dispositivo
        public int DeviceYear { get; set; }//Año del dispositivo
        public string Comments { get; set; }//Comentario del review item
        public int Rating { get; set; }//Calificación del dispositivo
        public override bool Equals(object? obj)
        {
            return obj is ReviewItemDTO dTO &&
                   DeviceId == dTO.DeviceId &&
                   DeviceName == dTO.DeviceName &&
                   ModelName == dTO.ModelName &&
                   DeviceYear == dTO.DeviceYear &&
                   Rating == dTO.Rating &&
                   Comments == dTO.Comments;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DeviceId, DeviceName, ModelName, DeviceYear,Rating , Comments);
        }


    }
}
