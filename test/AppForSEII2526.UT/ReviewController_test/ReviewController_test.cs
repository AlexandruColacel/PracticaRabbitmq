using AppForSEII2526.API.Controllers;
using AppForSEII2526.API.DTOs.ReviewDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppForSEII2526.UT.ReviewController_test {
    public class ReviewController_test : AppForSEII25264SqliteUT {

        private readonly ILogger<ReviewController> _logger;
        private ReviewDetailDTO _expectedDto;
        private int _reviewId_OK = 1;
        private int _reviewId_NotFound = 999;

        public ReviewController_test() {


            var testUser = new ApplicationUser {
                UserName = "testuser@example.com", // Requerido por IdentityUser
                Email = "testuser@example.com",    // Requerido
                Name = "John",
                Surname = "Doe"
            };

            var testModel = new Model {
                Id = 30,
                NameModel = "SuperModelo"
            };

            var testDevice = new Device {
                id = 20,
                Name = "SupaMegaAmazingPhone",
                Brand = "TechBrand",
                Model = testModel,
                Color = "Black",
                PriceForPurchase = 499.99,
                QuantityForPurchase = 5,
                Description = "A high-end tech device."
            };


            var testReview = new Review {
                CustomerId = testUser.UserName,
                ReviewTitle = "Great Product",
                CustomerCountry = 1,
                DateOfReview = DateTime.Now,
                ReviewItems = new List<ReviewItem> {
                    new ReviewItem {
                        Device = testDevice,
                        Rating = 5,
                        Comments = "Loved it!"
                    }
                },
                ReviewId = _reviewId_OK,
            };
            //int deviceId, int reviewId, string comments, int id, int rating, Review review
            var testReviewItem = new ReviewItem {
                DeviceId = testDevice.id,
                ReviewId = testReview.ReviewId,
                Comments = "Loved it!",
                Id = 1,
                Rating = 5,
                Review = testReview,

            };

            testReview.ReviewItems.Add(testReviewItem);

            _context.Add(testUser);
            _context.Add(testModel);
            _context.Add(testDevice);
            _context.Add(testReview);
            _context.Add(testReviewItem);
            _context.SaveChanges();


        }



        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetPurchase_NotFound_test() {
            // Arrange
            var controller = new ReviewController(_context,_logger);

            // Act
            var result = await controller.GetReview(_reviewId_NotFound);

            // Assert
            // Comprobamos que el resultado es 'NotFoundResult' (HTTP 404)
            Assert.IsType<NotFoundResult>(result);
        }
    }
}