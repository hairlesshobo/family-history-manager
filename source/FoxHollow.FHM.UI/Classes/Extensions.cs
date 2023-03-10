using System;
using System.Threading.Tasks;
using Splat;

namespace FoxHollow.FHM.UI.Classes;

public static class Extensions
{
    public static T GetRequiredService<T>(this IReadonlyDependencyResolver resolver, string contract = null)
        => resolver.GetService<T>(contract) ?? 
            throw new InvalidOperationException($"Unable to resolve service of type '{typeof(T).Name}'");
}