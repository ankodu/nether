﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;

namespace Nether.Analytics
{
    public interface IOutputManager
    {
        Task OutputMessageAsync(string pipelineName, int idx, Message msg);
        Task FlushAsync();
    }
}