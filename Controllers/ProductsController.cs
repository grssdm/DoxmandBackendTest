using System;
using System.Collections.Generic;
using DoxmandAPI.DTOs;
using DoxmandAPI.Models;
using DoxmandAPI.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoxmandAPI.Controllers
{
    [Authorize]
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly DoxmandRepo _repo = new DoxmandRepo();
        
        [HttpGet]
        public ActionResult<IEnumerable<Product>> GetAllProducts()
        {
            var products = _repo.GetAllProducts();

            if (products == null)
            {
                return NotFound("There are no Products");
            }

            return Ok(products);
        }

        [HttpGet("{id}")]
        public ActionResult<Product> GetProductById(string id)
        {
            var product = _repo.GetProductById(id);

            if (product == null)
            {
                return NotFound($"There is no Product with ID {id}");
            }

            return Ok(product);
        }

        [HttpPut("{id}")]
        public ActionResult EditProduct(Product product)
        {
            var _product = _repo.GetProductById(product.Product_ID);

            if (_product == null)
            {
                return NotFound($"There is no Product with ID {product.Product_ID}");
            }

            try
            {
                _repo.EditProduct(product);

                return Ok($"Product with ID {product.Product_ID} has been successfully updated");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteProductById(string productId)
        {
            var product = _repo.GetProductById(productId);

            if (product == null)
            {
                return NotFound($"There is no Product with ID {product.Product_ID}");
            }

            try
            {
                _repo.DeleteProductById(productId);

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("new")]
        public ActionResult<Product> AddNewProduct(ProductDTO productDto)
        {
            if (string.IsNullOrEmpty(productDto.Name))
            {
                return BadRequest("Some parameters are missing");
            }

            try
            {
                var product = _repo.AddProductToFirebase(productDto);
                return Ok($"Product with ID {product.Product_ID} has been successfully added");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("basic")]
        public ActionResult<IEnumerable<Product>> GetBasicProducts()
        {
            return Ok(_repo.GetBasicProducts());
        }

        [HttpGet("{id}/number")]
        public ActionResult<int> NumberOfProductionInPlans(string id)
        {
            var product = _repo.GetProductById(id);

            if (product == null)
            {
                return NotFound("");
            }

            return Ok(_repo.NumberOfProductInPlans(id));
        }
    }
}