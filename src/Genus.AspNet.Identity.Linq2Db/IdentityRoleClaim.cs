using LinqToDB.Mapping;
using System;

namespace Genus.AspNet.Identity.Linq2Db
{
    [Table(Name ="AspNetRoleClaims")]
    public class IdentityRoleClaim<TKey>
        where TKey : IEquatable<TKey>
    {
        [Column(IsPrimaryKey =true, IsIdentity =true)]
        public int Id { get; set; }

        [Column]
        public string ClaimType { get; set; }

        [Column]
        public string ClaimValue { get; set; }

        [Column]
        public TKey RoleId { get; set; }
    }
}