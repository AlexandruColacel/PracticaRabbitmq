namespace AppForSEII2526.API.DTOs.ReviewDTOs
{
    public class ReviewForCreateDTO
    {
        public ReviewForCreateDTO()
        {
        }
        
        public ReviewForCreateDTO(DateTime dateOfReview, string reviewTitle, string nombreCliente, int paisCliente, IList<ReviewItemDTO> reviewItems)
        {
            DateOfReview=dateOfReview;
            ReviewTitle = reviewTitle;
            CustomerId = nombreCliente;
            CustomerCountry = paisCliente;
            ReviewItems = reviewItems;


        }

        public DateTime DateOfReview { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please, enter a Title for the Review")]
        public string ReviewTitle { get; set; }

        public string CustomerId { get; set; }

        [Required(ErrorMessage = "Please, enter a Country for the Customer")]
        public int CustomerCountry { get; set; }

        public IList<ReviewItemDTO> ReviewItems { get; set; }


        public int OverallRating{
            get {
                return ReviewItems.Sum(ri => ri.Rating * ReviewItems.Count);
            }
        }



        protected bool CompareDate(DateTime date1, DateTime date2) {
            return (date1.Subtract(date2) < new TimeSpan(0, 1, 0));
        }

        public override bool Equals(object? obj) {
            return obj is ReviewForCreateDTO dTO &&
                   CompareDate(DateOfReview, dTO.DateOfReview) &&
                   ReviewTitle == dTO.ReviewTitle &&
                   CustomerId == dTO.CustomerId &&
                   CustomerCountry == dTO.CustomerCountry &&
                   ReviewItems.SequenceEqual(dTO.ReviewItems);

        }


        

    }
}
