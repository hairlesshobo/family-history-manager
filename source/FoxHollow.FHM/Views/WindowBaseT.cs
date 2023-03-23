//==========================================================================
//  Family History Manager - https://code.foxhollow.cc/fhm/
//
//  A cross platform tool to help organize and preserve all types
//  of family history
//==========================================================================
//  Copyright (c) 2020-2023 Steve Cross <flip@foxhollow.cc>
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//==========================================================================

using System;
using Avalonia.Controls;
using FoxHollow.FHM.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace FoxHollow.FHM.Views;

/// <summary>
///     Base class used for any windows that require custom view models
/// </summary>
/// <typeparam name="TViewModel">View model type</typeparam>
public class WindowBase<TViewModel> : Window
    where TViewModel : ViewModelBase
{
    protected IServiceProvider _services;
    protected TViewModel _model;

    /// <summary>
    ///     Constructor that expects a DI container to be passed. This constructor
    ///     automatically creates the view model
    /// </summary>
    /// <param name="services">DI Container</param>
    protected WindowBase(IServiceProvider services)
        : this(services, null)
    {
    }

    /// <summary>
    ///     Constructor that expects a DI container and view model instance to be passed.
    /// </summary>
    /// <param name="services">DI container</param>
    /// <param name="model">View model instance, if null, a new one will automatically be created</param>
    protected WindowBase(IServiceProvider services, TViewModel model)
    {
        if (model == null)
            model = ActivatorUtilities.CreateInstance<TViewModel>(services);

        _services = services ?? throw new ArgumentNullException(nameof(services));
        _model = model ?? throw new ArgumentNullException(nameof(model));

        this.DataContext = model;
    }
}