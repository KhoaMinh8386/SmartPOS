using System;
using System.Collections.Generic;

namespace SmartPos.Module.Products.Models
{
    public class CategoryDTO
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int? ParentID { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        
        // Dùng cho TreeView
        public string ParentName { get; set; }
        public int Level { get; set; }
    }

    public class CategoryNode
    {
        public CategoryDTO Data { get; set; }
        public List<CategoryNode> Children { get; set; } = new List<CategoryNode>();
    }
}
