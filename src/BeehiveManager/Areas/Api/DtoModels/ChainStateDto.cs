﻿using Etherna.BeehiveManager.Services.Utilities.Models;
using System;

namespace Etherna.BeehiveManager.Areas.Api.DtoModels
{
    public class ChainStateDto
    {
        public ChainStateDto(ChainState chainState)
        {
            if (chainState is null)
                throw new ArgumentNullException(nameof(chainState));

            Block = chainState.Block;
            ChainTip = chainState.ChainTip;
            CurrentPrice = chainState.CurrentPrice;
            SourceNodeId = chainState.SourceNodeId;
            TimeStamp = chainState.TimeStamp;
            TotalAmount = chainState.TotalAmount;
        }

        public int Block { get; }
        public int ChainTip { get; }
        public int CurrentPrice { get; }
        public string SourceNodeId { get; }
        public DateTime TimeStamp { get; }
        public int TotalAmount { get; }
    }
}
