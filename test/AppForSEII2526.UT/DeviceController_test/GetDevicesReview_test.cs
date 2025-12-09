using AppForSEII2526.API.Controllers;
using AppForSEII2526.API.DTOs.DeviceDTO;
using AppForSEII2526.API.DTOs.ReviewDTOs;
using AppForSEII2526.API.Models;
using Humanizer.Localisation;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppForSEII2526.UT.DeviceController_test {
    public class GetDevicesReview_test : AppForSEII25264SqliteUT {
        public GetDevicesReview_test() {

            var testmodel1 =
                new Model {
                    Id = 30,
                    NameModel = "Iphone16" };
            var testmodel2 =
                new Model {
                    Id = 31,
                    NameModel = "Samsung8" };
            var testmodel3 =
                new Model {
                    Id = 32,
                    NameModel = "PocoM3" };
            var testmodel4 =
                new Model {
                    Id = 33,
                    NameModel = "Iphone17" };
            

            _context.Add(testmodel1);
            _context.Add(testmodel2);
            _context.Add(testmodel3);
            _context.Add(testmodel4);
            
            var testUser = new ApplicationUser {
                UserName = "testuser@example.com", // Requerido por IdentityUser
                Email = "testuser@example.com",    // Requerido
                Name = "John",
                Surname = "Doe"
            };

            var testDevice1 =
                new Device("Negro", "Iphone", "Iphone 16 to guapo", 45.00, 1299.99, QualityType.Excelente, 2023, 50, 10, new List<ReviewItem>(), testmodel1, "Chip A17 Pro super potente", new List<PurchaseItem>(), new List<RentDevice>());
            var testDevice2 =
                new Device("Blanco", "Samsung", "Samsung 8 super", 30.50, 850.00, QualityType.Bueno, 2023, 5, 20, new List<ReviewItem>(), testmodel2, "Ultrabook ligero para trabajo", new List<PurchaseItem>(), new List<RentDevice>());
            var testDevice3 =
                new Device("Amarillo", "Poco", "Poco mucho", 25.00, 450.00, QualityType.Seminuevo, 2021, 2, 5, new List<ReviewItem>(), testmodel3, null, new List<PurchaseItem>(), new List<RentDevice>());
            var testDevice4 =
               new Device("Blanco", "Iphone", "Iphone 17 to guapo", 45.00, 1299.99, QualityType.Excelente, 2024, 50, 10, new List<ReviewItem>(), testmodel4, "Chip A18 Pro super potente", new List<PurchaseItem>(), new List<RentDevice>());
            

            _context.Add(testDevice1);
            _context.Add(testDevice2);
            _context.Add(testDevice3);
            _context.Add(testDevice4);





            _context.Add(testUser);
           
           
            
            _context.SaveChanges();
        }


        public static IEnumerable<object[]> TestCasesFor_GetDevicesForReview_OK() {

            var deviceDTOs = new List<DeviceParaReseñasDTO>() {
                new DeviceParaReseñasDTO(1,"Iphone 16 to guapo","Iphone","Negro",2023,"Iphone16"),
                new DeviceParaReseñasDTO(2,"Samsung 8 super","Samsung" ,"Blanco",2023,"Samsung8" ),
                new DeviceParaReseñasDTO(3, "Poco mucho", "Poco","Amarillo",2021,"PocoM3" ),
                new DeviceParaReseñasDTO(4,"Iphone 17 to guapo","Iphone","Blanco",2024,"Iphone17")
            };


            var deviceDTOsTC1 = new List<DeviceParaReseñasDTO>() { deviceDTOs[0], deviceDTOs[1], deviceDTOs[2], deviceDTOs[3] };


            var deviceDTOsTC2 = new List<DeviceParaReseñasDTO>() { deviceDTOs[0], deviceDTOs[3] };
            var deviceDTOsTC3 = new List<DeviceParaReseñasDTO>() { deviceDTOs[0], deviceDTOs[1] };

            var deviceDTOsTC4 = new List<DeviceParaReseñasDTO>() { deviceDTOs[0] };

            var allTests = new List<object[]>
            {             //filters to apply - expected devices
                                          
                new object[] { null, null, deviceDTOsTC1},
                new object[] { "Iphone", null, deviceDTOsTC2},
                new object[] { null, 2023, deviceDTOsTC3},
                new object[] { "Iphone", 2023, deviceDTOsTC4},
            };

            return allTests;
        }

        [Theory]
        [MemberData(nameof(TestCasesFor_GetDevicesForReview_OK))]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetDevicesForReview_OK_test(string? filterBrand, int? filterYear,
            IList<DeviceParaReseñasDTO> expectedDevices) {
            // Arrange
            var controller = new DeviceController(_context, null);

            // Act
            var result = await controller.GetDevicesParaReview(filterBrand, filterYear);

            //Assert
            //we check that the response type is OK 
            var okResult = Assert.IsType<OkObjectResult>(result);
            //and obtain the list of devices
            var deviceDTOsActual = Assert.IsType<List<DeviceParaReseñasDTO>>(okResult.Value);
            Assert.Equal(expectedDevices, deviceDTOsActual);

        }


        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetDeviceForReview_badrequest_test() {
            // Arrange
            var mock = new Mock<ILogger<DeviceController>>();
            ILogger<DeviceController> logger = mock.Object;
            var controller = new DeviceController(_context, logger);

            // Act
            var result = await controller.GetDevicesParaReview("",1999);

            //Assert
            //we check that the response type is OK and obtain the list of movies
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);
            var problem = problemDetails.Errors.First().Value[0];

            Assert.Equal("Error:Year cannot be under 2000", problem);
        }

    }
}