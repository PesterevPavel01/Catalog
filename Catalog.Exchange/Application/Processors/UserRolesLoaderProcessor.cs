using Calabonga.OperationResults;
using Catalog.Contracts.Dto.Authorization;
using Catalog.ExchangeService.Application.Handlers.Users;

namespace Catalog.ExchangeService.Application.Processors
{
    public sealed class UserRolesLoaderProcessor
    {
        private readonly UsersQueryHandler _queryHandler;

        public UserRolesLoaderProcessor(UsersQueryHandler queryHandler)
        {
            _queryHandler = queryHandler;
        }

        public async Task<Operation<UserDto, string>> ProcessAsync(string userName, CancellationToken cancellationToken)
        {
            var usersResult = await _queryHandler.HandleAsync(cancellationToken);

            if (!usersResult.Ok)
                return Operation.Error(usersResult.Error);

            if (!usersResult.Result.Any())
                return Operation.Error("Users not found!");

            var user = usersResult.Result
                .FirstOrDefault(
                    predicate: x => x.UserName == userName
                );

            if (user is null)
                return Operation.Error("User not found!");

            return user;
        }
    }
}