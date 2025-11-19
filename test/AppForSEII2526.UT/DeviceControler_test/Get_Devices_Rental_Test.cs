using AppForSEII2526.API.Controllers;
using AppForSEII2526.API.DTOs.DeviceDTO;
using AppForSEII2526.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AppForSEII2526.UT.DeviceControler_test {
    public class Get_Devices_Rental_Test : AppForSEII25264SqliteUT {
        // --- CORRECCIÓN: Inicializamos aquí arriba con 'static readonly' ---
        // Al ponerlo aquí, ya existen cuando se ejecuta el método TestCasesFor_...
        private static readonly DeviceParaRentDTO _device1DTO =
            new DeviceParaRentDTO(1, "Iphone 15 Pro", "Iphone-15", "Apple", 2023, "Black", 50.0);

        private static readonly DeviceParaRentDTO _device2DTO =
            new DeviceParaRentDTO(2, "Galaxy S24", "Galaxy-S24", "Samsung", 2024, "Grey", 45.0);

        public Get_Devices_Rental_Test() {
            // Aquí seguimos preparando la Base de Datos, eso está bien en el constructor
            var models = new List<Model>() {
                new Model("Iphone-15"),
                new Model("Galaxy-S24")
            };

            var devices = new List<Device>() {
                new Device(
                    color: "Black",
                    brand: "Apple",
                    name: "Iphone 15 Pro",
                    priceForRent: 50.0,
                    priceForPurchase: 1200.0,
                    quality: QualityType.Excelente,
                    year: 2023,
                    quantityForPurchase: 0,
                    quantityForRent: 10,
                    reviewItems: new List<ReviewItem>(),
                    model: models[0],
                    description: "Latest Apple device",
                    purchaseItems: new List<PurchaseItem>(),
                    deviceItems: new List<RentDevice>()
                ) { id = 1 },

                new Device(
                    color: "Grey",
                    brand: "Samsung",
                    name: "Galaxy S24",
                    priceForRent: 45.0,
                    priceForPurchase: 1000.0,
                    quality: QualityType.Bueno,
                    year: 2024,
                    quantityForPurchase: 0,
                    quantityForRent: 5,
                    reviewItems: new List<ReviewItem>(),
                    model: models[1],
                    description: "Latest Samsung device",
                    purchaseItems: new List<PurchaseItem>(),
                    deviceItems: new List<RentDevice>()
                ) { id = 2 }
            };

            _context.AddRange(models);
            _context.AddRange(devices);
            _context.SaveChanges();

            // HE BORRADO LAS ASIGNACIONES DE DTO QUE HABÍA AQUÍ
        }

        public static IEnumerable<object[]> TestCasesFor_GetDevicesForRent_OK() {
            // Ahora _device1DTO ya tiene datos y NO es null
            var caseFilterModel = new List<DeviceParaRentDTO>() { _device1DTO };
            var caseFilterPrice = new List<DeviceParaRentDTO>() { _device2DTO };
            var caseNoFilter = new List<DeviceParaRentDTO>() { _device1DTO, _device2DTO };

            var allTests = new List<object[]>
            {
                new object[] { "Iphone", null, caseFilterModel },
                new object[] { null, 45.0, caseFilterPrice },
                new object[] { null, null, caseNoFilter },
                new object[] { "Xiaomi", null, new List<DeviceParaRentDTO>() }
            };

            return allTests;
        }

        [Theory]
        [MemberData(nameof(TestCasesFor_GetDevicesForRent_OK))]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetDevicesParaRent_Filter_test(string? filterModel, double? filterPrice, IList<DeviceParaRentDTO> expectedDevices) {
            // Arrange
            var mockLogger = new Mock<ILogger<DeviceController>>();
            var controller = new DeviceController(_context, mockLogger.Object);

            // Act
            var result = await controller.GetDevicesParaRent(filterModel, filterPrice);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualDevices = Assert.IsAssignableFrom<IEnumerable<DeviceParaRentDTO>>(okResult.Value);

            // Comprobación exacta
            Assert.Equal(expectedDevices.Count(), actualDevices.Count());
            Assert.Equal(expectedDevices, actualDevices);
        }

        // El test simple de Fact (opcional mantenerlo)
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetDevicesParaRent_Success_test() {
            var mockLogger = new Mock<ILogger<DeviceController>>();
            var controller = new DeviceController(_context, mockLogger.Object);

            var result = await controller.GetDevicesParaRent(null, null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualDevicesDTO = Assert.IsAssignableFrom<IEnumerable<DeviceParaRentDTO>>(okResult.Value);

            Assert.Equal(2, actualDevicesDTO.Count());
        }
    }
}