using Microsoft.AspNetCore.Mvc;
using User.Services;
using User.Services.Models;

namespace User.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id) => Ok(await _userService.GetUser(id));

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateUserModel user)
    {
        var id = await _userService.CreateUser(user);
        return Created($"/user/{id}", null);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(Guid id, UpdateUserModel updateUser)
    {
        if (id != updateUser.Id)
        {
            return BadRequest();
        }
        
        await _userService.UpdateUser(updateUser);
        return NoContent();
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _userService.DeleteUser(id);
        return Ok();
    }

    [HttpGet("{id}/age")]
    public async Task<IActionResult> GetAge(Guid id) => Ok(new GetUserAgeModel
    {
        Age = await _userService.GetUserAge(id)
    });
}