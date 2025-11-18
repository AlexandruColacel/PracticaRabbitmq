using AppForSEII2526.API.Controllers;
using AppForSEII2526.API.DTOs.PurchaseDTO;
using AppForSEII2526.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AppForSEII2526.UT.PurchaseControler_test
{
    internal class PurchaseForCreate_test
    {
        //Atributos para las pruebas / datos constantes
        private const string _userName = "testuser@example.com";
        private const string _userSurname = "Doe";
        private const string _deliveryAddress = "123 Tech Street, Albacete";
        private const int _device1Id = 20;
        private const int _device2Id = 21; // Dispositivo sin stock o para otra prueba

        public PurchaseForCreate_test()
        {
            // Rellenar en un futuro, especialmente si se necesita un contexto de base de datos simulado
        }
    }
}
