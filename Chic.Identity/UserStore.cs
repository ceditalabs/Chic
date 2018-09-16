using Chic.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Chic.Identity
{
    public class UserStore :
        UserStore<IdentityUser<int>>
    {
        public UserStore(IServiceProvider provider, IdentityErrorDescriber describer) : base(provider, describer) { }
    }

    public class UserStore<TUser> :
        UserStore<TUser, IdentityRole<int>, int>
        where TUser : IdentityUser<int>, new()
    {
        public UserStore(IServiceProvider provider, IdentityErrorDescriber describer) : base(provider, describer) { }
    }

    public class UserStore<TUser, TRole, TKey> :
        UserStore<TUser, TRole, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>, IdentityRoleClaim<TKey>>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        public UserStore(IServiceProvider provider, IdentityErrorDescriber describer) : base(provider, describer) { }
    }

    public class UserStore<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim> :
        UserStoreBase<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>,
        IProtectedUserStore<TUser>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TUserClaim : IdentityUserClaim<TKey>, new()
        where TUserRole : IdentityUserRole<TKey>, new()
        where TUserLogin : IdentityUserLogin<TKey>, new()
        where TUserToken : IdentityUserToken<TKey>, new()
        where TRoleClaim : IdentityRoleClaim<TKey>, new()
    {
        public UserStore(IServiceProvider provider, IdentityErrorDescriber describer) : base(describer ?? new IdentityErrorDescriber())
        {
            this.provider = provider;
        }

        private readonly IServiceProvider provider;

        public override IQueryable<TUser> Users
        {
            get
            {
                return provider.GetRequiredService<IRepository<TUser, TKey>>().GetAllAsync().Result.AsQueryable();
            }
        }

        public override async Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            var userClaims = claims.Select(claim => CreateUserClaim(user, claim));

            using (var repository = provider.GetRequiredService<IRepository<TUserClaim, TKey>>())
            {
                await repository.InsertManyAsync(userClaims);
            }
        }

        public override async Task AddLoginAsync(TUser user, UserLoginInfo login, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (login == null)
            {
                throw new ArgumentNullException(nameof(login));
            }

            var userLogin = CreateUserLogin(user, login);

            using (var repository = provider.GetRequiredService<IRepository<TUserLogin, TKey>>())
            {
                // HACK: Workaround insertion limitation without PK
                await repository.InsertManyAsync(new[] { userLogin });
            }
        }

        public override async Task AddToRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (string.IsNullOrWhiteSpace(normalizedRoleName))
            {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }

            var role = await FindRoleAsync(normalizedRoleName, cancellationToken);

            if (role == null)
            {
                throw new InvalidOperationException($"Role {normalizedRoleName} not found.");
            }

            var userRole = CreateUserRole(user, role);

            using (var repository = provider.GetRequiredService<IRepository<TUserRole, TKey>>())
            {
                await repository.InsertManyAsync(new[] { userRole });
            }
        }

        public override async Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            using (var repository = provider.GetRequiredService<IRepository<TUser, TKey>>())
            {
                var insertedId = await repository.InsertAsync(user);
                user.Id = insertedId;
            }

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            using (var repository = provider.GetRequiredService<IRepository<TUser, TKey>>())
            {
                await repository.UpdateAsync(user);
            }

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            using (var repository = provider.GetRequiredService<IRepository<TUser, TKey>>())
            {
                await repository.DeleteAsync(user);
            }

            return IdentityResult.Success;
        }

        public override async Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            using (var repository = provider.GetRequiredService<IRepository<TUser, TKey>>())
            {
                var results = await repository.GetByWhereAsync(
                    $"[{nameof(IdentityUser<TKey>.NormalizedEmail)}] = @{nameof(normalizedEmail)}",
                    new { normalizedEmail });
                return results.SingleOrDefault();
            }
        }

        public override async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            var id = ConvertIdFromString(userId);
            using (var repository = provider.GetRequiredService<IRepository<TUser, TKey>>())
            {
                return await repository.GetByIdAsync(id);
            }
        }

        public override async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            using(var repository = provider.GetRequiredService<IRepository<TUser, TKey>>())
            {
                var results = await repository.GetByWhereAsync(
                    $"[{nameof(IdentityUser<TKey>.NormalizedUserName)}] = @{nameof(normalizedUserName)}",
                    new { normalizedUserName });
                return results.SingleOrDefault();
            }
        }

        public override async Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            using (var userClaimRepository = provider.GetRequiredService<IRepository<TUserClaim, TKey>>())
            {
                var claims = (await userClaimRepository.GetByWhereAsync(
                    $"[{nameof(IdentityUserRole<TKey>.UserId)}] = @{nameof(user.Id)}",
                    new { user.Id }))
                    .Select(m => m.ToClaim());

                return claims.ToList();
            }
        }

        public override async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            using (var userLoginRepository = provider.GetRequiredService<IRepository<TUserLogin, TKey>>())
            {
                var userLogins = (await userLoginRepository.GetByWhereAsync(
                    $"[{nameof(IdentityUserRole<TKey>.UserId)}] = @{nameof(user.Id)}",
                    new { user.Id }))
                    .Select(m => new UserLoginInfo(m.LoginProvider, m.ProviderKey, m.ProviderDisplayName));

                return userLogins.ToList();
            }
        }

        public override async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            
            using (var roleRepository = provider.GetRequiredService<IRepository<TRole, TKey>>())
            using (var userRoleRepository = provider.GetRequiredService<IRepository<TUserRole, TKey>>())
            {
                var userRoleIds = (await userRoleRepository.GetByWhereAsync(
                    $"[{nameof(IdentityUserRole<TKey>.UserId)}] = @{nameof(user.Id)}",
                    new { user.Id }))
                    .Select(m => m.RoleId);
                var roles = await roleRepository.GetByWhereAsync(
                    $"[{nameof(IdentityRole<TKey>.Id)}] IN @{nameof(userRoleIds)}",
                    userRoleIds);

                return roles.Select(m => m.Name).ToList();
            }
        }

        public override async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            using (var userRepository = provider.GetRequiredService<IRepository<TUser, TKey>>())
            using (var userClaimRepository = provider.GetRequiredService<IRepository<TUserClaim, TKey>>())
            {
                var userIds = (await userClaimRepository.GetByWhereAsync(
                    $"[{nameof(IdentityUserClaim<TKey>.ClaimValue)}] = @{nameof(claim.Value)} AND [{nameof(IdentityUserClaim<TKey>.ClaimType)}] = @{nameof(claim.Type)}",
                    new { claim.Value, claim.Type }))
                    .Select(m => m.UserId);
                var users = await userRepository.GetByWhereAsync(
                    $"[{nameof(IdentityUser<TKey>.Id)}] IN @{nameof(userIds)}",
                    userIds);

                return users.ToList();
            }
        }

        public override async Task<IList<TUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (string.IsNullOrEmpty(normalizedRoleName))
            {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }

            var role = await FindRoleAsync(normalizedRoleName, cancellationToken);

            using (var userRepository = provider.GetRequiredService<IRepository<TUser, TKey>>())
            using (var userRoleRepository = provider.GetRequiredService<IRepository<TUserRole, TKey>>())
            {
                var userIds = (await userRoleRepository.GetByWhereAsync(
                    $"[{nameof(IdentityUserRole<TKey>.RoleId)}] = @{nameof(role.Id)}",
                    new { role.Id }))
                    .Select(m => m.UserId);
                var users = await userRepository.GetByWhereAsync(
                    $"[{nameof(IdentityUser<TKey>.Id)}] IN @{nameof(userIds)}",
                    userIds);

                return users.ToList();
            }
        }

        public override async Task<bool> IsInRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (string.IsNullOrWhiteSpace(normalizedRoleName))
            {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }

            var role = await FindRoleAsync(normalizedRoleName, cancellationToken);

            if (role == null)
            {
                throw new InvalidOperationException($"Role {normalizedRoleName} not found.");
            }

            using (var userRoleRepository = provider.GetRequiredService<IRepository<TUserRole, TKey>>())
            {
                var results = await userRoleRepository.GetByWhereAsync(
                    $"[{nameof(IdentityUserRole<TKey>.UserId)}] = @{nameof(user.Id)} AND [{nameof(IdentityUserRole<TKey>.RoleId)}] = @roleId",
                    new { user.Id, roleId = role.Id });

                return results.Any();
            }
        }

        public override async Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            

            using (var repository = provider.GetRequiredService<IRepository<TUserClaim, TKey>>())
            {
                foreach(var claim in claims)
                {
                    var userClaim = CreateUserClaim(user, claim);
                    await repository.DeleteAsync(userClaim);
                }
            }
        }

        public override async Task RemoveFromRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (string.IsNullOrWhiteSpace(normalizedRoleName))
            {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }

            var role = await FindRoleAsync(normalizedRoleName, cancellationToken);

            if (role == null)
            {
                throw new InvalidOperationException($"Role {normalizedRoleName} not found.");
            }

            var userRole = CreateUserRole(user, role);

            using (var repository = provider.GetRequiredService<IRepository<TUserRole, TKey>>())
            {
                await repository.DeleteAsync(userRole);
            }
        }

        public override async Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var userLogin = await FindUserLoginAsync(user.Id, loginProvider, providerKey, cancellationToken);
            if (userLogin != null)
            {
                using (var repository = provider.GetRequiredService<IRepository<TUserLogin, TKey>>())
                {
                    await repository.DeleteAsync(userLogin);
                }
            }
        }

        public override async Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default(CancellationToken))
        {
            await RemoveClaimsAsync(user, new[] { claim }, cancellationToken);
            await AddClaimsAsync(user, new[] { newClaim }, cancellationToken);
        }

        protected override async Task AddUserTokenAsync(TUserToken token)
        {
            ThrowIfDisposed();

            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            using (var repository = provider.GetRequiredService<IRepository<TUserToken, TKey>>())
            {
                // HACK: Workaround insertion limitation without PK
                await repository.InsertManyAsync(new[] { token });
            }
        }

        protected override async Task<TRole> FindRoleAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            using (var repository = provider.GetRequiredService<IRepository<TRole, TKey>>())
            {
                var results = await repository.GetByWhereAsync(
                    $"[{nameof(IdentityRole<TKey>.NormalizedName)}] = @{nameof(normalizedRoleName)}",
                    new { normalizedRoleName });
                return results.SingleOrDefault();
            }
        }

        protected override async Task<TUserToken> FindTokenAsync(TUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            using (var repository = provider.GetRequiredService<IRepository<TUserToken, TKey>>())
            {
                var results = await repository.GetByWhereAsync(
                    $@"[{nameof(IdentityUserToken<TKey>.UserId)}] = @{nameof(user.Id)} AND
[{nameof(IdentityUserToken<TKey>.LoginProvider)}] = @{nameof(loginProvider)} AND
[{nameof(IdentityUserToken<TKey>.Name)}] = @{nameof(name)}",
                    new { user.Id, loginProvider, name });
                return results.SingleOrDefault();
            }
        }

        protected override Task<TUser> FindUserAsync(TKey userId, CancellationToken cancellationToken)
        {
            return FindByIdAsync(userId?.ToString(), cancellationToken);
        }

        protected override async Task<TUserLogin> FindUserLoginAsync(TKey userId, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            using (var repository = provider.GetRequiredService<IRepository<TUserLogin, TKey>>())
            {
                var results = await repository.GetByWhereAsync(
                    $@"[{nameof(IdentityUserLogin<TKey>.UserId)}] = @{nameof(userId)} AND
[{nameof(IdentityUserLogin<TKey>.LoginProvider)}] = @{nameof(loginProvider)} AND
[{nameof(IdentityUserLogin<TKey>.ProviderKey)}] = @{nameof(providerKey)}",
                    new { userId, loginProvider, providerKey });
                return results.SingleOrDefault();
            }
        }

        protected override async Task<TUserLogin> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            using (var repository = provider.GetRequiredService<IRepository<TUserLogin, TKey>>())
            {
                var results = await repository.GetByWhereAsync(
                    $@"[{nameof(IdentityUserLogin<TKey>.LoginProvider)}] = @{nameof(loginProvider)} AND
[{nameof(IdentityUserLogin<TKey>.ProviderKey)}] = @{nameof(providerKey)}",
                    new { loginProvider, providerKey });
                return results.SingleOrDefault();
            }
        }

        protected override async Task<TUserRole> FindUserRoleAsync(TKey userId, TKey roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            using (var repository = provider.GetRequiredService<IRepository<TUserRole, TKey>>())
            {
                var results = await repository.GetByWhereAsync(
                    $"[{nameof(IdentityUserRole<TKey>.UserId)}] = @{nameof(userId)} AND [{nameof(IdentityUserRole<TKey>.RoleId)}] = @{nameof(roleId)}",
                    new { userId, roleId });
                return results.SingleOrDefault();
            }
        }

        protected override async Task RemoveUserTokenAsync(TUserToken token)
        {
            ThrowIfDisposed();

            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            using (var repository = provider.GetRequiredService<IRepository<TUserToken, TKey>>())
            {
                await repository.DeleteAsync(token);
            }
        }
    }
}
