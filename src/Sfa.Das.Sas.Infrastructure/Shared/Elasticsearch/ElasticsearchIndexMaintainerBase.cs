﻿using SFA.DAS.NLog.Logger;

namespace Sfa.Das.Sas.Indexer.Infrastructure.Elasticsearch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Nest;
    using Sfa.Das.Sas.Indexer.ApplicationServices.Shared.Services;

    public abstract class ElasticsearchIndexMaintainerBase : IMaintainSearchIndexes
    {
        private readonly string _typeOfIndex;

        protected ElasticsearchIndexMaintainerBase(IElasticsearchCustomClient elasticsearchCustomClient, IElasticsearchMapper elasticsearchMapper, ILog log, string typeOfIndex)
        {
            Client = elasticsearchCustomClient;
            Log = log;
            ElasticsearchMapper = elasticsearchMapper;
            _typeOfIndex = typeOfIndex;
        }

        protected IElasticsearchCustomClient Client { get; }

        protected ILog Log { get; }

        protected IElasticsearchMapper ElasticsearchMapper { get; }

        public virtual bool AliasExists(string aliasName)
        {
            var aliasExistsResponse = Client.AliasExists(aliasName);

            return aliasExistsResponse.Exists;
        }

        public abstract void CreateIndex(string indexName);

        public virtual void CreateIndexAlias(string aliasName, string indexName)
        {
            Client.Alias(a => a.Add(add => add.Index(indexName).Alias(aliasName)));
        }

        public virtual bool DeleteIndex(string indexName)
        {
            return Client.DeleteIndex(indexName).Acknowledged;
        }

        public virtual bool DeleteIndexes(Func<string, bool> indexNameMatch)
        {
            var result = true;

            var allIndices = Client.IndicesStats(Indices.All).Indices;

            if (allIndices?.Count > 0)
            {
                var indicesToBeDeleted = allIndices.Select(x => x.Key).Where(indexNameMatch);

                Log.Debug($"Deleting {indicesToBeDeleted.Count()} old {_typeOfIndex} indexes...");

                foreach (var index in indicesToBeDeleted)
                {
                    Log.Debug($"Deleting {index}");
                    result = result && this.DeleteIndex(index);
                }

                Log.Debug("Deletion completed...");
            }
            else
            {
                Log.Warn("Could not find indices to delete.");
            }

            return result;
        }

        public virtual bool IndexIsCompletedAndContainsDocuments(string indexName, int totalAmountDocuments)
        {
            Log.Debug($"Amount of documents to index: {totalAmountDocuments}");
            var r1 = Client.DocumentCount(indexName).Count;
            Log.Debug($"Amount of documents indexed: {r1}");
            long r2 = 0;
            do
            {
                System.Threading.Thread.Sleep(15000);
                r2 = Client.DocumentCount(indexName).Count;
                Log.Debug($"Amount of documents indexed: {r2}");

                if (r1 == 0 && r2 == 0)
                {
                    return false;
                }

                Log.Debug($"Comparing {r1} against {r2}");

                if (r1 < r2)
                {
                    r1 = r2;
                    r2 = 0;
                }

            } while (r1 != r2);

            Log.Debug($"Total amount of documents indexed: {r1}");

            return r1 == totalAmountDocuments;
        }

        public virtual bool IndexExists(string indexName)
        {
            return Client.IndexExists(indexName).Exists;
        }

        public virtual void SwapAliasIndex(string aliasName, string newIndexName)
        {
            var existingIndexesOnAlias = Client.GetIndicesPointingToAlias(aliasName);
            var aliasRequest = new BulkAliasRequest { Actions = new List<IAliasAction>() };

            foreach (var existingIndexOnAlias in existingIndexesOnAlias)
            {
                aliasRequest.Actions.Add(new AliasRemoveAction { Remove = new AliasRemoveOperation { Alias = aliasName, Index = existingIndexOnAlias } });
            }

            aliasRequest.Actions.Add(new AliasAddAction { Add = new AliasAddOperation { Alias = aliasName, Index = newIndexName } });

            Client.Alias(aliasRequest);
        }

        public void LogResponse(BulkResponse[] elementIndexResult, string documentType)
        {
            var totalCount = 0;
            var took = 0;
            var errorCount = 0;

            foreach (var bulkResponse in elementIndexResult)
            {
                totalCount += bulkResponse.Items.Count();
                took += (int)bulkResponse.Took;
                errorCount += bulkResponse.ItemsWithErrors.Count();
            }

            LogBulk(documentType, totalCount, took, errorCount);

            foreach (var bulkResponse in elementIndexResult.Where(bulkResponse => bulkResponse.Errors))
            {
                ReportErrors(bulkResponse, documentType);
            }
        }

        private void ReportErrors(BulkResponse result, string documentType)
        {
            foreach (var item in result.ItemsWithErrors)
            {
                var properties = new Dictionary<string, object> { { "DocumentType", documentType }, { "Index", item.Index }, { "Reason", item.Error.Reason }, { "Id", item.Id } };
                Log.Warn($"Error indexing entry with id {item.Id}", properties);
            }
        }

        private void LogBulk(string documentType, int totalCount, int took, int errorCount)
        {
            var properties = new Dictionary<string, object>
            {
                { "DocumentType", documentType },
                { "TotalCount", totalCount },
                { "Identifier", "DocumentCount" },
                { "ExecutionTime", took },
                { "ErrorCount", errorCount }
            };

            if (errorCount != 0)
            {
                Log.Error(new Exception(), $"Total of {totalCount - errorCount} / {totalCount} {documentType} documents were indexed successfully", properties);
            }
            else
            {
                Log.Info($"Total of {totalCount - errorCount} / {totalCount} {documentType} documents were indexed successfully", properties);
            }
        }
    }
}