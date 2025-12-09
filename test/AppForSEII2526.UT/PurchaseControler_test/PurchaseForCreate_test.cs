using AppForSEII2526.API.Controllers;
using AppForSEII2526.API.DTOs.PurchaseDTO;
using AppForSEII2526.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AppForSEII2526.UT.PurchaseControler_test
{
    public class PurchaseForCreate_test : AppForSEII25264SqliteUT //Sin esto el context es null //Estamos heredando de la clase base que crea el contexto en memoria
    {
        //Atributos para las pruebas / datos constantes
        private readonly ILogger<PurchaseControler> _logger;
        private const string _userName = "testuser@example.com";
        private const string _userSurname = "Doe";
        private const string _deliveryAddress = "123 Tech Street, Albacete";
        private const int _device1Id = 20;
        private const int _device2Id = 21; // Dispositivo sin stock o para otra prueba
        private const int _device3Id = 22; //Device modelo invalido 1
        private const int _device4Id = 23; //Device modelo invalido 2

        public PurchaseForCreate_test()
        {
            //Mock del logger
            _logger = new Mock<ILogger<PurchaseControler>>().Object;

            // 1. Crear datos de prueba en la BBDD en memoria
            var user = new ApplicationUser
            {
                UserName = _userName,
                Email = _userName,
                Name = "John",
                Surname = _userSurname
            };

            var model = new Model
            {
                Id = 30,
                NameModel = "SuperModelo"
            };

            //MODIFICACIONES DE EXAMEN SPRINT 2
            var modelInvalido1 = new Model
            {
                //Xiaomi o Huawei no me sirven
                Id = 81,
                NameModel = "Huawei"
            };

            //MODIFICACIONES DE EXAMEN SPRINT 2
            var modelInvalido2 = new Model
            {
                Id = 82,
                NameModel = "Xiaomi"
            };

            // Dispositivo con stock suficiente
            var device1 = new Device
            {
                id = _device1Id,
                Name = "SupaMegaAmazingPhone",
                Brand = "TechBrand",
                Model = model,
                Color = "Black",
                PriceForPurchase = 499.99,
                QuantityForPurchase = 10, // Stock: 10
                Description = "A high-end tech device."
            };

            // Dispositivo SIN stock (para probar validaciones)
            var deviceNoStock = new Device
            {
                id = _device2Id,
                Name = "OutdatedPhone",
                Brand = "OldBrand",
                Model = model,
                Color = "White",
                PriceForPurchase = 199.99,
                QuantityForPurchase = 0, // Stock: 0
                Description = "An old device."
            };

            // Dispositivo con modelo invalido 1 (EXAMEN)
            var deviceModelInvalido1 = new Device
            {
                id = _device3Id,
                Name = "Xiaomi Phone",
                Brand = "Xiaomi",
                Model = modelInvalido1,
                Color = "White",
                PriceForPurchase = 199.99,
                QuantityForPurchase = 5,
                Description = "XiaomiDevice."
            };

            // Dispositivo con modelo invalido 2 (EXAMEN)
            var deviceModelInvalido2 = new Device
            {
                id = _device4Id,
                Name = "Huawei Phone",
                Brand = "Huawei",
                Model = modelInvalido2,
                Color = "White",
                PriceForPurchase = 199.99,
                QuantityForPurchase = 5,
                Description = "HuaweiDevice."
            };

            // Añadir al contexto
            _context.Add(user);
            _context.Add(model);
            _context.Add(device1);
            _context.Add(deviceNoStock);
            _context.Add(deviceModelInvalido1);
            _context.Add(deviceModelInvalido2);
            _context.SaveChanges();
        }// Del constructor

        //Tras el constructor, van los métodos de prueba, pero en este caso, es mejor primero declarar casos del prueba para errores comunes
        // --- CASOS DE PRUEBA PARA ERRORES (BadRequest) ---

        public static IEnumerable<object[]> TestCasesFor_CreatePurchase_Error()
        {
            // 1. Lista de items nula o vacía
            var purchaseNoItems = new PurchaseForCreateDTO(
                _userName, _userSurname, _deliveryAddress, PaymentMethod.TarjetaCredito,
                new List<PurchaseItemDTO>() // Lista vacía
            );

            // 2. Usuario no registrado
            var purchaseUserNotRegistered = new PurchaseForCreateDTO(
                "fakeuser@example.com", _userSurname, _deliveryAddress, PaymentMethod.TarjetaCredito,
                new List<PurchaseItemDTO> { new PurchaseItemDTO { Id = _device1Id, Quantity = 1 } }
            );

            // 3. Dispositivo no existe
            var purchaseDeviceNotExist = new PurchaseForCreateDTO(
                _userName, _userSurname, _deliveryAddress, PaymentMethod.TarjetaCredito,
                new List<PurchaseItemDTO> { new PurchaseItemDTO { Id = 999, Quantity = 1 } } // ID 999 no existe
            );

            // 4. Cantidad <= 0
            var purchaseQuantityZero = new PurchaseForCreateDTO(
                _userName, _userSurname, _deliveryAddress, PaymentMethod.TarjetaCredito,
                new List<PurchaseItemDTO> { new PurchaseItemDTO { Id = _device1Id, Quantity = 0 } }
            );

            // 5. Sin stock suficiente
            var purchaseOutOfStock = new PurchaseForCreateDTO(
                _userName, _userSurname, _deliveryAddress, PaymentMethod.TarjetaCredito,
                new List<PurchaseItemDTO> { new PurchaseItemDTO { Id = _device2Id, Quantity = 1 } } // device2 tiene stock 0
            );

            //Modelo invalido 1
            var purchaseInvalidModel1 = new PurchaseForCreateDTO(
                _userName, _userSurname, _deliveryAddress, PaymentMethod.TarjetaCredito,
                new List<PurchaseItemDTO> { new PurchaseItemDTO { Id = _device3Id, Quantity = 1, Brand = "Xiaomi" } } 
            );

            //Modelo invalido 2

            var purchaseInvalidModel2 = new PurchaseForCreateDTO(
                _userName, _userSurname, _deliveryAddress, PaymentMethod.TarjetaCredito,
                new List<PurchaseItemDTO> { new PurchaseItemDTO { Id = _device4Id, Quantity = 1, Brand = "Huawei" } } 
            );

            // Retornamos: [DTO de entrada, Mensaje de error esperado (o parte de él)]
            return new List<object[]>
            {
                new object[] { purchaseNoItems, "You must include at least one device to purchase." },
                new object[] { purchaseUserNotRegistered, "UserName is not registered." },
                new object[] { purchaseDeviceNotExist, "Device with id 999 does not exist." },
                new object[] { purchaseQuantityZero, "Quantity for device TechBrand SuperModelo must be greater than 0." }, // Asumiendo que el mensaje incluye marca/modelo
                new object[] { purchaseOutOfStock, "Not enough stock for device 'OldBrand SuperModelo'" },
                new object[] { purchaseInvalidModel1, "Invalid brand 'Xiaomi'."}, //(EXAMEN)
                new object[] { purchaseInvalidModel2, "Invalid brand 'Huawei'." }  //(EXAMEN)
            };

        }//De la lista de casos de prueba

        // --- MÉTODO DE PRUEBA PARA ERRORES EN CREATE PURCHASE ---
        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        [MemberData(nameof(TestCasesFor_CreatePurchase_Error))]
        public async Task CreatePurchase_Error_test(PurchaseForCreateDTO purchaseDTO, string errorExpected)
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PurchaseControler>>();
            var controller = new PurchaseControler(_context, mockLogger.Object);

            // Act
            var result = await controller.CreatePurchase(purchaseDTO);

            // Assert
            // 1. Verificamos que devuelve BadRequest
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            // 2. Verificamos que devuelve ValidationProblemDetails
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            // 3. Buscamos el primer error y comparamos el mensaje
            // Nota: problemDetails.Errors es un Diccionario <string, string[]>. 
            // First().Value[0] obtiene el primer mensaje del primer error encontrado.
            var errorActual = problemDetails.Errors.First().Value[0];

            Assert.Contains(errorExpected, errorActual); // Usamos Contains para ser flexibles con el formato exacto
        }//De pruebas caso error


        // --- MÉTODO DE PRUEBA PARA CREATE PURCHASE EXITOSO ---
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task CreatePurchase_Success_test()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<PurchaseControler>>();
            var controller = new PurchaseControler(_context, mockLogger.Object);

            int quantityToBuy = 2;

            // DTO de entrada
            var purchaseForCreate = new PurchaseForCreateDTO(
                _userName,
                _userSurname,
                _deliveryAddress,
                PaymentMethod.TarjetaCredito,
                new List<PurchaseItemDTO>
                {
                    new PurchaseItemDTO
                    {
                        Id = _device1Id,
                        Quantity = quantityToBuy,
                        Description = "Compra exitosa"
                    }
                }
            );

            // Construimos el DTO que ESPERAMOS recibir como respuesta.
            // Nota: El ID de la compra será 1 porque es la primera que se crea en este contexto de prueba.
            // Nota 2: El precio total se calcula: 499.99 * 2 = 999.98
            var expectedItemDto = new PurchaseItemDTO(
                _device1Id,
                "TechBrand",
                "SuperModelo",
                "Black",
                499.99m, // Precio unitario (decimal)
                quantityToBuy,
                "Compra exitosa"
            );

            var expectedPurchaseDetails = new PurchaseDetailsDTO(
                1, // ID esperado (primera inserción en la tabla Purchase)
                _userName,
                _userSurname,
                _deliveryAddress,
                DateTime.UtcNow, // Fecha aproximada, se validará con tolerancia en Equals
                999.98m, // Total Price
                quantityToBuy, // Total Quantity
                new List<PurchaseItemDTO> { expectedItemDto }
            );

            // Act
            var result = await controller.CreatePurchase(purchaseForCreate);

            // Assert
            // 1. Verificar CreatedAtActionResult (201)
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);

            // 2. Verificar que retorna PurchaseDetailsDTO
            var actualDto = Assert.IsType<PurchaseDetailsDTO>(createdResult.Value);

            // 3. Comparación
            // Dado que implementaste Equals() en PurchaseDetailsDTO y PurchaseItemDTO (aunque comentaste partes),
            // Assert.Equal usará tu implementación de Equals.
            // Asegúrate de que tu Equals permite la tolerancia de fecha y compara las listas correctamente.

            Assert.Equal(expectedPurchaseDetails, actualDto);

            // 4. Verificación adicional del estado de la BBDD (Opcional pero recomendado)
            //var deviceInDb = await _context.Device.FindAsync(_device1Id);
            //Assert.Equal(8, deviceInDb.QuantityForPurchase); // 10 iniciales - 2 comprados = 8

        }//De caso éxito

    }//De la clase

}//Del namespace
