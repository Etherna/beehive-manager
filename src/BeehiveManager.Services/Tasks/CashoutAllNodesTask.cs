﻿using Etherna.BeehiveManager.Domain;
using Etherna.BeehiveManager.Domain.Models;
using Etherna.BeehiveManager.Services.Utilities;
using Etherna.BeeNet.Clients.DebugApi;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Etherna.BeehiveManager.Services.Tasks
{
    public class CashoutAllNodesTask : ICashoutAllNodesTask
    {
        // Consts.
        public const string TaskId = "cashoutAllNodesTask";
        public const long MinAmount = 100_000_000_000_000; //10^14, 0.01 BZZ

        // Fields.
        private readonly IBeeNodesManager beeNodesManager;
        private readonly IBeehiveContext context;

        // Constructors.
        public CashoutAllNodesTask(
            IBeeNodesManager beeNodesManager,
            IBeehiveContext context)
        {
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
                    // Get info.
                    var nodeClient = beeNodesManager.GetBeeNodeClient(node);
                    if (nodeClient.DebugClient is null) //skip if doesn't have a debug api config
                        return;

                    var totalCashedout = 0L;
                    var txs = new List<string>();
                    try
                    {
                        // Enumerate peers.
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

                            var uncashed = cumulativePayout - cashedPayout;

                            // Cashout.
                            if (uncashed >= MinAmount)
                            {
                                try
                                {
                                    var cashoutResponse = await nodeClient.DebugClient.ChequebookCashoutPostAsync(peer);
                                    totalCashedout += uncashed;
                                    txs.Add(cashoutResponse.TransactionHash);
                                }
                                catch (BeeNetDebugApiException) { }
                            }
                        }
                    }
                    catch (BeeNetDebugApiException) { return; } //issues contacting the node instance api
                    catch (HttpRequestException) { return; }

                    // Add log.
                    if (totalCashedout > 0)
                    {
                        var log = new CashoutNodeLog(node, txs, totalCashedout);
                        await context.NodeLogs.CreateAsync(log);
                    }
                });
        }
    }
}