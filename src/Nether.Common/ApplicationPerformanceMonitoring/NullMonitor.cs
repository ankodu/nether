﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Nether.Common.ApplicationPerformanceMonitoring
{
    public class NullMonitor : IApplicationPerformanceMonitor
    {
        public void LogEvent(string eventName, string message = null, IDictionary<string, string> properties = null) { }
        public void LogError(Exception exception, string message = null, IDictionary<string, string> properties = null) { }
    }
}
