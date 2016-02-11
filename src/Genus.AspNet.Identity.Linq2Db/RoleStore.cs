using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Claims;
using LinqToDB;
using System.ComponentModel;

namespace Genus.AspNet.Identity.Linq2Db
{
    public class RoleStore<TRole> : RoleStore<TRole, string>
        where TRole : IdentityRole<string>
    {
        public RoleStore(IDataContext dataContext):base(dataContext){}
    }

    public class RoleStore<TRole, TKey> :
        IQueryableRoleStore<TRole>,
        IRoleClaimStore<TRole>
        where TRole : IdentityRole<TKey>
        where TKey:IEquatable<TKey>
    {
        private readonly IDataContext _dataContext;

        public RoleStore(IDataContext dataContext)
        {
            if (dataContext == null)
                throw new ArgumentNullException(nameof(dataContext));
            _dataContext = dataContext;
        }

        public IQueryable<TRole> Roles
        {
            get
            {
                return _dataContext.GetTable<TRole>();
            }
        }

        public Task AddClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));
            _dataContext.GetTable<IdentityRoleClaim<TKey>>()
                .Insert(() => new IdentityRoleClaim<TKey> { RoleId = role.Id, ClaimType = claim.Type, ClaimValue = claim.Value });
            return Task.FromResult(false);
        }

        public async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            await Task.Run(() => _dataContext.Insert(role));
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            await Task.Run(() => _dataContext.Delete(role));
            return IdentityResult.Success;
        }

        public void Dispose()
        {
        }

        private TKey GetIdFromString(string strId)
        {
            if (strId == null)
                return default(TKey);
            return (TKey)TypeDescriptor.GetConverter(typeof(TKey)).ConvertFromInvariantString(strId);
        }

        private string GetStringFromId(TKey id)
        {
            if (id == null)
                return null;
            return TypeDescriptor.GetConverter(typeof(TKey)).ConvertToInvariantString(id);
        }

        public Task<TRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var intRoleId = GetIdFromString(roleId);
            return _dataContext.GetTable<TRole>().SingleOrDefaultAsync(_ => _.Id.Equals(intRoleId));
        }

        public Task<TRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _dataContext.GetTable<TRole>().SingleOrDefaultAsync(_ => _.NormalizedName == normalizedRoleName);
        }

        public async Task<IList<Claim>> GetClaimsAsync(TRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            var query = from rc in _dataContext.GetTable<IdentityRoleClaim<TKey>>()
                        where rc.RoleId.Equals(role.Id)
                        select new Claim(rc.ClaimType, rc.ClaimValue);
            return await query.ToListAsync();
        }

        public Task<string> GetNormalizedRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            return Task.FromResult(role.NormalizedName);
        }

        public Task<string> GetRoleIdAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            return Task.FromResult(GetStringFromId(role.Id));
        }

        public Task<string> GetRoleNameAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            return Task.FromResult(role.Name);
        }

        public Task RemoveClaimAsync(TRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            return Task.Run(() =>{
                _dataContext.GetTable<IdentityRoleClaim<TKey>>()
                .Where(_ => _.RoleId.Equals(role.Id) && _.ClaimType == claim.Type && _.ClaimValue == claim.Value)
                .Delete();
            });
        }

        public Task SetNormalizedRoleNameAsync(TRole role, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            role.NormalizedName = normalizedName;
            return Task.FromResult(normalizedName);
        }

        public Task SetRoleNameAsync(TRole role, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            role.Name = roleName;
            return Task.FromResult(roleName);
        }

        public Task<IdentityResult> UpdateAsync(TRole role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            return Task.Run(() =>
            {
                _dataContext.Update(role);
                return IdentityResult.Success;
            });
        }
    }
}
