﻿//   Copyright 2021-present Etherna Sagl
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
using Etherna.BeeNet;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.BeehiveManager.Services.Utilities
{
    public interface IBeeNodesStatusManager
    {
        // Properties.
        IEnumerable<BeeNodeClient> HealthyClients { get; }

        // Methods.
        BeeNodeStatus AddBeeNode(BeeNode beeNode);
        Task<BeeNodeStatus> GetBeeNodeStatusAsync(string nodeId);
        Task LoadAllNodesAsync();
        bool RemoveBeeNode(string nodeId);
        void StartHealthHeartbeat();
        void StopHealthHeartbeat();
        BeeNodeStatus? TrySelectHealthyNodeAsync(BeeNodeSelectionMode mode);
        void UpdateNodeInfo(BeeNode node);
    }
}