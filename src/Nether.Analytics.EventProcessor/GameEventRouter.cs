// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Host.Bindings;

namespace Nether.Analytics.EventProcessor
{
    /// <summary>
    /// Routes Game Events depending on their Game Event Type to correct and registered
    /// Event Type Actions. Any Game Events with unknown Event Type or Event Formats will
    /// be routed to registered actions for handling as well.
    /// </summary>
    public class GameEventRouter
    {
        private readonly Action<GameEventData> _resolveGameEventTypeAction;
        private readonly Dictionary<string, Action<GameEventData>> _gameEventTypeActions;
        private Action<GameEventData> _unknownGameEventTypeAction;
        private Action<GameEventData> _unknownGameEventFormatAction;
        private Action _flushWriteQueuesAction;

        public GameEventRouter(Action<GameEventData> resolveGameEventTypeAction,
            Action<GameEventData> unknownGameEventFormatAction = null,
            Action<GameEventData> unknownGameEventTypeAction = null,
            Action flushWriteQueuesAction = null)
        {
            _resolveGameEventTypeAction = resolveGameEventTypeAction;
            _unknownGameEventFormatAction = unknownGameEventFormatAction;
            _unknownGameEventTypeAction = unknownGameEventTypeAction;
            _flushWriteQueuesAction = flushWriteQueuesAction;

            _gameEventTypeActions = new Dictionary<string, Action<GameEventData>>();
        }

        /// <summary>
        /// Registeres Game Event Types and what action to take if received
        /// </summary>
        /// <param name="gameEventType">Game Event Type</param>
        /// <param name="action">Action(gameEventType, data) to be called when Game Event is received</param>
        public void RegisterKnownGameEventTypeHandler(string gameEventType, Action<GameEventData> action)
        {
            _gameEventTypeActions.Add(gameEventType, action);
        }

        /// <summary>
        /// Handles a Game Event by looking at the Game Event Type and routing the event data to the
        /// correct registered action.
        /// </summary>
        /// <param name="data">Game Event Data</param>
        public void HandleGameEvent(GameEventData gameEventData)
        {
            try
            {
                _resolveGameEventTypeAction(gameEventData);
            }
            catch (Exception)
            {
                // Resolving game event type failed. Unknown Game Event format?!?!
                // Invoke action to handle Unknown Game Event Formats if registered (not null)
                _unknownGameEventFormatAction?.Invoke(gameEventData);

                // No more can be done, so return
                return;
            }

            // GameEventData object from now on contains game event type and version information
            // since it was set by the _gameEventTypeResolver

            // Check if game event type is known
            if (!_gameEventTypeActions.ContainsKey(gameEventData.VersionedType))
            {
                // No registered action found for the found Game Event Type
                // Invoke action to handle Unknown Game Event Type if registered (not null)
                _unknownGameEventTypeAction?.Invoke(gameEventData);

                // No more can be done, so return
                return;
            }

            // Get correct game event handler from dictionary of registered handlers
            var handler = _gameEventTypeActions[gameEventData.VersionedType];
            // Pass game event data to correct action
            handler(gameEventData);
        }

        public void Flush()
        {
            _flushWriteQueuesAction();
        }
    }
}