namespace AppForSEII2526.API.DTOs.ReviewDTOs
{
    public class ReviewForCreateDTO
    {
        public ReviewForCreateDTO()
        {
        }

        public ReviewForCreateDTO(string customerId, string reviewTitle, List<ReviewItemDTO> reviewItems, int customerCountry, DateTime dateOfReview, int overallRating, int reviewId)
        {
            CustomerId = customerId;
            ReviewTitle = reviewTitle;
            ReviewItems = reviewItems;
            CustomerCountry = customerCountry;
            DateOfReview = dateOfReview;
            OverallRating = overallRating;
            ReviewId = reviewId;


        }

        public string CustomerId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please, enter a Title for the Review")]
        public string ReviewTitle { get; set; }
        
        public List<ReviewItemDTO> ReviewItems { get; set; }

        [Required(ErrorMessage = "Please, enter a Country for the Customer")]
        public int CustomerCountry { get; set; }
        
        public DateTime DateOfReview { get; set; }
        
        public int OverallRating { get; set; }
        
        public int ReviewId { get; set; }




    }
}
