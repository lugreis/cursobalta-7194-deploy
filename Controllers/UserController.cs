using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Shop.Services;

namespace Shop.Controllers
{
    [Route("users")]
    public class UserController : Controller
    {
        [HttpGet]
        [Route("")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<List<User>>> GetAction([FromServices] DataContext context)
        {
            var users = await context.Users.AsNoTracking().ToListAsync();
            return users;
        }

        [HttpPost]
        [Route("")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> Post([FromServices] DataContext context, [FromBody] User model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                model.Role = "employee";

                context.Users.Add(model);
                await context.SaveChangesAsync();

                model.Password = "";
                return model;
            }
            catch (Exception)
            {
                return BadRequest(new {message = "Não foi possível criar o usuário"});
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<User>> Put([FromServices] DataContext context, int id, [FromBody] User model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            if (id != model.Id)
                return NotFound(new { message = "Usuário não encontado"});
            
            try{
                context.Entry(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return model;
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível criar o usuário"});
            }
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ActionResult<dynamic>> Authenticate([FromServices] DataContext context, [FromBody]User model)
        {
            var user = await context.Users.AsNoTracking().Where(x => x.Username == model.Username && x.Password == model.Password).FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "Usuário ou senha inválidos"});
            
            var token = TokenService.GenerateToken(user);
            user.Password = "";
            return new
            {
                user = user,
                token = token
            };
        }

        [HttpDelete]
        [Route("{id:int}")]
        public async Task<ActionResult<List<Category>>> Delete(int id, [FromServices]DataContext context)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            return NotFound(new {message = "Usuário não encontrado"});
        
            try
            {
                context.Users.Remove(user);
                await context.SaveChangesAsync();
                return Ok(new { message = "Usuário removido com sucesso"});
            }
            catch (Exception)
            {
                return BadRequest(new { message = "Não foi possível remover o usuário"});
            }
        }
    }
}