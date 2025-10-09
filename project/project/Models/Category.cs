using System.ComponentModel.DataAnnotations;
namespace project.Models
{
    public class Category
    {
        public int? Id { get; set; }

        public string? Type { get; set; }

        // Navigation Properties
        public virtual ICollection<Blog>? Blogs { get; set; }
    }
}
