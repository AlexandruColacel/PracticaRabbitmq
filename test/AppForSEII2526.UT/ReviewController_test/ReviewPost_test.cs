using AppForSEII2526.API.Controllers;
using AppForSEII2526.API.DTOs.RentalDTOs;
using AppForSEII2526.API.DTOs.ReviewDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AppForSEII2526.UT.ReviewController_test {
    public class ReviewPost_test : AppForSEII25264SqliteUT {


        private readonly ILogger<ReviewController> _logger;
        private ReviewDetailDTO _expectedDto;
        


        public ReviewPost_test() {
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
                ReviewId = 100,
                ApplicationUser = testUser
            };





            _context.Add(testUser);
            _context.Add(testModel);
            _context.Add(testDevice);
            _context.Add(testReview);
            _context.SaveChanges();


        }



        public static IEnumerable<object[]> TestCasesFor_CreateReview() {

            



            var allTests = new List<object[]> {




            };

            return allTests;
        
        }
        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        [MemberData(nameof(TestCasesFor_CreateReview))]
        public async Task CreateReview_Error_test(ReviewForCreateDTO reviewDTO, string errorExpected) {
            // Arrange
            var mock = new Mock<ILogger<ReviewController>>();
            ILogger<ReviewController> logger = mock.Object;

            var controller = new ReviewController(_context, logger);

            // Act
            var result = await controller.CreateReview(reviewDTO);

            //Assert
            //we check that the response type is BadRequest and obtain the error returned
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            var errorActual = problemDetails.Errors.First().Value[0];

            //we check that the expected error message and actual are the same
            Assert.StartsWith(errorExpected, errorActual);

        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateReview_Success_test() {
            // Arrange
            var mock = new Mock<ILogger<ReviewController>>();
            ILogger<ReviewController> logger = mock.Object;

            var controller = new ReviewController(_context, logger);

            DateTime to = DateTime.Today.AddDays(6);
            DateTime from = DateTime.Today.AddDays(7);

            var reviewDTO = new ReviewForCreateDTO(//_userName, _customerNameSurname,
                //_deliveryAddress, PaymentMethodTypes.CreditCard,
                //to, from, new List<RentalItemDTO>()
                //{ new RentalItemDTO(2, _movie1Title, _movie1Genre, 1.0) }
                );

            var expectedreviewDetailDTO = new ReviewDetailDTO(//2, DateTime.Now,
                //_userName, _customerNameSurname,
                //_deliveryAddress, PaymentMethodTypes.CreditCard,
                //to, from, new List<RentalItemDTO>()
                //{ new RentalItemDTO(2, _movie1Title, _movie1Genre, 1.0) }
                );

            // Act
            var result = await controller.CreateReview(reviewDTO);

            //Assert
            //we check that the response type is BadRequest and obtain the error returned
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var actualReviewDetailDTO = Assert.IsType<ReviewDetailDTO>(createdResult.Value);

            Assert.Equal(expectedreviewDetailDTO, actualReviewDetailDTO);

        }

    }
}
