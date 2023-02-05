using Microsoft.AspNetCore.Mvc;
using User.Services;
using User.Services.Models;

namespace User.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    
    public UserController(ILogger<UserController> logger)
    {
        _logger = logger;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id, [FromServices] IUserService userService) => Ok(await userService.GetUser(id));

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateUserModel user, [FromServices] IUserService userService)
    {
        var id = await userService.CreateUser(user);
        return Created($"/user/{id}", null);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(Guid id, UpdateUserModel updateUser, [FromServices] IUserService userService)
    {
        if (id != updateUser.Id)
        {
            return BadRequest();
        }
        
        await userService.UpdateUser(updateUser);
        return NoContent();
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, [FromServices] IUserService userService)
    {
        await userService.DeleteUser(id);
        return Ok();
    }

    [HttpGet("{id}/age")]
    public async Task<IActionResult> GetAge(Guid id, [FromServices] IUserService userService) => Ok(new GetUserAgeModel
    {
        Age = await userService.GetUserAge(id)
    });

}