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

    }//De la clase

}//Del namespace
