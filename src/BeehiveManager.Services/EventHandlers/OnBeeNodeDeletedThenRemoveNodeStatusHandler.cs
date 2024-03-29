﻿//   Copyright 2021-present Etherna SA
// 
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
// 
//       http://www.apache.org/licenses/LICENSE-2.0
// 
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.BeehiveManager.Domain.Models;
using Etherna.BeehiveManager.Services.Utilities;
using Etherna.DomainEvents;
using Etherna.DomainEvents.Events;
using System.Threading.Tasks;

namespace Etherna.BeehiveManager.Services.EventHandlers
{
    internal sealed class OnBeeNodeDeletedThenRemoveNodeStatusHandler : EventHandlerBase<EntityDeletedEvent<BeeNode>>
    {
        // Fields.
        private readonly IBeeNodeLiveManager beeNodeLiveManager;

        // Constructor.
        public OnBeeNodeDeletedThenRemoveNodeStatusHandler(
            IBeeNodeLiveManager beeNodeLiveManager)
        {
            this.beeNodeLiveManager = beeNodeLiveManager;
        }

        // Methods.
        public override Task HandleAsync(EntityDeletedEvent<BeeNode> @event)
        {
            beeNodeLiveManager.RemoveBeeNode(@event.Entity.Id);
            return Task.CompletedTask;
        }
    }
}
