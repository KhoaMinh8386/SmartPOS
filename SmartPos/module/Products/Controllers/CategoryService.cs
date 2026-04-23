using System;
using System.Collections.Generic;
using System.Linq;
using SmartPos.Module.Products.Backend;
using SmartPos.Module.Products.Models;

namespace SmartPos.Module.Products.Controllers
{
    public class CategoryService
    {
        private readonly CategoryRepository _repository;

        public CategoryService()
        {
            _repository = new CategoryRepository();
        }

        public List<CategoryNode> GetCategoryTree()
        {
            var all = _repository.GetAll();
            var nodes = all.Select(c => new CategoryNode { Data = c }).ToList();
            var dict = nodes.ToDictionary(n => n.Data.CategoryID);
            var rootNodes = new List<CategoryNode>();

            foreach (var node in nodes)
            {
                if (node.Data.ParentID.HasValue && dict.ContainsKey(node.Data.ParentID.Value))
                {
                    var parent = dict[node.Data.ParentID.Value];
                    parent.Children.Add(node);
                    node.Data.Level = parent.Data.Level + 1;
                }
                else
                {
                    node.Data.Level = 0;
                    rootNodes.Add(node);
                }
            }
            return rootNodes;
        }

        public List<CategoryDTO> GetFlatListForDropdown()
        {
            var tree = GetCategoryTree();
            var result = new List<CategoryDTO>();
            Flatten(tree, result);
            return result;
        }

        private void Flatten(List<CategoryNode> nodes, List<CategoryDTO> result)
        {
            foreach (var node in nodes)
            {
                var dto = node.Data;
                string prefix = new string('-', dto.Level * 2);
                if (dto.Level > 0) prefix += " ";
                
                result.Add(new CategoryDTO
                {
                    CategoryID = dto.CategoryID,
                    CategoryName = prefix + dto.CategoryName,
                    ParentID = dto.ParentID
                });
                
                if (node.Children.Any())
                {
                    Flatten(node.Children, result);
                }
            }
        }

        public string AddCategory(CategoryDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CategoryName)) return "Tên danh mục không được để trống.";
            
            // Kiểm tra trùng cùng cấp
            var all = _repository.GetAll();
            if (all.Any(c => c.ParentID == dto.ParentID && c.CategoryName.Equals(dto.CategoryName, StringComparison.OrdinalIgnoreCase)))
            {
                return "Tên danh mục đã tồn tại ở cấp độ này.";
            }

            // Kiểm tra tối đa 3 cấp
            if (dto.ParentID.HasValue)
            {
                int level = GetLevel(all, dto.ParentID.Value);
                if (level >= 2) return "Hệ thống chỉ hỗ trợ tối đa 3 cấp danh mục.";
            }

            return _repository.Save(dto) ? null : "Không thể thêm danh mục.";
        }

        public string UpdateCategory(CategoryDTO dto)
        {
            if (dto.CategoryID == dto.ParentID) return "Danh mục không thể là cha của chính nó.";
            
            // Kiểm tra vòng lặp: cha không thể là con của con mình
            if (dto.ParentID.HasValue && IsDescendant(dto.CategoryID, dto.ParentID.Value))
            {
                return "Không thể đặt danh mục làm con của chính cấp dưới của nó.";
            }

            return _repository.Save(dto) ? null : "Không thể cập nhật danh mục.";
        }

        public string DeleteCategory(int id)
        {
            if (_repository.HasProducts(id))
            {
                // Nếu có sản phẩm, chỉ deactivate
                var all = _repository.GetAll();
                var cat = all.FirstOrDefault(c => c.CategoryID == id);
                if (cat != null)
                {
                    cat.IsActive = false;
                    _repository.Save(cat);
                    return "Danh mục có sản phẩm đang dùng. Đã chuyển sang trạng thái Ngừng hoạt động thay vì xóa cứng.";
                }
            }
            
            return _repository.Delete(id) ? null : "Không thể xóa danh mục.";
        }

        private int GetLevel(List<CategoryDTO> all, int id)
        {
            var cat = all.FirstOrDefault(c => c.CategoryID == id);
            if (cat == null) return 0;
            if (!cat.ParentID.HasValue) return 0;
            return 1 + GetLevel(all, cat.ParentID.Value);
        }

        private bool IsDescendant(int parentId, int potentialChildId)
        {
            var tree = GetCategoryTree();
            var parentNode = FindNode(tree, parentId);
            if (parentNode == null) return false;
            
            return FindNode(parentNode.Children, potentialChildId) != null;
        }

        private CategoryNode FindNode(List<CategoryNode> nodes, int id)
        {
            foreach (var node in nodes)
            {
                if (node.Data.CategoryID == id) return node;
                var found = FindNode(node.Children, id);
                if (found != null) return found;
            }
            return null;
        }
    }
}
