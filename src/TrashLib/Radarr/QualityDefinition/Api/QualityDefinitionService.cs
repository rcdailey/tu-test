﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using TrashLib.Radarr.QualityDefinition.Api.Objects;

namespace TrashLib.Radarr.QualityDefinition.Api
{
    internal class QualityDefinitionService : IQualityDefinitionService
    {
        private readonly IServerInfo _serverInfo;

        public QualityDefinitionService(IServerInfo serverInfo)
        {
            _serverInfo = serverInfo;
        }

        private string BaseUrl => _serverInfo.BuildUrl();

        public async Task<List<RadarrQualityDefinitionItem>> GetQualityDefinition()
        {
            return await BaseUrl
                .AppendPathSegment("qualitydefinition")
                .GetJsonAsync<List<RadarrQualityDefinitionItem>>();
        }

        public async Task<IList<RadarrQualityDefinitionItem>> UpdateQualityDefinition(
            IList<RadarrQualityDefinitionItem> newQuality)
        {
            return await BaseUrl
                .AppendPathSegment("qualityDefinition/update")
                .PutJsonAsync(newQuality)
                .ReceiveJson<List<RadarrQualityDefinitionItem>>();
        }
    }
}