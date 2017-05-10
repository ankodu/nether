﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Nether.Analytics
{
    public class Message
    {
        public string Id { get; set; }
        public string MessageType { get; set; }
        public string Version { get; set; }
        public DateTime EnqueueTimeUtc { get; set; }
        public Dictionary<string, string> Properties { get; } = new Dictionary<string, string>();

        public override string ToString()
        {
            var str = new StringBuilder();

            str.AppendLine($"Id:             {Id}");
            str.AppendLine($"MessageType:    {MessageType}");
            str.AppendLine($"Version:        {Version}");
            str.AppendLine($"EnqueueTimeUtc: {EnqueueTimeUtc}");
            str.AppendLine($"Properties:");

            foreach (var prop in Properties.Keys)
            {
                str.AppendLine($"    {prop}: {Properties[prop]}");
            }

            return str.ToString();
        }
    }
}