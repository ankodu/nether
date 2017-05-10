﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Nether.Analytics.DataLake;
using Nether.Analytics.EventHubs;
using Nether.Analytics.GeoLocation;
using Nether.Analytics.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Nether.Analytics.Host
{
    internal class Program
    {
        private const string AppSettingsFile = "appsettings.json";

        // Configuration parameters
        private const string NAH_EHListener_ConnectionString = "NAH_EHLISTENER_CONNECTIONSTRING";
        private const string NAH_EHListener_EventHubPath = "NAH_EHLISTENER_EVENTHUBPATH";
        private const string NAH_EHListener_ConsumerGroup = "NAH_EHLISTENER_CONSUMERGROUP";
        private const string NAH_EHListener_StorageConnectionString = "NAH_EHLISTENER_STORAGECONNECTIONSTRING";
        private const string NAH_EHListener_LeaseContainerName = "NAH_EHLISTENER_LEASECONTAINERNAME";

        private const string NAH_AAD_Domain = "NAH_AAD_DOMAIN";
        private const string NAH_AAD_ClientId = "NAH_AAD_CLIENTID";
        private const string NAH_AAD_ClientSecret = "NAH_AAD_CLIENTSECRET";

        private const string NAH_Azure_SubscriptionId = "NAH_AZURE_SUBSCRIPTIONID";

        private const string NAH_Azure_DLSOutputManager_AccountName = "NAH_AZURE_DLSOUTPUTMANAGER_ACCOUNTNAME";


        private static IConfigurationRoot s_configuration;

        private static void Main(string[] args)
        {
            Greet();

            SetupConfigurationProviders();

            // Check that all configurations are set before continuing
            var configStatus = CheckConfigurationStatus(
                NAH_EHListener_ConnectionString,
                NAH_EHListener_EventHubPath,
                NAH_EHListener_ConsumerGroup,
                NAH_EHListener_StorageConnectionString,
                NAH_EHListener_LeaseContainerName,
                NAH_AAD_Domain,
                NAH_AAD_ClientId,
                NAH_AAD_ClientSecret,
                NAH_Azure_SubscriptionId,
                NAH_Azure_DLSOutputManager_AccountName);

            if (configStatus != ConfigurationStatus.Ok)
            {
                // Exiting due to missing configuration
                Console.WriteLine("Press any key to continue");
                Console.ReadKey(true);
                return;
            }


            // Setup Listener. This will be the same for all pipelines we are building.
            var listenerConfig = new EventHubsListenerConfiguration
            {
                EventHubConnectionString = s_configuration[NAH_EHListener_ConnectionString],
                EventHubPath = s_configuration[NAH_EHListener_EventHubPath],
                ConsumerGroupName = s_configuration[NAH_EHListener_ConsumerGroup],
                StorageConnectionString = s_configuration[NAH_EHListener_StorageConnectionString],
                LeaseContainerName = s_configuration[NAH_EHListener_LeaseContainerName]
            };
            var listener = new EventHubsListener(listenerConfig);

            // Setup Message Parser. By default we are using Nether JSON Messages
            // Setting up parser that knows how to parse those messages.
            var parser = new EventHubListenerMessageJsonParser();

            // User a builder to create routing infrastructure for messages and the pipelines
            var builder = new MessageRouterBuilder();

            // Setting up "Geo Clustering Recipe"

            var clusteringSerializer = new CsvOutputFormatter("id", "type", "version", "enqueueTimeUtc", "gameSessionId", "lat", "lon", "geoHash", "geoHashPrecision", "geoHashCenterLat", "geoHashCenterLon");

            var clusteringDlsOutputManager = new DataLakeStoreOutputManager(
                clusteringSerializer,
                new PipelineDateFilePathAlgorithm(newFileOption: NewFileNameOptions.Every5Minutes),

                domain: s_configuration[NAH_AAD_Domain],
                clientId: s_configuration[NAH_AAD_ClientId],
                clientSecret: s_configuration[NAH_AAD_ClientSecret],
                subscriptionId: s_configuration[NAH_Azure_SubscriptionId],
                adlsAccountName: s_configuration[NAH_Azure_DLSOutputManager_AccountName]);

            var clusteringConsoleOutputManager = new ConsoleOutputManager(clusteringSerializer);

            builder.Pipeline("clustering")
                .HandlesMessageType("geo-location", "1.0.0")
                .HandlesMessageType("geo-location", "1.0.1")
                .AddHandler(new GeoHashMessageHandler { CalculateGeoHashCenterCoordinates = true })
                .OutputTo(clusteringConsoleOutputManager, clusteringDlsOutputManager);


            // Build all pipelines
            var router = builder.Build();

            // Attach the differeing parts of the message processor together
            var messageProcessor = new MessageProcessor<EventHubListenerMessage>(listener, parser, router);

            // Run in an async context since main method is not allowed to be marked as async
            Task.Run(async () =>
            {
                await messageProcessor.ProcessAndBlockAsync();
            }).GetAwaiter().GetResult();
        }


        private static void SetupConfigurationProviders()
        {
            var defaultConfigValues = new Dictionary<string, string>
            {
                {NAH_EHListener_ConnectionString, ""},
                {NAH_EHListener_EventHubPath, "nether"},
                {NAH_EHListener_ConsumerGroup, "$default"},
                {NAH_EHListener_StorageConnectionString, ""},
                {NAH_EHListener_LeaseContainerName, "sync"}
            };

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddInMemoryCollection(defaultConfigValues)
                .AddJsonFile(AppSettingsFile, optional: true)
                .AddEnvironmentVariables();

            s_configuration = configBuilder.Build();
        }

        private static ConfigurationStatus CheckConfigurationStatus(params string[] settings)
        {
            const int maxValueLengthPrinted = 100;

            Console.WriteLine("Using the following configuration values:");
            Console.WriteLine();

            var missingSettings = new List<string>();

            foreach (var setting in settings)
            {
                var val = s_configuration[setting];

                if (string.IsNullOrWhiteSpace(val))
                {
                    missingSettings.Add(setting);
                }
                else
                {
                    ConsoleEx.Write(ConsoleColor.DarkGray, setting);
                    Console.WriteLine(" : ");
                    ConsoleEx.WriteLine(ConsoleColor.Yellow, "  " + (val.Length < maxValueLengthPrinted ? val : val.Substring(0, maxValueLengthPrinted - 3) + "..."));
                }
            }

            Console.WriteLine();

            if (missingSettings.Count > 0)
            {
                Console.WriteLine("The following setting(s) are missing values:");
                Console.WriteLine();


                foreach (var setting in missingSettings)
                {
                    ConsoleEx.WriteLine(ConsoleColor.Magenta, $"  {setting}");
                }

                Console.WriteLine();
                Console.WriteLine($"Make sure to set all the above configuration parameters in {AppSettingsFile} or using Environment Variables.");
                Console.WriteLine("Then start Nether.Analytics.Host again.");
                Console.WriteLine();

                return ConfigurationStatus.MissingConfig;
            }
            else
            {
                return ConfigurationStatus.Ok;
            }
        }

        private static void Greet()
        {
            Console.WriteLine();
            Console.WriteLine(@"   _   _      _   _               ");
            Console.WriteLine(@"  | \ | | ___| |_| |__   ___ _ __ ");
            Console.WriteLine(@"  |  \| |/ _ \ __| '_ \ / _ \ '__|");
            Console.WriteLine(@"  | |\  |  __/ |_| | | |  __/ |   ");
            Console.WriteLine(@"  |_| \_|\___|\__|_| |_|\___|_| Analytics");
            Console.WriteLine(@"  Message Processor Host ");
            Console.WriteLine();
        }
    }

    public enum ConfigurationStatus
    {
        Ok,
        MissingConfig
    }

    public static class ConsoleEx
    {
        public static void Write(ConsoleColor color, string value)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(value);
            Console.ForegroundColor = currentColor;
        }

        public static void WriteLine(ConsoleColor color, string value)
        {
            Write(color, value + Environment.NewLine);
        }
    }
}
