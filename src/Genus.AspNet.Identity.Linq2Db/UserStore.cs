using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using LinqToDB;
using System.Security.Claims;
using System.Transactions;
using System.ComponentModel;

namespace Genus.AspNet.Identity.Linq2Db
{
    public class UserStore<TUser, TRole>: UserStore<TUser, TRole, string>
        where TUser: IdentityUser<string>
        where TRole: IdentityRole<string>
    {
        public UserStore(IDataContext dataContext):base(dataContext){}
    }

    public class UserStore<TUser, TRole, TKey> :
        IUserLoginStore<TUser>,
        IUserRoleStore<TUser>,
        IUserClaimStore<TUser>,
        IUserPasswordStore<TUser>,
        IUserSecurityStampStore<TUser>,
        IUserEmailStore<TUser>,
        IUserLockoutStore<TUser>,
        IUserPhoneNumberStore<TUser>,
        IQueryableUserStore<TUser>,
        IUserTwoFactorStore<TUser>
        where TUser: IdentityUser<TKey>
        where TRole: IdentityRole<TKey>
        where TKey: IEquatable<TKey>

    {
        private readonly IDataContext _dataContext;

        public UserStore(IDataContext dataContext)
        {
            if (dataContext == null)
                throw new ArgumentNullException(nameof(dataContext));
            _dataContext = dataContext;
        }

        public IQueryable<TUser> Users
        {
            get
            {
                return _dataContext.GetTable<TUser>();
            }
        }

        public Task AddClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (claims == null)
                throw new ArgumentNullException(nameof(claims));
            using (new TransactionScope())
            {
                foreach (var claim in claims)
                {
                    _dataContext.GetTable<IdentityUserClaim<TKey>>().Insert(
                        () => new IdentityUserClaim<TKey> { UserId = user.Id, ClaimType = claim.Type, ClaimValue = claim.Value }
                        );
                }
            }
            return Task.FromResult(false);
        }

        public Task AddLoginAsync(TUser user, UserLoginInfo loginInfo, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (loginInfo == null)
                throw new ArgumentNullException(nameof(loginInfo));
            using(new TransactionScope())
            {
                var login = new IdentityUserLogin<TKey>
                {
                    UserId = user.Id,
                    ProviderKey = loginInfo.ProviderKey,
                    LoginProvider = loginInfo.LoginProvider,
                    ProviderDisplayName = loginInfo.ProviderDisplayName
                };
                _dataContext.Insert(login);
            }
            return Task.FromResult(false);
            
        }

