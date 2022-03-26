using BlogApi.Data;
using BlogApi.Models;
using BlogApi.ViewModels;
using BlogApi.ViewModels.Posts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PostController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> ListAllPosts(
            [FromServices] BlogDataContext context,
            [FromQuery] int page = 0,
            [FromQuery] int perPage = 25)
        {

            try
            {
                var total = await context.Posts.AsNoTracking().CountAsync();
                var posts = await context
                    .Posts
                    .AsNoTracking()
                    .Include(x => x.Category)
                    .Include(x => x.Author)
                    .Select(x =>
                     new
                     {
                         Id = x.Id,
                         Title = x.Title,
                         Slug = x.Slug,
                         LastUpdateDate = x.LastUpdateDate,
                         Category = x.Category.Name,
                         Author = $"{x.Author.Name} ({x.Author.Email})"
                     })
                    .Skip(page * perPage)
                    .Take(perPage)
                    .OrderByDescending(x => x.LastUpdateDate)
                    .ToListAsync();

                return Ok(new ResultViewModel<dynamic>(payload:
                new
                {
                    total,
                    page,
                    perPage,
                    posts
                }));
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("Error", ex);
                return StatusCode(
                    statusCode: 500,
                    value: new ResultViewModel<string>(
                    error: "05x04 - Falha interna do servidor."));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(
            [FromServices] BlogDataContext context,
            [FromRoute] int id)
        {
            try
            {
                var post = await context
                .Posts
                .AsNoTracking()
                .Include(x => x.Author)
                .ThenInclude(x => x.Roles)
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);

                if (post is null)
                    return NotFound(new ResultViewModel<string>(error: "Conteúdo não encontrado."));

                return Ok(new ResultViewModel<Post>(payload: post));

            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("Error", ex);
                return StatusCode(500, new ResultViewModel<string>(error: "05x44 - Falha interna do servidor"));
            }
        }

        [HttpGet("category/{slug}")]
        public async Task<IActionResult> GetByCategoryId(
            [FromServices] BlogDataContext context,
            [FromRoute] string slug,
            [FromQuery] int page = 0,
            [FromQuery] int perPage = 25)
        {
            try
            {
                var total = await context.Posts.AsNoTracking().CountAsync();
                var posts = await context
                .Posts
                .AsNoTracking()
                .Include(x => x.Author)
                .Include(x => x.Category)
                .Where(x => x.Category.Slug == slug)
                .Select(x => new ListPostsViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Slug = x.Slug,
                    LastUpdated = x.LastUpdateDate,
                    Category = x.Category.Name,
                    Author = $"{x.Author.Name} ({x.Author.Email})"
                })
                .Skip(page * perPage)
                .Take(perPage)
                .OrderByDescending(x => x.LastUpdated)
                .ToListAsync();

                if (posts is null)
                    return NotFound(new ResultViewModel<string>(error: "Conteúdo não encontrado."));

                return Ok(new ResultViewModel<dynamic>(payload:
                new
                {
                    total,
                    page,
                    perPage,
                    posts
                }));

            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("Error", ex);
                return StatusCode(500, new ResultViewModel<string>(error: "05x47 - Falha interna do servidor"));
            }
        }

    }
}