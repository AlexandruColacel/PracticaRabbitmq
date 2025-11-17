using AppForSEII2526.API.Controllers;
using AppForSEII2526.API.DTOs.RentalDTOs;
using AppForSEII2526.API.DTOs.ReviewDTOs;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppForSEII2526.UT.ReviewController_test {
    public class ReviewController_test : AppForSEII25264SqliteUT {

        private readonly ILogger<ReviewController> _logger;
        private ReviewDetailDTO _expectedDto;
        private int _reviewId_OK = 100;
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
                Description = "A high-end tech device.",
                Year = 2023
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
        [Trait("Database", "WithoutFixture")]
        public async Task GetPurchase_NotFound_test() {
            // Arrange

            var mock = new Mock<ILogger<ReviewController>>();
            ILogger<ReviewController> logger = mock.Object;


            var controller = new ReviewController(_context, logger);

            // Act
            var result = await controller.GetReview(0);

            // Assert
            // Comprobamos que el resultado es 'NotFoundResult' (HTTP 404)
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetRental_Found_test() {
            //arranque
            var mock = new Mock<ILogger<ReviewController>>();
            ILogger<ReviewController> logger = mock.Object;
            var controller = new ReviewController(_context, logger);

            var expectedReview = new ReviewDetailDTO(100,DateTime.Now, "Great Product", "John",1, new List<ReviewItemDTO>());
            //int deviceId, string deviceName,  string modelName, int deviceYear,int rating, string comments
            expectedReview.ReviewItems.Add(new ReviewItemDTO(20, "SupaMegaAmazingPhone", "SuperModelo",2023,5, "Loved it!"));


            // Act 
            var result = await controller.GetReview(1);

            //Assert
            //we check that the response type is OK and obtain the rental
            var okResult = Assert.IsType<OkObjectResult>(result);
            var rentalDTOActual = Assert.IsType<ReviewDetailDTO>(okResult.Value);
            var eq = expectedReview.Equals(rentalDTOActual);
            //we check that the expected and actual are the same
            Assert.Equal(expectedReview, rentalDTOActual);




        }




    }
}