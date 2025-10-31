using Microsoft.AspNetCore.Identity;

namespace AppForSEII2526.API.Models
{

    // Add profile data for application users by adding properties to the ApplicationUser class
    //he añadido nombre y apellido a la clase user 
    //Lo siguiente que voy a hacer 
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
        }

        [Display(Name = "Nombre")]
        public string Name
        {
            get;
            set;
        }

        [Display(Name = "Apellido")]
        public string Surname
        {
            get;
            set;
        }
        //FK con Rental
        public IList<Rental> Rentals { get; set; }

        //FK con Purchase (1:N)
        public IList<Purchase> Purchases { get; set; }

        public IList<Review> Reviews { get; set; }
    }
}