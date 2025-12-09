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

        //Antes de pasar a las pruebas, como es costumbre, vamos a definir casos de prueba (MemberData)

        // --- DEFINICIÓN DE CASOS DE PRUEBA (MemberData) ---

        public static IEnumerable<object[]> TestCasesFor_GetDevicesParaPurchase()
        {
            // Recreamos los DTOs esperados basados en los datos del seed
            var dto1 = new DeviceParaCompraDTOs(1, "iPhone 13", "Apple", "Pro Max", "Negro", 800);
            var dto2 = new DeviceParaCompraDTOs(2, "iPhone 14", "Apple", "Pro Max", "Blanco", 900);
            var dto3 = new DeviceParaCompraDTOs(3, "Samsung Galaxy", "Samsung", "Pro Max", "Negro", 750);

            return new List<object[]>
            {
                // Caso 1: Sin filtros (null, null) -> Debe devolver TODOS (1, 2 y 3)
                new object[]
                {
                    null, // nombreFILTRO
                    null, // colorFILTRO
                    new List<DeviceParaCompraDTOs> { dto1, dto2, dto3 } // Esperado
                },

                // Caso 2: Filtro por Nombre "iPhone" -> Debe devolver 1 y 2
                new object[]
                {
                    "iPhone",
                    null,
                    new List<DeviceParaCompraDTOs> { dto1, dto2 }
                },

                // Caso 3: Filtro por Color "Negro" -> Debe devolver 1 y 3
                new object[]
                {
                    null,
                    "Negro",
                    new List<DeviceParaCompraDTOs> { dto1, dto3 }
                },

                // Caso 4: Filtro combinado (Nombre "Samsung" Y Color "Negro") -> Debe devolver 3
                new object[]
                {
                    "Samsung",
                    "Negro",
                    new List<DeviceParaCompraDTOs> { dto3 }
                },

                // Caso 5: Filtro sin coincidencias -> Lista vacía
                new object[]
                {
                    "Xiaomi",
                    "Rojo",
                    new List<DeviceParaCompraDTOs>()
                }
            };
        }// De la lista de casos de prueba

        // --- PRUEBAS ---

        //Prueba principal usando los casos de prueba definidos
        [Theory]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        [MemberData(nameof(TestCasesFor_GetDevicesParaPurchase))]
        public async Task GetDevicesParaPurchase_OK_Test(string? nombreFiltro, string? colorFiltro, List<DeviceParaCompraDTOs> expectedDevices)
        {
            // Arrange
            var controller = new DeviceController(_context, _logger);

            // Act
            var result = await controller.GetDevicesParaPurchase(nombreFiltro, colorFiltro);

            // Assert
            // 1. Verificar que es OkObjectResult (200 OK)
            var okResult = Assert.IsType<OkObjectResult>(result);

            // 2. Verificar que devuelve una lista de DTOs
            var actualDevices = Assert.IsType<List<DeviceParaCompraDTOs>>(okResult.Value);

            // 3. Verificar que la lista obtenida es igual a la esperada
            // (Esto funciona gracias al Equals que añadimos al DTO)
            //Assert.Equal(expectedDevices.Count, actualDevices.Count); // Opcional: Verificar conteo primero
            Assert.Equal(expectedDevices, actualDevices);

        }//De la prueba okey

        //Prueba para verificar que un filtro de nombre muy corto provoca BadRequest
        [Fact]
        [Trait("LevelTesting", "Unit Testing")]
        [Trait("Database", "WithoutFixture")]
        public async Task GetDevicesParaPurchase_BadRequest_Test()
        {
            // Arrange
            var controller = new DeviceController(_context, _logger);

            // Input inválido según nuestra nueva regla: nombre con menos de 3 caracteres ("ab")
            string invalidNameFilter = "ab";
            string validColorFilter = null;

            // Act
            var result = await controller.GetDevicesParaPurchase(invalidNameFilter, validColorFilter);

            // Assert
            // 1. Verificar que devuelve BadRequest
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            // 2. Verificar que devuelve detalles del problema
            var problemDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            // 3. (Opcional) Verificar el mensaje de error específico
            Assert.True(problemDetails.Errors.ContainsKey("nombreFILTRO"));
            Assert.Contains("least 3 characters", problemDetails.Errors["nombreFILTRO"][0]);
        }//De la prueba bad request

    }//De la clase

}//Del namespace
