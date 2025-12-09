using AppForSEII2526.API.Controllers;
using AppForSEII2526.API.DTOs.RentalDTOs;
using AppForSEII2526.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq; // Asegúrate de tener Moq instalado
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit; // Asegúrate de tener xUnit

namespace AppForSEII2526.UT.RentalController_test {
    public class POSTRental_test : AppForSEII25264SqliteUT {
        // Constantes para usar en los casos de prueba estáticos
        private const string _userName = "LexG@uclm.es";
        private const string _customerNameSurname = "Lex G";
        private const string _deliveryAddressbad = "Avda. España s/n, Albacete 02071";
        private const string _deliveryAddress = "Calle España s/n, Albacete 02071";

        private const string _model1Brand = "iphone";
        private const string _model1Model = "Iphone-3x";
        private const string _model2Brand = "Google";
        private const string _model2Model = "Google-4y";

        // IDs fijos para asegurar que coinciden en los tests
        private const int _device1Id = 1;
        private const int _device2Id = 2;

        public POSTRental_test() {
            var models = new List<Model>() {
                new Model(_model1Model),
                new Model(_model2Model),
            };

            var devices = new List<Device>(){
                 new Device(
                    color: "Black",
                    brand: _model1Brand,
                    name: _model1Model,
                    priceForRent: 50.0,
                    priceForPurchase: 1200.0,
                    quality: QualityType.Excelente,
                    year: 2022,
                    quantityForPurchase: 5,
                    quantityForRent: 10, // NOTA: Hay 10 disponibles
                    reviewItems: new List<ReviewItem>(),
                    model: models[0],
                    description: "Latest iPhone model with advanced features",
                    purchaseItems: new List<PurchaseItem>(),
                    deviceItems: new List<RentDevice>()
                ) { id = _device1Id }, // Forzamos ID
                new Device(
                    color: "White",
                    brand: _model2Brand,
                    name: _model2Model,
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
                ) { id = _device2Id } // Forzamos ID
            };

            ApplicationUser user = new ApplicationUser() {
                Id = "7",
                Name = "Lex",
                Surname = "G",
                UserName = _userName,
                Email = "lex@uclm.es"
            };

            // Añadimos un alquiler previo para tener datos en la BD, 
            // aunque para los tests de POST usaremos datos nuevos.
            var rental = new Rental {
                Name = "Lex",
                Surname = "G",
                ApplicationUser = user,
                DeliveryAddress = _deliveryAddress,
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
            // Calculamos precio (opcional para setup, pero bueno para consistencia)
            //rental.TotalPrice = ... (no es estrictamente necesario para el setup del POST test)

            _context.ApplicationUsers.Add(user);
            _context.AddRange(models);
            _context.AddRange(devices);
            _context.Add(rental);
            _context.SaveChanges();
        }

        public static IEnumerable<object[]> TestCasesFor_CreatePurchase() {
            // 1. Caso sin items
            var rentalNoITem = new RentalPostDTO(_userName, _customerNameSurname,
                _deliveryAddress, PaymentMethodTypes.Tarjeta,
                DateTime.Today.AddDays(2), DateTime.Today.AddDays(5), new List<RentalItemDTO>());

            // Items válidos para reusar en otros casos
            // Constructor RentalItemDTO: (id, model, brand, price, quantity)
            var rentalItems = new List<RentalItemDTO>() {
                new RentalItemDTO(_device2Id, _model2Model, _model2Brand, 40.0, 1)
            };

            // 2. Fecha inicio hoy (inválida según tu controller)
            var rentalFromBeforeToday = new RentalPostDTO(_userName, _customerNameSurname,
                _deliveryAddress, PaymentMethodTypes.Tarjeta,
                DateTime.Today, DateTime.Today.AddDays(5), rentalItems);

            // 3. Fecha fin antes que inicio
            var rentalToBeforeFrom = new RentalPostDTO(_userName, _customerNameSurname,
                _deliveryAddress, PaymentMethodTypes.Tarjeta,
                DateTime.Today.AddDays(5), DateTime.Today.AddDays(2), rentalItems);

            // 4. Usuario no registrado
            var RentalApplicationUser = new RentalPostDTO("victor.lopez@uclm.es", _customerNameSurname,
                _deliveryAddress, PaymentMethodTypes.Tarjeta,
                DateTime.Today.AddDays(2), DateTime.Today.AddDays(4), rentalItems);

            // 5. Dispositivo no disponible (Pedimos más cantidad de la que hay)
            // El dispositivo 1 tiene QuantityForRent = 10. Pedimos 20.
            var rentalDeviceNotAvailable = new RentalPostDTO(_userName, _customerNameSurname,
                _deliveryAddress, PaymentMethodTypes.Tarjeta,
                DateTime.Today.AddDays(2), DateTime.Today.AddDays(5),
                new List<RentalItemDTO>() {
                    new RentalItemDTO(_device1Id, _model1Model, _model1Brand, 50.0, 20) // 20 > 10 disponibles
                });
            //Objeto con mala direccion de envio(No comienza por carretra)
            var rentalBadDeliveryAddres = new RentalPostDTO(
                _userName,
                _customerNameSurname,
                _deliveryAddressbad, //En este caso utilizo esta porque me sirve ya que arriba tengo declado que es "Avenida de españa... por lo cual no es correcta"
                PaymentMethodTypes.Tarjeta,
                DateTime.Today.AddDays(2),
                DateTime.Today.AddDays(7),
                rentalItems //USO ESTE YA QUE NO NECESITO UN DISPOSITIVO INCORRECTO EN ESTE CASO YA QUE CONTROLO QUE LA DIRECCION NO SEA NULL
                );

            // IMPORTANTE: Los mensajes de error deben ser IDÉNTICOS a los de RentalController.cs
            var allTests = new List<object[]>
            {
                new object[] { rentalNoITem, "Error! You must include at least one device to be rented",  },
                new object[] { rentalFromBeforeToday, "Error! Your rental date must start later than today", },
                new object[] { rentalToBeforeFrom, "Error! Your rental must end later than it starts", },
                new object[] { RentalApplicationUser, "Error! UserName is not registered", },
                // Mensaje dinámico basado en tu controlador: $"Error! Device '{brand} {model}' only has..."
                new object[] { rentalDeviceNotAvailable, $"Error! Device '{_model1Brand} {_model1Model}' only has 9 units available" },
                //new object[] {rentalBadNullDeliveryAddres, "Error en la direccion de envio.Por favor, introduce una direccion valida incluyendo las palabras Calle o Carretera" },
                new object[] {rentalBadDeliveryAddres, "Error en la direccion de envio.Por favor, introduce una direccion valida incluyendo las palabras Calle o Carretera" }
                // Nota: Dice "9 units" porque en el setup() ya alquilamos 1 unidad del device 1.
                // QuantityForRent(10) - NumberOfRentedDevices(1) = 9 disponibles.
            };

            return allTests;
        }

        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        [MemberData(nameof(TestCasesFor_CreatePurchase))]
        public async Task CreateRental_Error_test(RentalPostDTO rentalDTO, string errorExpected) {
            // Arrange
            var mock = new Mock<ILogger<RentalController>>();
            ILogger<RentalController> logger = mock.Object;

            var controller = new RentalController(_context, logger);

            // Act
            var result = await controller.CreateRental(rentalDTO);

            //Assert
            // Chequeamos que sea BadRequest
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            // Obtenemos el primer error
            var errorActual = problemDetails.Errors.First().Value[0];

            // Verificamos que comience con el mensaje esperado
            Assert.StartsWith(errorExpected, errorActual);
        }

        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreateRental_Success_test() {
            // Arrange
            var mock = new Mock<ILogger<RentalController>>();
            ILogger<RentalController> logger = mock.Object;

            var controller = new RentalController(_context, logger);

            DateTime from = DateTime.Today.AddDays(6);
            DateTime to = DateTime.Today.AddDays(7);
            int quantity = 1;

            // IMPORTANTE: Usamos _device2Id en lugar de _device1Id
            // El Device 1 ya tiene un alquiler creado en el constructor y causa conflicto de tracking.
            // El Device 2 está libre y limpio para este test.
            var rentalDTO = new RentalPostDTO(_userName, _customerNameSurname,
                _deliveryAddress, PaymentMethodTypes.Tarjeta,
                from, to, new List<RentalItemDTO>()
                { 
                    // Usamos _device2Id, _model2Model, _model2Brand
                    new RentalItemDTO(_device2Id, _model2Model, _model2Brand, 40.0, quantity)
                });

            var expectedRentalDetails = new RentalDetailsDTO(
                0,
                DateTime.Now,
                _userName,
                _customerNameSurname,
                _deliveryAddress,
                PaymentMethodTypes.Tarjeta,
                from,
                to,
                new List<RentalItemDTO>() {
                     new RentalItemDTO(_device2Id, _model2Model, _model2Brand, 40.0, quantity)
                },
                40.0 // Precio del device 2 es 40.0 * 1 día = 40.0
            );

            // Act
            var result = await controller.CreateRental(rentalDTO);

            //Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var actualRentalDetailDTO = Assert.IsType<RentalDetailsDTO>(createdResult.Value);

            Assert.Equal(expectedRentalDetails.CustomerUserName, actualRentalDetailDTO.CustomerUserName);
            Assert.Equal(expectedRentalDetails.TotalPrice, actualRentalDetailDTO.TotalPrice);
            Assert.Single(actualRentalDetailDTO.RentalItems);
        }
    }
    
}