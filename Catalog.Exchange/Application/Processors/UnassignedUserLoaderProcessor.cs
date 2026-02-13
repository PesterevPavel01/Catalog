using Calabonga.OperationResults;
using Catalog.Contracts.Dto.Authorization;
using Catalog.ExchangeService.Application.Handlers.Users;

namespace Catalog.ExchangeService.Application.Processors
{
    public sealed class UnassignedUserLoaderProcessor
    {
        private readonly CachedUsersQueryHandler _queryHandler;

        public UnassignedUserLoaderProcessor(CachedUsersQueryHandler queryHandler)
        {
            _queryHandler = queryHandler;
        }

        public async Task<Operation<IEnumerable<UserDto>, string>> ProcessAsync(CancellationToken cancellationToken)
        {
            var usersResult = await _queryHandler.HandleAsync(cancellationToken);

            if (!usersResult.Ok)
                return Operation.Error(usersResult.Error);

            var users = usersResult.Result
                .Where(x => !x.Roles.Any() && x.ExternalId == "none");

            if (!users.Any())
                return Operation.Error("Users not found!");

            return users.ToArray();
        }
    }
}