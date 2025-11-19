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

        private const string _userName = "morenito.19@uclm.es";
        private const string _Name = "John Po";

        private const string _Device1Title = "Device M";
        private const string _Device1Model = "Motorola GG";
        private const string _Device2Title = "The man in the high castle";
        private const string _Device2Model = "Drama";
        private const string _ReviewTitle = "Great Product";

        private readonly ILogger<ReviewController> _logger;
        private ReviewDetailDTO _expectedDto;
        


        public ReviewPost_test() {
            var testUser = new ApplicationUser {
                UserName = _userName, // Requerido por IdentityUser
                Email = "testuser@example.com",    // Requerido
                Name = _Name,
                Surname = "Doe"
            };

            var testModel = new Model {
                Id = 30,
                NameModel = _Device1Model
            };

            var testDevice = new Device {
                id = 20,
                Name = _Device1Title,
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
                ReviewTitle = _ReviewTitle,
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

            var reviewItems = new List<ReviewItemDTO> {
                new ReviewItemDTO( _Device1Title, _Device1Model,2023,5,"Excellent device"),
            };

            var reviewNoITem = new ReviewForCreateDTO(DateTime.Now, _ReviewTitle, _userName,1, new List<ReviewItemDTO>());
            
            var reviewInvalidUser = new ReviewForCreateDTO(DateTime.Now, _ReviewTitle, "novalido@uclm.es",1,reviewItems);

            var reviewInvalidCountry = new ReviewForCreateDTO(DateTime.Now, _ReviewTitle, _userName,99,reviewItems);

            var allTests = new List<object[]> {
                // validar que haya al menos un reviewitem
                new object[] { reviewNoITem, "Debe haber al menos un elemento en la reseña" },
                // validar usuario
                new object[] { reviewInvalidUser, "El usuario no existe" },
                // Validar pais
                new object[] { reviewInvalidCountry, "El país no es válido" }

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


            //DateTime dateOfReview, string reviewTitle, string nombreCliente, int paisCliente, IList<ReviewItemDTO> reviewItems
            var reviewDTO = new ReviewForCreateDTO(DateTime.Now, _ReviewTitle, _userName, 1, new List<ReviewItemDTO>()
                { new ReviewItemDTO( _Device1Title, _Device1Model,2023,5,"Excellent device") }
                );


            //nt id, DateTime dateOfReview, string reviewTitle, string nombreCliente, int paisCliente, IList<ReviewItemDTO> reviewItems
            var expectedreviewDetailDTO = new ReviewDetailDTO(2, DateTime.Now,_ReviewTitle, _userName, 1, new List<ReviewItemDTO>()
                { new ReviewItemDTO( _Device1Title, _Device1Model,2023,5,"Excellent device") }
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
