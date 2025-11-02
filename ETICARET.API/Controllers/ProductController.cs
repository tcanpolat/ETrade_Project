using ETICARET.API.Models;
using ETICARET.Business.Abstract;
using ETICARET.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ETICARET.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            try
            {
                var products = _productService.GetAll();

                if(products == null || !products.Any())
                {
                    return Ok(ApiResponse<List<Product>>.SuccessResponse(new List<Product>(), "Ürünler bulanamadı"));
                }

                return Ok(ApiResponse<List<Product>>.SuccessResponse(products, "Ürünler başarıyla getirildi"));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<List<Product>>.ErrorResponse(new List<string> { "Sunucu hatası. Lütfen daha sonra tekrar deneyin." }, "İşlem Başarısız"));
            }
        }
    }
}
