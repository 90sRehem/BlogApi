using BlogApi.Data;
using BlogApi.Models;
using BlogApi.ViewModels;
using BlogApi.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BlogApi.Controllers
{
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        [HttpGet]
        [Route("api/v1/[controller]")]
        public async Task<IActionResult> List(
            [FromServices] BlogDataContext context,
            [FromServices] IMemoryCache cache)
        {
            try
            {
                var categories = cache.GetOrCreate(
                    key: "CategoriesCache",
                    factory: entry =>
                 {
                     entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                     return GetCategories(context);
                 });

                return Ok(new ResultViewModel<List<Category>>(categories));
            }
            catch
            {
                return StatusCode(500, new ResultViewModel<List<Category>>("05X04 - Falha interna no servidor"));
            }
        }

        [HttpGet]
        [Route("api/v1/[controller]/{id:int}")]
        public async Task<IActionResult> FindById(
            [FromRoute] int id,
            [FromServices] BlogDataContext context)
        {
            try
            {
                var category = await context
                    .Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (category == null)
                {
                    return NotFound(new ResultViewModel<Category>("Nenhum resultado encontrado."));
                }
                return Ok(new
                {
                    status = Ok().StatusCode,
                    message = "success",
                    payload = category,
                });
            }
            catch (System.Exception)
            {
                return StatusCode(500, "EX0102 - Falha interna do servidor.");
            }
        }

        [HttpPost]
        [Route("api/v1/[controller]")]
        public async Task<IActionResult> CreateCategory(
                    [FromBody] CreateCategoryViewModel category,
                    [FromServices] BlogDataContext context)
        {
            if (!ModelState.IsValid) return BadRequest(
                new ResultViewModel<Category>("Não foi possível realizar o cadastro."));

            try
            {
                var newCategory = new Category
                {
                    Id = 0,
                    Name = category.Name,
                    Slug = category.Slug ?? category.Name.ToLower(),
                };

                await context.AddAsync(newCategory);
                await context.SaveChangesAsync();

                return Created($"api/v1/[controller]/{newCategory.Id}",
                new ResultViewModel<Category>(newCategory));
            }
            catch (DbUpdateException)
            {
                return StatusCode(500,
                 new
                 {
                     message = "Não foi possível salvar a nova categoria.",
                     status = 500,
                 });
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
                return StatusCode(500, "EX0103 - Falha interna do servidor.");
            }
        }

        [HttpPut]
        [Route("api/v1/[controller]/{id:int}")]
        public async Task<IActionResult> UpdateCategory(
                    [FromRoute] int id,
                    [FromBody] UpdateCategoryViewModel category,
                    [FromServices] BlogDataContext context)
        {
            try
            {
                var updatedCategory = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);

                if (updatedCategory == null)
                {
                    return NotFound(new
                    {
                        status = NotFound().StatusCode,
                        message = "A categoria informada não foi encontrada.",
                    });
                }
                updatedCategory.Name = category.Name;
                updatedCategory.Slug = category.Slug ?? category.Name.ToLower();

                context.Categories.Update(updatedCategory);
                await context.SaveChangesAsync();

                return NoContent();
            }
            catch (System.Exception)
            {
                return StatusCode(500, "EX0104 - Falha interna do servidor.");
            }
        }

        [HttpDelete]
        [Route("api/v1/[controller]/{id:int}")]
        public async Task<IActionResult> RemoveCategory(
                    [FromRoute] int id,
                    [FromServices] BlogDataContext context)
        {
            try
            {
                var categoryExists = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);

                if (categoryExists == null)
                {
                    return NotFound(new
                    {
                        status = NotFound().StatusCode,
                        message = "A categoria informada não foi encontrada.",
                    });
                }

                context.Categories.Remove(categoryExists);
                await context.SaveChangesAsync();

                return NoContent();
            }
            catch (System.Exception)
            {
                return StatusCode(500, "EX0105 - Falha interna do servidor.");
            }
        }

        private List<Category> GetCategories(BlogDataContext context)
        {
            return context.Categories.ToList();
        }
    }
}