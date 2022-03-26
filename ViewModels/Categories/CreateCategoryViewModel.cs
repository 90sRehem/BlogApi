using System.ComponentModel.DataAnnotations;

namespace BlogApi.ViewModels.Categories
{
    public class CreateCategoryViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Name { get; set; }

        public string Slug { get; set; }
    }
}