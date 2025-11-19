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

namespace AppForSEII2526.UT.DeviceControler_test
{
    public class GetDevices_test : AppForSEII25264SqliteUT
    {
        private readonly ILogger<DeviceController> _logger;

        //Vamos a "sembrar" datos en la base de datos en memoria
        //Para eso, usamos el constructor de la clase de test
        public GetDevices_test()
        {
            _logger = new Mock<ILogger<DeviceController>>().Object;

            // 1. Crear Modelo
            var model = new Model { Id = 1, NameModel = "Pro Max" };

            // 2. Crear Dispositivos con variedad de Nombres y Colores para probar filtros
            var devices = new List<Device>
            {
                new Device
                {
                    id = 1,
                    Name = "iPhone 13",
                    Brand = "Apple",
                    Model = model,
                    Color = "Negro",
                    PriceForPurchase = 800,
                    QuantityForPurchase = 10
                },
                new Device
                {
                    id = 2,
                    Name = "iPhone 14",
                    Brand = "Apple",
                    Model = model,
                    Color = "Blanco",
                    PriceForPurchase = 900,
                    QuantityForPurchase = 5
                },
                new Device
                {
                    id = 3,
                    Name = "Samsung Galaxy",
                    Brand = "Samsung",
                    Model = model,
                    Color = "Negro",
                    PriceForPurchase = 750,
                    QuantityForPurchase = 8
                }
            };

            // 3. Añadir al contexto
            _context.Add(model);
            _context.AddRange(devices);
            _context.SaveChanges();
        }//De constructor

    }//De la clase

}//Del namespace
