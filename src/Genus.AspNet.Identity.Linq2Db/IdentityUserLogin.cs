using LinqToDB.Mapping;
using System;

namespace Genus.AspNet.Identity.Linq2Db
{
    [Table(Name ="AspNetUserLogins")]
    public class IdentityUserLogin<TKey>
        where TKey : IEquatable<TKey>
    {
        [PrimaryKey(0)]
        public string LoginProvider { get; set; }

        [PrimaryKey(1)]
        public string ProviderDisplayName { get; set; }

        [Column]
        public string ProviderKey { get; set; }

        [Column]
        public TKey UserId { get; set; }
    }
}