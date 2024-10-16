﻿// Copyright 2021-present Etherna SA
// This file is part of BeehiveManager.
// 
// BeehiveManager is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// BeehiveManager is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with BeehiveManager.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.BeehiveManager.Areas.Api.DtoModels;
using Etherna.BeehiveManager.Services.Utilities;

namespace Etherna.BeehiveManager.Areas.Api.Services
{
    public class ChainControllerService : IChainControllerService
    {
        // Fields.
        private readonly IBeeNodeLiveManager liveManager;

        // Constructor.
        public ChainControllerService(
            IBeeNodeLiveManager liveManager)
        {
            this.liveManager = liveManager;
        }

        // Methods.
        public ChainStateDto? GetChainState() =>
            liveManager.ChainState is null ? null :
            new ChainStateDto(liveManager.ChainState);
    }
}
