using System;
using System.Collections.Generic;
using SmartPos.Module.Products.Backend;
using SmartPos.Module.Products.Models;

namespace SmartPos.Module.Products.Controllers
{
    public class ProductController
    {
        private readonly ProductBackend _backend;

        public ProductController()
        {
            _backend = new ProductBackend();
        }

        private bool IsAdmin()
        {
            return UserSession.CurrentUser != null && UserSession.CurrentUser.RoleID == 1;
        }

        public List<CategoryListItem> GetCategories()
        {
            return _backend.GetCategories();
        }

        public List<ProductListItem> GetProducts(string search = null, int categoryId = 0)
        {
            return _backend.GetProducts(search, categoryId);
        }

        public ProductDetail GetProductDetail(int productId)
        {
            return _backend.GetProductDetail(productId);
        }

        public void SaveProduct(ProductDetail product)
        {
            if (!IsAdmin())
            {
                throw new UnauthorizedAccessException("Ban khong co quyen thuc hien thao tac nay.");
            }

            if (string.IsNullOrWhiteSpace(product.ProductName))
                throw new ArgumentException("Ten san pham khong duoc de trong.");
            
            if (string.IsNullOrWhiteSpace(product.ProductCode))
                throw new ArgumentException("Ma SKU khong duoc de trong.");

            _backend.SaveProduct(product);
        }

        public void DeleteProduct(int productId)
        {
            if (!IsAdmin())
            {
                throw new UnauthorizedAccessException("Ban khong co quyen thuc hien thao tac nay.");
            }

            _backend.DeleteProduct(productId);
        }

        public void SaveCategory(CategoryListItem category)
        {
            if (!IsAdmin())
            {
                throw new UnauthorizedAccessException("Ban khong co quyen thuc hien thao tac nay.");
            }

            if (string.IsNullOrWhiteSpace(category.CategoryName))
                throw new ArgumentException("Ten danh muc khong duoc de trong.");

            _backend.SaveCategory(category);
        }

        public List<SupplierLookupItem> GetSuppliers()
        {
            return _backend.GetSuppliers();
        }

        public List<UnitLookupItem> GetUnits()
        {
            return _backend.GetUnits();
        }
    }
}
