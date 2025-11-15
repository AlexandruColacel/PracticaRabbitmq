using AppForSEII2526.API.Controllers;
using AppForSEII2526.API.DTOs.PurchaseDTO;
using AppForSEII2526.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Necesario para Include y ThenInclude
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AppForSEII2526.UT.PurchaseControler_test
{
    public class PurchaseDetails_test: AppForSEII25264SqliteUT
    {
        private readonly ILogger<PurchaseControler> _logger;
        private PurchaseDetailsDTO _expectedDto; // DTO esperado para la Compra 1
        private int _purchaseId_OK = 1;
        private int _purchaseId_NotFound = 999;

        //Constructor: usamos esto para sembrar datos en la base de datos en memoria
        public PurchaseDetails_test()
        {
            //Mock del logger
            _logger = new Mock<ILogger<PurchaseControler>>().Object;

            //Creación de entidades necesarias
            var testUser = new ApplicationUser
            {
                UserName = "testuser@example.com", // Requerido por IdentityUser
                Email = "testuser@example.com",    // Requerido
                Name = "John",
                Surname = "Doe"
            };

            var testModel = new Model
            {
                Id = 30,
                NameModel = "SuperModelo"
            };

            var testDevice = new Device
            {
                id = 20,
                Name = "SupaMegaAmazingPhone",
                Brand = "TechBrand",
                Model = testModel,
                Color = "Black",
                PriceForPurchase = 499.99,
                QuantityForPurchase = 5, //REVISAR
                Description = "A high-end tech device."
            };

            //Ahora necesitamos crear la entidad principal, que en nuestro caso es Purchase
            var testPurchase = new Purchase
            {
                Id = _purchaseId_OK, 
                CustomerUserName = testUser.Name,
                CustomerUserSurname = testUser.Surname,
                TotalPrice = 999.98,
                DeliveryAddress = "123 Tech Street",
                PaymentMethod = PaymentMethod.TarjetaCredito,
                PurchaseDate = DateTime.Now,
                Quantity = 2,
                ApplicationUser = testUser,
                PurchaseItems = new List<PurchaseItem>() //Inicializamos la lista vacía
            };

            //Teniendo ya a mano el usuario, modelo, dispositivo, la compra, y el itemCompra, pasamos a meterlos en el contexto.
            _context.Add(testUser);
            _context.Add(testModel);
            _context.Add(testDevice);
            _context.Add(testPurchase);
            //_context.Add(testPurchaseItem); //EF Core es inteligente y "rastrea" no solo la compra, sino también los objetos de su lista.
            //Nota personal, no es necesario hacer _context.Add(_logger) ya que no es entidad para la base de datos, solo es un servicio para escribir logs, y no un dato que deba persistir.

            //Creamos el purchase item por separado para tenerlo a mano en el DTO esperado y vincularlo correctamente con la compra y el dispositivo
            var testPurchaseItem = new PurchaseItem
            {
                //PurchaseId = 40, //No necesito "PurchaseID" si ya das de manera explicita el purchase
                Device = testDevice,
                Purchase = testPurchase,
                Price = 499.99, // Precio unitario en el momento de la compra
                Quantity = 2,
                Description = "A high-end tech device."
            };

            // Añadimos solo el item
            _context.Add(testPurchaseItem);

            // AÑADIDO: También se debe añadir el item a la lista de la compra si el DTO lo va a necesitar
            testPurchase.PurchaseItems.Add(testPurchaseItem);

            // Guardamos los cambios para que todo esté en la BBDD en memoria
            _context.SaveChanges();

            //Una vez con la base de datos ya construida, vamos a dar forma al DTO que nuestro controlador debe de hacer
            //Crucial para aserción (Assert)
            var itemEsperadoDTO = new PurchaseItemDTO(testDevice.id, testDevice.Brand, testModel.NameModel, testDevice.Color, (decimal)testDevice.PriceForPurchase, testPurchaseItem.Quantity, testPurchaseItem.Description);

            _expectedDto = new PurchaseDetailsDTO(
                testPurchase.Id,
                testPurchase.CustomerUserName,
                testPurchase.CustomerUserSurname,
                testPurchase.DeliveryAddress,
                testPurchase.PurchaseDate,
                (decimal) testPurchase.TotalPrice,
                testPurchase.Quantity,
                new List<PurchaseItemDTO> { itemEsperadoDTO }
            );

        }//Del constructor

        //Test para el caso de éxito (OK 200)
        //Vamos a comprobar que al pedir un ID existente, nos devuelve el DTO esperado

        //[Theory] //Usamos Theory porque vamos a pasar parámetros, en mi caso no es necesario hacer esto, pero lo dejo comentado para futuras referencias
        [Fact] //Usamos Fact porque no vamos a pasar parámetros
        [Trait("Database", "WithoutFixture")] //Etiqueta personalizada para identificar el tipo de base de datos usada. WithoutFixture indica que no usamos un fixture externo
        [Trait("LevelTesting", "Unit Testing")] //Etiqueta personalizada para identificar el nivel de testing

        public async Task GetPurchase_OK_test()
        {
            //Arrange (Preparación) 
            var controller = new PurchaseControler(_context, _logger);

            //Act (Ejecución)
            var actionResult = await controller.GetPurchase(_purchaseId_OK);

            //Assert (Verificación)

            //Verificamos que el resultado es OkObjectResult
            var okResult = Assert.IsType<OkObjectResult>(actionResult);

            //Verificamos que el valor dentro del OkObjectResult es del tipo esperado (PurchaseDetailsDTO)
            var actualDto = Assert.IsType<PurchaseDetailsDTO>(okResult.Value);

            //Comparamos los DTOs usando Equals()
            Assert.Equal(_expectedDto, actualDto);
        }



    }//De la clase PurchaseDetails_test

}//Del namespace AppForSEII2526.UT.PurchaseControler_test
