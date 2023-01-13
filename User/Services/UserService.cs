using User.Clients;
using User.Entities;
using User.Exceptions;
using User.Repositories;
using User.Services.Models;

namespace User.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IIszrClient _iszrClient;

    public UserService(IUserRepository userRepository, IIszrClient iszrClient)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _iszrClient = iszrClient ?? throw new ArgumentNullException(nameof(iszrClient));
    }

    public async Task<Guid> CreateUser(CreateUserModel createUserModel)
    {
        var ruian = await _iszrClient.GetRuianByBirthNumber(createUserModel.BirthNumber);

        if (ruian is null)
        {
            throw new RuianNotFoundException();
        }
        
        var userEntity = new UserEntity
        {
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
            return null;
        }
        
        return new GetUserModel
        {
            Id = userEntity.Id,
            Ruian = userEntity.Ruian,
            FirstName = userEntity.FirstName,
            LastName = userEntity.LastName,
            DateOfBirth = userEntity.DateOfBirth
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
}