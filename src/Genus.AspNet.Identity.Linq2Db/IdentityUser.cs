using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genus.AspNet.Identity.Linq2Db
{
    public class IdentityUser : IdentityUser<string>
    {
        public IdentityUser()
        {
            Id = Guid.NewGuid().ToString();
        }

        public IdentityUser(string userName) : this()
        {
            UserName = userName;
        }
    }

    [Table(Name ="AspNetUsers")]
    public class IdentityUser<TKey>
        where TKey: IEquatable<TKey>
    {
        [PrimaryKey]
        public TKey Id { get; set; }

        [Column]
        public string NormalizedEmail { get; set; }

        [Column]
        public string NormalizedUserName { get; set; }

        [Column]
        public int AccessFailedCount { get; set; }

        [Column]
        public string Email { get; set; }

        [Column]
        public bool EmailConfirmed { get; set; }

        [Column]
        public bool LockoutEnabled { get; set; }

        [Column]
        public DateTimeOffset? LockoutEnd { get; set; }

        [Column]
        public string PasswordHash { get; set; }

        [Column]
        public string PhoneNumber { get; set; }

        [Column]
        public bool PhoneNumberConfirmed { get; set; }

        [Column]
        public string SecurityStamp { get; set; }

        [Column]
        public bool TwoFactorEnabled { get; set; }

        [Column]
        public string UserName { get; set; }
    }
}
