﻿using System;

namespace Sfa.Das.Sas.Tools.MetaDataCreationTool.Services
{
    using System.Collections.Generic;
    using Nest;
    using Sfa.Das.Sas.Indexer.ApplicationServices.Lars.Services;
    using Sfa.Das.Sas.Indexer.ApplicationServices.Shared.Settings;
    using Sfa.Das.Sas.Indexer.Core.Apprenticeship.Models.Standard;
    using Sfa.Das.Sas.Indexer.Core.Models.Framework;
    using Sfa.Das.Sas.Indexer.Infrastructure.Elasticsearch;
    using Sfa.Das.Sas.Tools.MetaDataCreationTool.Services.Interfaces;

    public sealed class ElasticsearchLarsDataService : IElasticsearchLarsDataService
    {
        private readonly IElasticsearchCustomClient _elasticsearchCustomClient;
        private readonly IIndexSettings<IMaintainLarsIndex> _larsSettings;

        public ElasticsearchLarsDataService(
            IElasticsearchCustomClient elasticsearchCustomClient,
            IIndexSettings<IMaintainLarsIndex> larsSettings)
        {
            _elasticsearchCustomClient = elasticsearchCustomClient;
            _larsSettings = larsSettings;
        }

        public IEnumerable<LarsStandard> GetListOfStandards()
        {
            var size = GetLarsStandardsSize();

            var standards = _elasticsearchCustomClient
                .Search<LarsStandard>(s => s
                    .Index(_larsSettings.IndexesAlias)
                    .Query(q => q
                        .Bool(b => b
                            .Filter(f => f
                                .Term(t => t.Field("documentType").Value("StandardLars")))))
                    .From(0)
                    .Size(size));

            return standards.Documents;
        }

        public IEnumerable<FrameworkMetaData> GetListOfFrameworks()
        {
            var size = GetLarsFrameworksSize();

            var frameworks = _elasticsearchCustomClient
                .Search<FrameworkMetaData>(s => s
                    .Index(_larsSettings.IndexesAlias)
                    .Query(q => q
                        .Bool(b => b
                            .Filter(f => f
                                .Term(t => t.Field("documentType").Value("FrameworkLars")))))
                    .From(0)
                    .Size(size));

            return frameworks.Documents;
        }

        private int GetLarsStandardsSize()
        {
            var response = _elasticsearchCustomClient
                .Search<LarsStandard>(s => s
                    .Index(_larsSettings.IndexesAlias)
                    .Query(q => q
                        .Bool(b => b
                            .Filter(f => f
                                .Term(t => t.Field("documentType").Value("StandardLars"))))));

            if (!response.IsValid)
            {
                throw new Exception($"{response.ServerError.Error.Reason} {response.ServerError.Error.Index}", response.OriginalException);
            }

            return (int)response.HitsMetadata.Total.Value;
        }

        private int GetLarsFrameworksSize()
        {
            var response = _elasticsearchCustomClient
                .Search<FrameworkMetaData>(s => s
                    .Index(_larsSettings.IndexesAlias)
                    .Query(q => q
                        .Bool(b => b
                            .Filter(f => f
                                .Term(t => t.Field("documentType").Value("FrameworkLars"))))));

            if (!response.IsValid)
            {
                throw new Exception($"{response.ServerError.Error.Reason} {response.ServerError.Error.Index}", response.OriginalException);
            }

            return (int)response.HitsMetadata.Total.Value;
        }
    }
}