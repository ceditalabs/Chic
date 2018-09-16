// Copyright (c) Cedita Ltd. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the solution root for license information.
using Chic.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chic.Identity
{
    public class RoleStore<TRole> : RoleStore<TRole, int>
        where TRole : IdentityRole<int>
    {
        public RoleStore(IServiceProvider provider, IdentityErrorDescriber describer) : base(provider, describer)
        {
        }
    }

    public class RoleStore<TRole, TKey> : RoleStore<TRole, TKey, IdentityUserRole<TKey>, IdentityRoleClaim<TKey>>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        public RoleStore(IServiceProvider provider, IdentityErrorDescriber describer) : base(provider, describer)
        {
        }
    }

    public class RoleStore<TRole, TKey, TUserRole, TRoleClaim> :
        RoleStoreBase<TRole, TKey, TUserRole, TRoleClaim>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TUserRole : IdentityUserRole<TKey>, new()
        where TRoleClaim : IdentityRoleClaim<TKey>, new()
    {
        public RoleStore(IServiceProvider provider, IdentityErrorDescriber describer) : base(describer ?? new IdentityErrorDescriber())
        {
            this.provider = provider;
        }

        private readonly IServiceProvider provider;

        public override IQueryable<TRole> Roles
        {
            get
            {
                return provider.GetRequiredService<IRepository<TRole, TKey>>().GetAllAsync().Result.AsQueryable();
            }
        }

        public override async Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            var roleClaim = CreateRoleClaim(role, claim);

            using (var repository = provider.GetRequiredService<IRepository<TRoleClaim, TKey>>())
            {
                // HACK: Workaround insertion limitation without PK
                await repository.InsertManyAsync(new[] { roleClaim });
            }
        }

        public override async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            using (var repository = provider.GetRequiredService<IRepository<TRole, TKey>>())
            {
                var insertedId = await repository.InsertAsync(role);
                role.Id = insertedId;
            }

            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            using (var repository = provider.GetRequiredService<IRepository<TRole, TKey>>())
            {
                await repository.DeleteAsync(role);
            }

            return IdentityResult.Success;
        }

        public override async Task<TRole> FindByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            var roleId = ConvertIdFromString(id);
            using (var repository = provider.GetRequiredService<IRepository<TRole, TKey>>())
            {
                return await repository.GetByIdAsync(roleId);
            }
        }

        public override async Task<TRole> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            using (var repository = provider.GetRequiredService<IRepository<TRole, TKey>>())
            {
                var results = await repository.GetByWhereAsync(
                    $"[{nameof(IdentityRole<TKey>.NormalizedName)}] = @{nameof(normalizedName)}",
                    new { normalizedName });
                return results.SingleOrDefault();
            }
        }

        public override async Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            using (var roleClaimRepository = provider.GetRequiredService<IRepository<TRoleClaim, TKey>>())
            {
                var claims = (await roleClaimRepository.GetByWhereAsync(
                    $"[{nameof(IdentityRoleClaim<TKey>.RoleId)}] = @{nameof(role.Id)}",
                    new { role.Id }))
                    .Select(m => m.ToClaim());

                return claims.ToList();
            }
        }

        public override async Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            var roleClaim = CreateRoleClaim(role, claim);

            using (var repository = provider.GetRequiredService<IRepository<TRoleClaim, TKey>>())
            {
                await repository.DeleteAsync(roleClaim);
            }
        }

        public override async Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            ThrowIfDisposed();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            using (var repository = provider.GetRequiredService<IRepository<TRole, TKey>>())
            {
                await repository.UpdateAsync(role);
            }

            return IdentityResult.Success;
        }
    }
}
