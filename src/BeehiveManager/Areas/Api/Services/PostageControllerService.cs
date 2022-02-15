﻿using Etherna.BeehiveManager.Areas.Api.DtoModels;
using Etherna.BeehiveManager.Services.Utilities;
using Etherna.BeeNet.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.BeehiveManager.Areas.Api.Services
{
    public class PostageControllerService : IPostageControllerService
    {
        // Fields.
        private readonly IBeeNodesStatusManager beeNodeStatusManager;

        // Constructor.
        public PostageControllerService(
            IBeeNodesStatusManager beeNodeStatusManager)
        {
            this.beeNodeStatusManager = beeNodeStatusManager;
        }

        // Methods.
        public async Task<PostageBatchRefDto> BuyPostageBatchAsync(
            long amount, int depth, long? gasPrice, bool immutable, string? label, string? nodeId)
        {
            // Try to select an healthy node.
            var beeNodeStatus = nodeId is null ?
                beeNodeStatusManager.TrySelectHealthyNodeAsync(BeeNodeSelectionMode.RoundRobin) :
                await beeNodeStatusManager.GetBeeNodeStatusAsync(nodeId);

            if (beeNodeStatus is null)
                throw new InvalidOperationException("No healthy nodes available");

            // Buy postage.
            var batchId = await beeNodeStatus.Client.DebugClient!.BuyPostageBatchAsync(amount, depth, label, immutable, gasPrice);

            return new PostageBatchRefDto(batchId, beeNodeStatus.Id);
        }

        public async Task<IEnumerable<PostageBatchDto>> GetPostageBatchesFromAllNodes()
        {
            var batches = new List<PostageBatchDto>();
            foreach (var nodeStatus in beeNodeStatusManager.HealthyNodes)
            {
                try
                {
                    batches.AddRange((await nodeStatus.Client.DebugClient!.GetAllValidPostageBatchesFromAllNodesAsync())
                        .Select(b => new PostageBatchDto(b)));
                }
                catch (Exception e) when (e is BeeNetDebugApiException) { }
            }

            return batches;
        }
    }
}
