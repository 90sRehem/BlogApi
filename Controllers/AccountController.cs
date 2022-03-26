using System.Text.RegularExpressions;
using BlogApi.Data;
using BlogApi.Extensions;
using BlogApi.Models;
using BlogApi.Services;
using BlogApi.ViewModels;
using BlogApi.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;

namespace BlogApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AccountController : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginViewModel data,
            [FromServices] BlogDataContext context,
            [FromServices] TokenService tokenService)
        {
            if (!ModelState.IsValid)
                return BadRequest(error: new ResultViewModel<string>(errors: ModelState.GetErrors()));


            var userExists = await context
            .Users
            .AsNoTracking()
            .Include(x => x.Roles)
            .FirstOrDefaultAsync(x => x.Email == data.Email);

            if (userExists is null)
                return Unauthorized(value: new ResultViewModel<string>(error: "Usuário ou senha inválidos."));

            if (!PasswordHasher.Verify(userExists.PasswordHash, data.Password))
                return Unauthorized(value: new ResultViewModel<string>(error: "Usuário ou senha inválidos."));

            try
            {
                var token = tokenService.GenerateToken(userExists);

                return Ok(value: new ResultViewModel<dynamic>(payload: new
                {
                    user = new
                    {
                        name = userExists.Name,
                        email = userExists.Email,
                        token,
                    }
                }));
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex);
                return StatusCode(500, new ResultViewModel<string>(error: "05x86 - Falha interna do servidor."));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(
            [FromBody] RegisterViewModel data,
            [FromServices] EmailService emailService,
            [FromServices] BlogDataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

            var user = new User
            {
                Name = data.Name,
                Email = data.Email,
                Slug = data.Email.Replace(oldValue: "@", newValue: "-").Replace(oldValue: ".", newValue: "-"),
                PasswordHash = PasswordHasher.Hash(data.Password),
            };

            try
            {
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                emailService.Send(
                        toName: user.Name,
                        toEmail: user.Email,
                        subject: "Bem vindo ao blog!",
                        body: $"Sua senha é <strong>{user.PasswordHash}<strong>");

                return Created(
                    $"api/v1/[controller]/{user.Id}",
                     new ResultViewModel<dynamic>(payload: new
                     {
                         user.Id,
                         user.Name,
                         user.Roles,
                         user.Image,
                     }));
            }
            catch (DbUpdateException ex)
            {
                System.Console.WriteLine(ex);
                return StatusCode(400, value: new ResultViewModel<string>(error: "05x99 - Este e-mail já está sendo utilizado."));
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex);
                return StatusCode(500, new ResultViewModel<dynamic>("05x89 - Erro interno do servidor."));
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPost(template: "upload-image")]
        public async Task<IActionResult> UploadImage(
            [FromBody] UploadImageViewModel model,
            [FromServices] BlogDataContext context)
        {
            var fileName = $"{Guid.NewGuid().ToString()}.jpg";
            var data = new Regex(pattern: @"^data:image\/[a-z]+;base64,")
                .Replace(input: model.Base64Image, replacement: "");
            var bytes = Convert.FromBase64String(data);

            try
            {
                await System.IO.File.WriteAllBytesAsync(path: $"wwwroot/images/{fileName}", bytes);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("Error", ex);
                return StatusCode(statusCode: 500, new ResultViewModel<string>(error: "05x14 - Falha interna do servidor."));
            }

            var user = await context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity.Name);

            if (user is null)
            {
                return NotFound(new ResultViewModel<string>(error: "Usuário não encontrado."));
            }

            user.Image = $"https://localhost:0000/images/{fileName}";

            try
            {
                context.Users.Update(user);
                await context.SaveChangesAsync();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("Error", ex);
                return StatusCode(statusCode: 500, new ResultViewModel<string>(error: "05x15 - Falha interna do servidor."));
            }

            return NoContent();
        }
    }
}