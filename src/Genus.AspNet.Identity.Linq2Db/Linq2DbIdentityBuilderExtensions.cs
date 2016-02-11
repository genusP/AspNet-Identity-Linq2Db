using Microsoft.AspNet.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genus.AspNet.Identity.Linq2Db
{
    public static class Linq2DbIdentityBuilderExtensions
    {
        public static IdentityBuilder AddLinqToDbStores(this IdentityBuilder builder)
        {
            builder.Services.TryAdd(GetDefaultServices(builder.UserType, builder.RoleType));
            return builder;
        }

        public static IdentityBuilder AddLinqToDbStores<TKey>(this IdentityBuilder builder)
            where TKey : IEquatable<TKey>
        {
            builder.Services.TryAdd(GetDefaultServices(builder.UserType, builder.RoleType, typeof(TKey)));
            return builder;
        }

        private static IServiceCollection GetDefaultServices(Type userType, Type roleType, Type keyType=null)
        {
            Type userStoreType;
            Type roleStoreType;
            if (keyType != null)
            {
                userStoreType = typeof(UserStore<,,>).MakeGenericType(userType, roleType, keyType);
                roleStoreType = typeof(RoleStore<,>).MakeGenericType(roleType, keyType);
            }
            else
            {
                userStoreType = typeof(UserStore<,>).MakeGenericType(userType, roleType);
                roleStoreType = typeof(RoleStore<>).MakeGenericType(roleType);
            }

            var services = new ServiceCollection();
            services.AddScoped(
                typeof(IUserStore<>).MakeGenericType(userType),
                userStoreType);
            services.AddScoped(
                typeof(IRoleStore<>).MakeGenericType(roleType),
                roleStoreType);
            return services;
        }
    }
}
