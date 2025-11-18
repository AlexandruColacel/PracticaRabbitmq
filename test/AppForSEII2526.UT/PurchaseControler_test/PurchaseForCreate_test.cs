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

            // Añadir al contexto
            _context.Add(user);
            _context.Add(model);
            _context.Add(device1);
            _context.Add(deviceNoStock);
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

            // Retornamos: [DTO de entrada, Mensaje de error esperado (o parte de él)]
            return new List<object[]>
            {
                new object[] { purchaseNoItems, "You must include at least one device to purchase." },
                new object[] { purchaseUserNotRegistered, "UserName is not registered." },
                new object[] { purchaseDeviceNotExist, "Device with id 999 does not exist." },
                new object[] { purchaseQuantityZero, "Quantity for device TechBrand SuperModelo must be greater than 0." }, // Asumiendo que el mensaje incluye marca/modelo
                new object[] { purchaseOutOfStock, "Not enough stock for device 'OldBrand SuperModelo'" }
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
        }



    }//De la clase

}//Del namespace
