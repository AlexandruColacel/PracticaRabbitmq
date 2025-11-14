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
    }
}
