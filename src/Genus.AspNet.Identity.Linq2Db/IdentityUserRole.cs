using LinqToDB.Mapping;
using System;

namespace Genus.AspNet.Identity.Linq2Db
{
    [Table(Name ="AspNetUserRoles")]
    public class IdentityUserRole<TKey>
        where TKey : IEquatable<TKey>
    {
        [PrimaryKey(0)]
        public TKey RoleId { get; set; }

        [PrimaryKey(1)]
        public TKey UserId { get; set; }
    }
}