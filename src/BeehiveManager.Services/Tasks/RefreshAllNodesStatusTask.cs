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

using Etherna.BeehiveManager.Domain;
using Etherna.BeehiveManager.Domain.Models;
using Etherna.BeehiveManager.Domain.Models.BeeNodeAgg;
using Etherna.BeehiveManager.Services.Utilities;
using Etherna.BeeNet.Clients.DebugApi;
using Hangfire;
using MongoDB.Driver;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Etherna.BeehiveManager.Services.Tasks
{
    public class RefreshAllNodesStatusTask : IRefreshAllNodesStatusTask
    {
        // Consts.
        public const string TaskId = "refreshAllNodesStatusTask";

        // Fields.
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IBeeNodesManager beeNodesManager;
        private readonly IBeehiveContext context;

        // Constructors.
        public RefreshAllNodesStatusTask(
            IBackgroundJobClient backgroundJobClient,
            IBeeNodesManager beeNodesManager,
            IBeehiveContext context)
        {
            this.backgroundJobClient = backgroundJobClient;
            this.beeNodesManager = beeNodesManager;
            this.context = context;
        }

        // Methods.
        public async Task RunAsync()
        {
            // List all nodes.
            await context.BeeNodes.Collection.Find(FilterDefinition<BeeNode>.Empty, new FindOptions { NoCursorTimeout = true })
                .ForEachAsync(async node =>
                {
                    // Verify if has addresses.
                    if (node.Addresses is null)
                        backgroundJobClient.Enqueue<IRetrieveNodeAddressesTask>(task => task.RunAsync(node.Id));

                    // Get info.
                    var nodeClient = beeNodesManager.GetBeeNodeClient(node);
                    if (nodeClient.DebugClient is null) //skip if doesn't have a debug api config
                        return;

                    long totalUncashed = 0;
                    try
                    {
                        var peersResponse = await nodeClient.DebugClient.ChequebookChequeGetAsync();
                        var peers = peersResponse.Lastcheques.Select(c => c.Peer);

                        foreach (var peer in peers)
                        {
                            var cumulativePayout = 0L;
                            var cashedPayout = 0L;

                            try
                            {
                                var chequeResponse = await nodeClient.DebugClient.ChequebookChequeGetAsync(peer);
                                cumulativePayout = chequeResponse.Lastreceived.Payout;
                            }
                            catch (BeeNetDebugApiException) { }

                            try
                            {
                                var cashoutResponse = await nodeClient.DebugClient.ChequebookCashoutGetAsync(peer);
                                cashedPayout = cashoutResponse.CumulativePayout;
                            }
                            catch (BeeNetDebugApiException) { }

                            totalUncashed += cumulativePayout - cashedPayout;
                        }
                    }
                    catch (BeeNetDebugApiException) { return; } //issues contacting the node instance api
                    catch (HttpRequestException) { return; }

                    // Update node.
                    node.Status = new BeeNodeStatus(totalUncashed);

                    // Save changes.
                    await context.SaveChangesAsync();
                });
        }
    }
}