using System.ComponentModel.DataAnnotations;

namespace BlogApi.ViewModels.Categories
{
    public class UpdateCategoryViewModel
    {
        public string Name { get; set; }

        public string Slug { get; set; }


    }
}