using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genus.AspNet.Identity.Linq2Db
{
    public class IdentityRole : IdentityRole<string>
    {
        public IdentityRole()
        {
            Id = Guid.NewGuid().ToString();
        }

        public IdentityRole(string roleName) : this()
        {
            Name = roleName;
        }
    }

    [Table(Name ="AspNetRoles")]
    public class IdentityRole<TKey>
        where TKey:IEquatable<TKey>
    {
        [Column(IsIdentity =true, IsPrimaryKey =true)]
        public TKey Id { get; set; }

        [Column]
        public string Name { get; set; }

        [Column]
        public string NormalizedName { get; internal set; }
    }
}
