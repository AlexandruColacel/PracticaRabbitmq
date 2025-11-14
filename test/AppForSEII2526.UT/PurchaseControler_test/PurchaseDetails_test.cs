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
            _logger = new Mock<ILogger<PurchaseControler>>().Object; //Un mock es un objeto simulado que imita el comportamiento de objetos reales en pruebas unitarias.

            //Creación de entidades necesarias
            var testUser = new ApplicationUser
            {
                UserName = "John",
                Surname = "Doe"
            };

            var testModel = new Model
            {
                Id = 3,
                NameModel = "SuperModelo"
            };

            var testDevice = new Device
            {
                id = 2,
                Name = "SupaMegaAmazingPhone",
                Brand = "TechBrand",
                Model = testModel,
                Color = "Black",
                PriceForPurchase = 499.99,
                QuantityForPurchase = 5,
                Description = "A high-end tech device."
            };

            //Ahora necesitamos crear la entidad principal, que en nuestro caso es Purchase
            var testPurchase = new Purchase
            {
                Id = 1, 
                CustomerUserName = testUser.UserName,
                TotalPrice = 999.98,
                DeliveryAddress = "123 Tech Street",
                PaymentMethod = PaymentMethod.TarjetaCredito,
                PurchaseDate = DateTime.Now,
                Quantity = 2,
                ApplicationUser = testUser,
                PurchaseItems = new List<PurchaseItem>
                {
                    new PurchaseItem
                    {
                        Device = testDevice,
                        Quantity = 2
                    }
                }
            };

            //Ahora, vamos a crear la entidad que nos dará los detalles ( PurchaseItem )

            var testPurchaseItem = new PurchaseItem
            {
                PurchaseId = 4,
                Device = testDevice,
                Purchase = testPurchase,
                Price = 499.99, // Precio unitario en el momento de la compra
                Quantity = 2,
                Description = "El cliente dejo un comentario grosero y fue ejecutado"
            };

            //Teniendo ya a mano el usuario, modelo, dispositivo, la compra, y el itemCompra, pasamos a meterlos en el contexto.

            _context.Add(testUser);
            _context.Add(testModel);
            _context.Add(testDevice);
            _context.Add(testPurchase);
            _context.Add(testPurchaseItem);
            //Nota personal, no es necesario hacer _context.Add(_logger) ya que no es entidad para la base de datos, solo es un servicio para escribir logs, y no un dato que deba persistir.

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
    }
}
