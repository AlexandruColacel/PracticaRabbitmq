using AppForSEII2526.API.Controllers;
using Humanizer.Localisation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppForSEII2526.UT.RentalController_test {
    public class GetRental_test : AppForSEII25264SqliteUT {
        public GetRental_test() {
            //ARRANGE START
            var models = new List<Model>() {
                new Model("Iphone-3x"),
                new Model("Google-4y"),
            };

            var devices = new List<Device>(){
                new Device(
                    color: "Black",
                    brand: "Apple",
                    name: "iPhone 14 Pro",
                    priceForRent: 50.0,
                    priceForPurchase: 1200.0,
                    quality: QualityType.Excelente,
                    year: 2022,
                    quantityForPurchase: 5,
                    quantityForRent: 10,
                    reviewItems: new List<ReviewItem>(),
                    model: models[0],
                    description: "Latest iPhone model with advanced features",
                    purchaseItems: new List<PurchaseItem>(),
                    deviceItems: new List<RentDevice>()
                ),
                new Device(
                    color: "White",
                    brand: "Google",
                    name: "Pixel 7",
                    priceForRent: 40.0,
                    priceForPurchase: 900.0,
                    quality: QualityType.Seminuevo,
                    year: 2023,
                    quantityForPurchase: 0,
                    quantityForRent: 15,
                    reviewItems: new List<ReviewItem>(),
                    model: models[1],
                    description: "Google flagship phone with clean Android",
                    purchaseItems: new List<PurchaseItem>(),
                    deviceItems: new List<RentDevice>()
                ),
            };

            ApplicationUser user = new ApplicationUser() {
                Id = "7",
                Name = "Lex",
                Surname = "G",
                UserName = "LexG@uclm.es",
                Email = "lex@uclm.es"
            };

            var rental = new Rental {
                Name = "Lex",
                Surname = "G",
                ApplicationUser = user,
                DeliveryAddress = "Avda. España s/n, Albacete 02071",
                RentalDate = DateTime.Now,
                PaymentMethod = PaymentMethodTypes.Tarjeta,
                RentalDateFrom = DateTime.Today.AddDays(2),
                RentalDateTo = DateTime.Today.AddDays(5),
                RentDevices = new List<RentDevice>()
            };

            var rentDevice = new RentDevice {
                Device = devices[0],
                DeviceId = devices[0].id,
                Rental = rental,
                RentId = rental.Id,
                Price = devices[0].PriceForRent,
                Quantity = 1
            };

            rental.RentDevices.Add(rentDevice);
            rental.TotalPrice = rentDevice.Price * rentDevice.Quantity *
                               (rental.RentalDateTo.Value - rental.RentalDateFrom.Value).Days;

            _context.ApplicationUsers.Add(user);
            _context.AddRange(models);
            _context.AddRange(devices);
            _context.Add(rental);
            _context.SaveChanges();
            //ARRANGE FIN
        }
        [Fact]
        [Trait("Database", "WithoutFixture")]
        [Trait("LevelTesting", "Unit Testing")]
        public async Task GetRental_NotFound_test() {
            // Arrange
            var mock = new Mock<ILogger<RentalController>>();
            ILogger<RentalController> logger = mock.Object;

            var controller = new RentalController(_context, logger);

            // Act
            var result = await controller.GetRental(0);

            //Assert
            //we check that the response type is OK and obtain the list of movies
            Assert.IsType<NotFoundResult>(result);

        }

    }
}