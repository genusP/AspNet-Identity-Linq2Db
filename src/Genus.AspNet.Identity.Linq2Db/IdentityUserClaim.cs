using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genus.AspNet.Identity.Linq2Db
{
    [Table(Name ="AspNetUserClaims")]
    public class IdentityUserClaim<TKey>
        where TKey : IEquatable<TKey>
    {
        [Column(IsPrimaryKey =true, IsIdentity =true)]
        public int Id { get; set; }

        [Column]
        public TKey UserId { get; set; }

        [Column]
        public string ClaimType { get; set; }

        [Column]
        public string ClaimValue { get; set; }
    }
}
