using System.Collections.Generic;
using DoxmandAPI.DTOs;
using DoxmandAPI.Models;

namespace DoxmandAPI.Repos
{
    public interface IProductRepo
    {
        IEnumerable<Product> GetAllProducts();
        Product GetProductById(string productId);
        Product EditProduct(Product product);
        void DeleteProductById(string productId);
        Product AddProductToFirebase(ProductDTO productDto);
        IEnumerable<Product> GetBasicProducts();
        int NumberOfProductInPlans(string productId);
    }
}