        public async Task AddToRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException("Value cannot be null or empty", nameof(roleName));
            }
            var roleEntity = await _dataContext.GetTable<TRole>().SingleOrDefaultAsync(r => r.Name.ToUpper() == roleName.ToUpper(), cancellationToken);
            if (roleEntity == null)
            {
                throw new InvalidOperationException($"Role {roleName} not found");
            }
            var userRole = new IdentityUserRole<TKey> { UserId = user.Id, RoleId = roleEntity.Id };
            _dataContext.Insert(userRole);
        }

        public Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            _dataContext.Insert(user);
             return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> DeleteAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            _dataContext.Delete(user);
            return Task.FromResult(IdentityResult.Success);
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

        public async Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _dataContext.GetTable<TUser>().FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);
        }

        public async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var id = GetIdFromString(userId);
            return await _dataContext.GetTable<TUser>().FirstOrDefaultAsync(u => u.Id.Equals(id), cancellationToken);
        }

        public async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var query = from l in _dataContext.GetTable<IdentityUserLogin<TKey>>()
                        join u in _dataContext.GetTable<TUser>() on l.UserId equals u.Id
                        where l.LoginProvider == loginProvider && l.ProviderKey == providerKey
                        select u;
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _dataContext.GetTable<TUser>().FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);
        }

        public Task<int> GetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.AccessFailedCount);
        }

        public async Task<IList<Claim>> GetClaimsAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            var query = from c in _dataContext.GetTable<IdentityUserClaim<TKey>>()
                        where c.UserId.Equals(user.Id)
                        select new Claim(c.ClaimType, c.ClaimValue);
            return await query.ToListAsync(cancellationToken);
        }

        public Task<string> GetEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task<bool> GetLockoutEnabledAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.LockoutEnabled);
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.LockoutEnd);
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            var query = from l in _dataContext.GetTable<IdentityUserLogin<TKey>>()
                        where l.UserId.Equals( user.Id)
                        select new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.ProviderDisplayName);
            return await query.ToListAsync();
        }

        public Task<string> GetNormalizedEmailAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.NormalizedEmail);
        }

        public Task<string> GetNormalizedUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetPasswordHashAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.PasswordHash);
        }

        public Task<string> GetPhoneNumberAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            var query = from r in _dataContext.GetTable<TRole>()
                        join ur in _dataContext.GetTable<IdentityUserRole<TKey>>() on r.Id equals ur.RoleId
                        where ur.UserId.Equals(user.Id)
                        select r.Name;
            return await query.ToListAsync();
        }

        public Task<string> GetSecurityStampAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.SecurityStamp);
        }

        public Task<bool> GetTwoFactorEnabledAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task<string> GetUserIdAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.UserName);
        }

        public async Task<IList<TUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            var query = from cl in _dataContext.GetTable<IdentityUserClaim<TKey>>()
                        join u in _dataContext.GetTable<TUser>() on cl.UserId equals u.Id
                        where cl.ClaimType == claim.Type && cl.ClaimValue == claim.Value
                        select u;
            return await query.ToListAsync();
        }

        public async Task<IList<TUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Value cannot be null or empty", nameof(roleName));

            var query = from r in _dataContext.GetTable<TRole>()
                        join ur in _dataContext.GetTable<IdentityUserRole<TKey>>() on r.Id equals ur.RoleId
                        join u in _dataContext.GetTable<TUser>() on ur.UserId equals u.Id
                        select u;
            return await query.ToListAsync();
        }

        public Task<bool> HasPasswordAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.PasswordHash!=null);
        }

        public Task<int> IncrementAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _dataContext.GetTable<TUser>()
                .Where(u=>u.Id.Equals(user.Id))
                .Set(u=>u.AccessFailedCount, u=>u.AccessFailedCount+1)
                .Update();
            return Task.FromResult(user.AccessFailedCount+1);
        }

        public async Task<bool> IsInRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            var query = from r in _dataContext.GetTable<TRole>()
                        join ur in _dataContext.GetTable<IdentityUserRole<TKey>>() on r.Id equals ur.RoleId
                        join u in _dataContext.GetTable<TUser>() on ur.UserId equals u.Id
                        where r.Name.ToUpper()==roleName && u.Id.Equals(user.Id)
                        select u;
            return await query.AnyAsync(cancellationToken);
        }

        public Task RemoveClaimsAsync(TUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (claims == null)
                throw new ArgumentNullException(nameof(claims));
            foreach (var claim in claims)
            {
                _dataContext.GetTable<IdentityUserClaim<TKey>>()
                    .Delete(uc => uc.UserId.Equals(user.Id) && uc.ClaimType == claim.Type && uc.ClaimValue == claim.Value);
            }
            return Task.FromResult(false);
        }

        public async Task RemoveFromRoleAsync(TUser user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentNullException("Value cannot be null or empty", nameof(roleName));

            var role = await _dataContext.GetTable<IdentityRole<TKey>>().SingleOrDefaultAsync(r => r.Name.ToUpper() == roleName.ToUpper());
            if (role != null) {
                _dataContext.GetTable<IdentityUserRole<TKey>>()
                    .Delete(ur => ur.UserId.Equals(user.Id) && ur.RoleId.Equals(role.Id));
            }
        }

        public Task RemoveLoginAsync(TUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            _dataContext.GetTable<IdentityUserLogin<TKey>>()
                .Delete(_ => _.UserId.Equals(user.Id) && _.LoginProvider == loginProvider && _.ProviderKey == providerKey);
            return Task.FromResult(false);
        }

        public Task ReplaceClaimAsync(TUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));
            if (newClaim == null)
                throw new ArgumentNullException(nameof(newClaim));

            var matchedClaims =  _dataContext
                .GetTable<IdentityUserClaim<TKey>>()
                .Where(uc => uc.UserId.Equals(user.Id) && uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type)
                .Set(uc=>uc.ClaimType, uc=>newClaim.Type)
                .Set(uc=>uc.ClaimValue, uc=>newClaim.Value)
                .Update();
            return Task.FromResult(false);
        }

        public Task ResetAccessFailedCountAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.AccessFailedCount= 0;
            return Task.FromResult(0);
        }

        public Task SetEmailAsync(TUser user, string email, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.Email= email;
            return Task.FromResult(email);
        }

        public Task SetEmailConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.EmailConfirmed = confirmed;
            return Task.FromResult(confirmed);
        }

        public Task SetLockoutEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.LockoutEnabled = enabled;
            return Task.FromResult(enabled);
        }

        public Task SetLockoutEndDateAsync(TUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.LockoutEnd= lockoutEnd;
            return Task.FromResult(lockoutEnd);
        }

        public Task SetNormalizedEmailAsync(TUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.NormalizedEmail = normalizedEmail;
            return Task.FromResult(normalizedEmail);
        }

        public Task SetNormalizedUserNameAsync(TUser user, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.NormalizedUserName = normalizedName;
            return Task.FromResult(normalizedName);
        }

        public Task SetPasswordHashAsync(TUser user, string passwordHash, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.PasswordHash = passwordHash;
            return Task.FromResult(passwordHash);
        }

        public Task SetPhoneNumberAsync(TUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.PhoneNumber = phoneNumber;
            return Task.FromResult(phoneNumber);
        }

        public Task SetPhoneNumberConfirmedAsync(TUser user, bool confirmed, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.PhoneNumberConfirmed = confirmed;
            return Task.FromResult(confirmed);
        }

        public Task SetSecurityStampAsync(TUser user, string stamp, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.SecurityStamp= stamp;
            return Task.FromResult(stamp);
        }

        public Task SetTwoFactorEnabledAsync(TUser user, bool enabled, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.TwoFactorEnabled= enabled;
            return Task.FromResult(enabled);
        }

        public Task SetUserNameAsync(TUser user, string userName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.UserName = userName;
            return Task.FromResult(userName);
        }

        public async Task<IdentityResult> UpdateAsync(TUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            await Task.Run(() => _dataContext.Update(user));
            return IdentityResult.Success;
        }
    }
}
