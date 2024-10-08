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

using Etherna.BeehiveManager.Services.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Etherna.BeehiveManager.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void StartBeeNodeLiveManager(this IApplicationBuilder appBuilder)
        {
            ArgumentNullException.ThrowIfNull(appBuilder, nameof(appBuilder));

            var beeNodeLiveManager = appBuilder.ApplicationServices.GetRequiredService<IBeeNodeLiveManager>();

            var task = beeNodeLiveManager.LoadAllNodesAsync();
            task.Wait();

            beeNodeLiveManager.StartHealthHeartbeat();
        }
    }
}
