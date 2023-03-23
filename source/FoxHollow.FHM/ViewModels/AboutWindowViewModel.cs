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
using System.Diagnostics;
using System.IO.Packaging;
using System.Reflection;

namespace FoxHollow.FHM.ViewModels;

public class AboutWindowViewModel : ViewModelBase
{
    /// <summary>
    ///     Name of the project
    /// </summary>
    public string ProjectName { get; set; }

    /// <summary>
    ///     URL of the project
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    ///     Current application version
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    ///     Application description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///     Project copyright notice
    /// </summary>
    public string Copyright { get; set; }

    /// <summary>
    ///     License that the project is released under
    /// </summary>
    public string License { get; set; }

    /// <summary>
    ///     Author of the project
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    ///     Default constructor
    /// </summary>
    public AboutWindowViewModel(IServiceProvider services)
        : base(services)
    {
        var info = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
        this.ProjectName = info.ProductName;
        this.Version = "Version " + info.ProductVersion;
        this.Copyright = info.LegalCopyright.Split(" <")?[0];

        // TODO: FIX ME
        this.Url = "https://code.foxhollow.cc/fhm/";
        this.Description = "A cross platform tool to help organize and preserve all types of family history";
        this.License = "Apache License, Version 2.0";
        this.Author = "Steve Cross";
    }
}