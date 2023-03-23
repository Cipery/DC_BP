using System.Net;
using User.Clients;
using User.Entities;
using User.Exceptions;
using User.Repositories;
using User.Services.Models;

namespace User.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IIszrClient _iszrClient;
    private readonly IClockService _clockService;

    public UserService(IUserRepository userRepository, IIszrClient iszrClient, IClockService clockService)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _iszrClient = iszrClient ?? throw new ArgumentNullException(nameof(iszrClient));
        _clockService = clockService ?? throw new ArgumentNullException(nameof(clockService));
    }

    public async Task<Guid> CreateUser(CreateUserModel createUserModel)
    {
        var ruian = await _iszrClient.GetRuianByBirthNumber(createUserModel.BirthNumber);

        createUserModel.DateOfBirth = DateTime.SpecifyKind(createUserModel.DateOfBirth, DateTimeKind.Utc);
        
        var userEntity = new UserEntity
        {
            Id = Guid.NewGuid(),
            FirstName = createUserModel.FirstName,
            LastName = createUserModel.LastName,
            DateOfBirth = createUserModel.DateOfBirth,
            BirthNumber = createUserModel.BirthNumber,
            Ruian = ruian
        };

        await _userRepository.Add(userEntity);
        return userEntity.Id;
    }

    public async Task<GetUserModel?> GetUser(Guid userId)
    {
        var userEntity = await _userRepository.Get(userId);

        if (userEntity is null)
        {
            throw new EntityNotFoundException();
        }
        
        return new GetUserModel
        {
            Id = userEntity.Id,
            Ruian = userEntity.Ruian,
            FirstName = userEntity.FirstName,
            LastName = userEntity.LastName,
            DateOfBirth = userEntity.DateOfBirth.DateTime
        };
    }

    public async Task DeleteUser(Guid id)
    {
        var userEntity = await _userRepository.Get(id);

        if (userEntity is null)
        {
            throw new EntityNotFoundException();
        }

        await _userRepository.Remove(userEntity);
    }
    
    public async Task UpdateUser(UpdateUserModel updateUserModel)
    {
        if (updateUserModel.FirstName is null && updateUserModel.LastName is null)
        {
            throw new ApiException(HttpStatusCode.BadRequest, "At least one property must be set to update the user.");
        }
        
        var userEntity = await _userRepository.Get(updateUserModel.Id);

        if (userEntity is null)
        {
            throw new EntityNotFoundException();
        }

        if (updateUserModel.FirstName is not null)
        {
            userEntity.FirstName = updateUserModel.FirstName;
        }

        if (updateUserModel.LastName is not null)
        {
            userEntity.LastName = updateUserModel.LastName;
        }
        
        await _userRepository.Update(userEntity);
    }

    public async Task<int> GetUserAge(Guid id)
    {
        var userEntity = await _userRepository.Get(id);

        if (userEntity is null)
        {
            throw new EntityNotFoundException();
        }
        
        var utcNow = _clockService.NowUtc();
        var delta = (utcNow - userEntity!.DateOfBirth);
        return (int)(delta.TotalDays / 365.25d);
    }
}