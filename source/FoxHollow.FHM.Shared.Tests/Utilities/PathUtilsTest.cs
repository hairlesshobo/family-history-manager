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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using FoxHollow.FHM.Shared.Exceptions;
using FoxHollow.FHM.Shared.Models;

namespace FoxHollow.FHM.Shared.Utilities;

public class PathUtils_Tests
{
    [Theory]
    [InlineData(@"D:\Some\Path\To\File.txt", "D:/Some/Path/To/File.txt")]
    [InlineData(@"\\server\Some\Path\To\File.txt", "//server/Some/Path/To/File.txt")]
    [InlineData(@"//server\Some\Path\To\File.txt", "//server/Some/Path/To/File.txt")]
    [InlineData(@".\Some\Path\To\File.txt", "./Some/Path/To/File.txt")]
    [InlineData(@"/Some/Path/To/File.txt", "/Some/Path/To/File.txt")]
    public void CleanPath_Test(string input, string expected)
    {
        Assert.Equal(expected, PathUtils.CleanPath(input));
    }

    [Theory]
    [InlineData(@"remote://", "/")]
    [InlineData(@"/root/path", "/root/path")]
    [InlineData(@"C:\Windows\cmd.exe", @"C:\Windows\cmd.exe")]
    [InlineData(@"smb://server/path", "/server/path")]
    [InlineData(@"default:///path", "/path")]
    [InlineData(@"file://D:\path\to\file.txt", @"D:\path\to\file.txt")]
    public void StripProtocol_Test(string input, string expected)
    {
        Assert.Equal(expected, PathUtils.StripProtocol(input));
    }

    [Theory]
    [InlineData(@"")]
    [InlineData(@"    ")]
    [InlineData(null)]
    public void StripProtocol_Throw(string input)
    {
        Assert.Throws<ArgumentException>(() => PathUtils.StripProtocol(input));
    }

    [Theory]
    [InlineData(@"remote://path/to/file.txt", ".txt")]
    [InlineData(@"./file.tiff", ".tiff")]
    [InlineData(@"/root/path", "")]
    [InlineData(@"/root/path.", ".")]
    [InlineData(@"C:\Windows\cmd.exe", @".exe")]
    [InlineData(@"/path/to/file.tiff.yaml", ".yaml")]
    public void GetExtension_Test(string input, string expected)
    {
        Assert.Equal(expected, PathUtils.GetExtension(input));
    }

    [Theory]
    [InlineData(@"")]
    [InlineData(@"    ")]
    [InlineData(null)]
    public void GetExtension_Throw(string input)
    {
        Assert.Throws<ArgumentException>(() => PathUtils.GetExtension(input));
    }
}