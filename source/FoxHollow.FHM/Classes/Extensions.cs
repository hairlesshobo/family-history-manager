/**
 *  Family History Manager - https://code.foxhollow.cc/fhm/
 *
 *  A cross platform tool to help organize and preserve all types
 *  of family history
 * 
 *  Copyright (c) 2020-2023 Steve Cross <flip@foxhollow.cc>
 *
 *  This Source Code Form is subject to the terms of the Mozilla Public
 *  License, v. 2.0. If a copy of the MPL was not distributed with this
 *  file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using Splat;

namespace FoxHollow.FHM.Classes;

public static class Extensions
{
    public static T GetRequiredService<T>(this IReadonlyDependencyResolver resolver, string contract = null)
        => resolver.GetService<T>(contract) ?? 
            throw new InvalidOperationException($"Unable to resolve service of type '{typeof(T).Name}'");
}