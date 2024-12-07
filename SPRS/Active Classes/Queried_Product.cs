using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPRS.Active_Classes
{
    public static class Queried_Products
    {
        private static Dictionary<int, Product> queriedProducts = new Dictionary<int, Product>();

        // Add or update a product in the catalog
        public static void AddProduct(Product product)
        {
            if (!queriedProducts.ContainsKey(product.ProductId))
            {
                queriedProducts.Add(product.ProductId, product);
            }

        }
        // Retrieve a product by its ID, create it if not found
        public static Product GetProductById(int productId)
        {
            // Check if the product exists in the dictionary
            if (!queriedProducts.ContainsKey(productId))
            {
                // If not, create a new product and add it to the dictionary
                Product newProduct = new Product(productId);
                newProduct.LoadProductData(); // Method to populate the product data
                queriedProducts.Add(productId, newProduct);
            }
            else
            {
                //ensure the product data is loaded if it wasnt already
                Product existingProduct = queriedProducts[productId];
                if (string.IsNullOrEmpty(existingProduct.Title))
                {
                    existingProduct.LoadProductData();
                }
            }
       
            // Return the product (either newly created or already existing)
            return queriedProducts[productId];
            
        }
    }
}
