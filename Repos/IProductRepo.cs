using DoxmandBackend.DTOs;
using DoxmandBackend.Models;
using System.Collections.Generic;

namespace DoxmandBackend.Repos
{
    public interface IProductRepo
    {
        IEnumerable<Product> GetAllProducts();
        Product GetProductById(string productId);
        Product EditProduct(Product product);
        void DeleteProductById(string productId);
        Product AddProductToFirebase(ProductDTO productDto);
        bool AddProductToUser(User user, Product product);
        IEnumerable<Product> GetBasicProducts();
        int NumberOfProductInPlans(string productId);
        Product CheckUserProductsForName(User user, string name);
    }
}