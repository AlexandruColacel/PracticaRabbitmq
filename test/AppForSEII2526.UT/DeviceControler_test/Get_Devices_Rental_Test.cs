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
        // CONSTRUCTOR: PREPARO EL ESCENARIO (DATABASE)
        public Get_Devices_Rental_Test() {
            // 1. Creo los Modelos (necesarios para que la navegación d.Model.NameModel funcione)
            var models = new List<Model>() {
                new Model("Iphone-15"),
                new Model("Galaxy-S24")
            };

            // 2. Creo los Dispositivos completos (Entities)
            // Uso datos específicos (ID, Precio) que luego esperaré en los DTOs.
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

            // 3. Guardo todo en mi contexto de base de datos en memoria
            _context.AddRange(models);
            _context.AddRange(devices);
            _context.SaveChanges();
        }

        // MÉTODO ESTÁTICO PARA DEFINIR LOS CASOS DE PRUEBA (ESTILO PROFESORA)
        public static IEnumerable<object[]> TestCasesFor_GetDevicesForRent_OK() {
            // PASO 1: Creo los DTOs esperados AQUÍ MISMO (Localmente).
            // Así evito problemas de variables estáticas nulas.
            var deviceDTOs = new List<DeviceParaRentDTO>() {
                new DeviceParaRentDTO(1, "Iphone 15 Pro", "Iphone-15", "Apple", 2023, "Black", 50.0),
                new DeviceParaRentDTO(2, "Galaxy S24", "Galaxy-S24", "Samsung", 2024, "Grey", 45.0)
            };

            // PASO 2: Defino sublistas para cada escenario.
            var devicesTC1 = new List<DeviceParaRentDTO>() { deviceDTOs[0] }; // Solo Iphone
            var devicesTC2 = new List<DeviceParaRentDTO>() { deviceDTOs[1] }; // Solo Samsung
            var devicesTC3 = new List<DeviceParaRentDTO>() { deviceDTOs[0], deviceDTOs[1] }; // Ambos

            // PASO 3: Devuelvo los casos para el [Theory].
            var allTests = new List<object[]>
            {
                new object[] { "Iphone", null, devicesTC1 }, // Filtro por texto
                new object[] { null, 45.0, devicesTC2 },     // Filtro por precio
                new object[] { null, null, devicesTC3 },     // Sin filtros
                new object[] { "Xiaomi", null, new List<DeviceParaRentDTO>() } // Filtro sin resultados
            };

            return allTests;
        }

        // TEST 1: FILTRADO CORRECTO (THEORY)
        [Theory]
        [MemberData(nameof(TestCasesFor_GetDevicesForRent_OK))]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetDevicesParaRent_Filter_test(string? filterModel, double? filterPrice, IList<DeviceParaRentDTO> expectedDevices) {
            // Arrange
            // Creo un Mock del Logger porque el controlador lo exige en el constructor.
            var mockLogger = new Mock<ILogger<DeviceController>>();
            var controller = new DeviceController(_context, mockLogger.Object);

            // Act
            // Llamo al método con los filtros de este caso de prueba.
            var result = await controller.GetDevicesParaRent(filterModel, filterPrice);

            // Assert
            // 1. Verifico que la respuesta es 200 OK.
            var okResult = Assert.IsType<OkObjectResult>(result);

            // 2. Extraigo la lista de dispositivos devuelta.
            var actualDevices = Assert.IsType<List<DeviceParaRentDTO>>(okResult.Value);

            // 3. Comparo que la lista recibida sea IDÉNTICA a la esperada.
            Assert.Equal(expectedDevices, actualDevices);
        }

        // TEST 2: VALIDACIÓN DE ERRORES (BAD REQUEST)
        // Este test comprueba que si envío datos inválidos (precio negativo), la API se protege.
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetDevicesParaRent_BadRequest_test() {
            // Arrange
            var mock = new Mock<ILogger<DeviceController>>();
            ILogger<DeviceController> logger = mock.Object;
            var controller = new DeviceController(_context, logger);

            // Act: Llamo con un precio negativo (-50.0), lo cual está prohibido por mi lógica.
            var result = await controller.GetDevicesParaRent(null, -50.0);

            // Assert
            // 1. Compruebo que devuelve BadRequest (Código 400).
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            // 2. Compruebo que el detalle del error es del tipo correcto (ValidationProblemDetails).
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            // 3. Busco el mensaje de error específico del campo "RentPrice".
            var errorActual = problemDetails.Errors["RentPrice"][0];

            // 4. Verifico que el mensaje es exactamente el que programé en el controlador.
            Assert.Equal("Error! RentPrice cannot be negative", errorActual);
        }
    }
}