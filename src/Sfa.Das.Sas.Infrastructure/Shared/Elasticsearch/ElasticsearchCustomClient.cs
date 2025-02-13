using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Elasticsearch.Net;
using Nest;
using Polly;

using SFA.DAS.NLog.Logger;
using Sfa.Das.Sas.Indexer.ApplicationServices.Shared.Logging.Models;
using Sfa.Das.Sas.Indexer.Core.Exceptions;
using Policy = Polly.Policy;

namespace Sfa.Das.Sas.Indexer.Infrastructure.Elasticsearch
{
    public class ElasticsearchCustomClient : IElasticsearchCustomClient
    {
        private readonly ILog _logger;

        private readonly IElasticClient _client;

        public ElasticsearchCustomClient(IElasticsearchClientFactory elasticsearchClientFactory, ILog logger)
        {
            _client = elasticsearchClientFactory.GetElasticClient();
            _logger = logger;
        }

        public ISearchResponse<T> Search<T>(Func<SearchDescriptor<T>, ISearchRequest> selector, [CallerMemberName] string callerName = "")
            where T : class
        {
            var timer = Stopwatch.StartNew();
            var policy = Policy
                .Handle<FailedToPingSpecifiedNodeException>()
                .WaitAndRetry(5, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) * 10));

            var result = policy.Execute(() => SearchData(selector));
            ValidateResponse(result);
            SendLog(result.ApiCall, result.Took, timer.ElapsedMilliseconds, $"Elasticsearch.Search.{callerName}");
            return result;
        }

        public ExistsResponse IndexExists(IndexName index, [CallerMemberName] string callerName = "")
        {
            var timer = Stopwatch.StartNew();
            var result = _client.Indices.Exists(index);
            ValidateResponse(result);
            SendLog(result.ApiCall, null, timer.ElapsedMilliseconds, $"Elasticsearch.IndexExists.{callerName}");
            return result;
        }

        public DeleteIndexResponse DeleteIndex(IndexName index, [CallerMemberName] string callerName = "")
        {
            var timer = Stopwatch.StartNew();
            var result = _client.Indices.Delete(index);
            ValidateResponse(result);
            SendLog(result.ApiCall, null, timer.ElapsedMilliseconds, $"Elasticsearch.DeleteIndex.{callerName}");
            return result;
        }

        public GetMappingResponse GetMapping<T>(Func<GetMappingDescriptor<T>, IGetMappingRequest> selector = null, [CallerMemberName] string callerName = "")
            where T : class
        {
            var timer = Stopwatch.StartNew();
            var result = _client.Indices.GetMapping(selector);
            ValidateResponse(result);
            SendLog(result.ApiCall, null, timer.ElapsedMilliseconds, $"Elasticsearch.GetMapping.{callerName}");
            return result;
        }

        public RefreshResponse Refresh(IRefreshRequest request, [CallerMemberName] string callerName = "")
        {
            var timer = Stopwatch.StartNew();
            var result = _client.Indices.Refresh(request);
            ValidateResponse(result);
            SendLog(result.ApiCall, null, timer.ElapsedMilliseconds, $"Elasticsearch.Refresh.{callerName}");
            return result;
        }

        public RefreshResponse Refresh(Indices indices, Func<RefreshDescriptor, IRefreshRequest> selector = null, string callerName = "")
        {
            var timer = Stopwatch.StartNew();
            var result = _client.Indices.Refresh(indices);
            ValidateResponse(result);
            SendLog(result.ApiCall, null, timer.ElapsedMilliseconds, $"Elasticsearch.Refresh.{callerName}");
            return result;
        }

        public ExistsResponse AliasExists(string aliasName, string callerName = "")
        {
            var timer = Stopwatch.StartNew();
            var result = _client.Indices.AliasExists(new AliasExistsRequest(aliasName));
            ValidateResponse(result);
            SendLog(result.ApiCall, null, timer.ElapsedMilliseconds, $"Elasticsearch.AliasExists.{callerName}");
            return result;
        }

        public BulkAliasResponse Alias(Func<BulkAliasDescriptor, IBulkAliasRequest> selector, string callerName = "")
        {
            var timer = Stopwatch.StartNew();
            var result = _client.Indices.BulkAlias(selector);
            ValidateResponse(result);
            SendLog(result.ApiCall, null, timer.ElapsedMilliseconds, $"Elasticsearch.Alias.{callerName}");
            return result;
        }

        public BulkAliasResponse Alias(IBulkAliasRequest request, string callerName = "")
        {
            var timer = Stopwatch.StartNew();
            var result = _client.Indices.BulkAlias(request);
            ValidateResponse(result);
            SendLog(result.ApiCall, null, timer.ElapsedMilliseconds, $"Elasticsearch.Alias.{callerName}");
            return result;
        }

        public IndicesStatsResponse IndicesStats(Indices indices, Func<IndicesStatsDescriptor, IIndicesStatsRequest> selector = null, string callerName = "")
        {
            var timer = Stopwatch.StartNew();
            var result = _client.Indices.Stats(indices, selector);
            ValidateResponse(result);
            SendLog(result.ApiCall, null, timer.ElapsedMilliseconds, $"Elasticsearch.IndicesStats.{callerName}");
            return result;
        }

        public IList<string> GetIndicesPointingToAlias(string aliasName, string callerName = "")
        {
            var timer = Stopwatch.StartNew();
            var result = _client.GetIndicesPointingToAlias(aliasName);
            SendLog(null, null, timer.ElapsedMilliseconds, $"Elasticsearch.GetIndicesPointingToAlias.{callerName}");
            return result.ToList();
        }

        public CreateIndexResponse CreateIndex(IndexName index, Func<CreateIndexDescriptor, ICreateIndexRequest> selector = null, string callerName = "")
        {
            var timer = Stopwatch.StartNew();
            var result = _client.Indices.Create(index, selector);
            ValidateResponse(result);
            SendLog(result.ApiCall, null, timer.ElapsedMilliseconds, $"Elasticsearch.CreateIndex.{callerName}");
            return result;
        }

        public virtual async Task<BulkResponse> BulkAsync(IBulkRequest request, string callerName = "")
        {
            var timer = Stopwatch.StartNew();
            var policy = Policy
                .Handle<TooManyRequestsException>()
                .WaitAndRetry(5, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) * 10));

            var result = await policy.Execute(() => BulkData(request));

            SendLog(null, null, timer.ElapsedMilliseconds, $"Elasticsearch.BulkAsync.{callerName}");
            return result;
        }

        public void BulkAllGeneric<T>(List<T> elementList, string indexName)
            where T : class
        {
            var elementCount = elementList.Count();

            var count = 0;

            var waitHandle = new CountdownEvent(1);

            var bulkAll = _client.BulkAll(elementList, b => b
                .Index(indexName)
                .BackOffRetries(5)
                .BackOffTime(TimeSpan.FromSeconds(15))
                .RefreshOnCompleted(true)
                .MaxDegreeOfParallelism(2)
                .BulkResponseCallback((c) =>
                {
                    if (!c.IsValid)
                    {
                        _logger.Debug($"Bulk Response was invalid: {c.DebugInformation}");
                    }
                })
                .Size(1000));

            bulkAll.Subscribe(observer: new BulkAllObserver(
                onNext: (b) =>
                {
                    count = count + 1000;

                    if (count > elementCount)
                    {
                        count = elementCount;
                    }

                    _logger.Debug($"Indexed group of {typeof(T)}: {count} of {elementCount}");
                },
                onError: (e) =>
                {
                    _logger.Error(e, e.Message);
                    waitHandle.Signal();
                },
                onCompleted: () =>
                {
                    waitHandle.Signal();
                }));
            waitHandle.Wait();
        }

        public void IndexMany<T>(List<T> entries, string indexName)
            where T : class
        {
            var smallLists = SplitAndReturn(entries, 1000);

            var smallListsAmount = smallLists.Count();
            var count = 1;

            var indexedDocuments = 0;

            foreach (var smallList in smallLists)
            {
                _logger.Debug($"Indexing group {count} of {smallListsAmount}");
                var result = _client.IndexMany(smallList, indexName);
                indexedDocuments = indexedDocuments + smallList.Count;
                _logger.Debug($"Indexed {indexedDocuments} documents");

                if (!result.IsValid)
                {
                    foreach (var item in result.ItemsWithErrors)
                    {
                        _logger.Warn($"Failed to index document {item.Id}: {item.Error}");
                    }
                }

                count++;
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }

        public CountResponse DocumentCount(IndexName index, [CallerMemberName] string callerName = "")
        {
            var timer = Stopwatch.StartNew();
            var result = _client.Count(new CountRequest(index));
            ValidateResponse(result);
            SendLog(result.ApiCall, null, timer.ElapsedMilliseconds, $"Elasticsearch.Count.{callerName}");
            return result;
        }

        private IEnumerable<List<T>> SplitAndReturn<T>(List<T> entries, int size)
        {
            for (var i = 0; i < entries.Count; i = i + size)
            {
                var actualSize = Math.Min(size, entries.Count - i);
                yield return entries.GetRange(i, actualSize);
            }
        }

        private ISearchResponse<T> SearchData<T>(Func<SearchDescriptor<T>, ISearchRequest> selector)
            where T : class
        {
            var response = _client.Search(selector);

            if (!response.IsValid && response.ApiCall.OriginalException.InnerException?.InnerException?.Message == "Failed to ping the specified node.")
            {
                _logger.Warn("Failedto ping the specified node, retrying");
                throw new FailedToPingSpecifiedNodeException();
            }

            if (!response.IsValid)
            {

                if (response.ApiCall.OriginalException != null)
                {
                    _logger.Error(response.ApiCall.OriginalException.InnerException, response.ApiCall.OriginalException.InnerException.Message);
                }

                throw new HttpException((int) response.ApiCall.HttpStatusCode, "Something failed trying to insert data into the bulk service", response.ApiCall.OriginalException);
            }

            return response;
        }

        private Task<BulkResponse> BulkData(IBulkRequest request)
        {
            var response = _client.BulkAsync(request);

            if (!response.Result.IsValid)
            {
                if ((response.Result.OriginalException != null && response.Result.OriginalException.Message.Contains("The underlying connection was closed: A connection that was expected to be kept alive was closed by the server"))
                    || (response.Result.ItemsWithErrors != null && response.Result.ItemsWithErrors.First().Status == 503))
                {
                    _logger.Warn("Elasticsearch overload, retrying");
                    throw new TooManyRequestsException();
                }

                if (response.Exception != null)
                {
                    foreach (var ex in response.Exception.InnerExceptions)
                    {
                        _logger.Error(ex, ex.Message);
                    }
                }

                throw new HttpException(response.Result.ItemsWithErrors.First().Status, "Something failed trying to insert data into the bulk service", response.Exception);
            }

            return response;
        }
        
        private void SendLog(IApiCallDetails apiCallDetails, long? took, double networkTime, string identifier)
        {
            string body = string.Empty;
            if (apiCallDetails?.RequestBodyInBytes != null)
            {
                body = System.Text.Encoding.Default.GetString(apiCallDetails.RequestBodyInBytes);
            }

            var logEntry = new ElasticSearchLogEntry
            {
                ReturnCode = apiCallDetails?.HttpStatusCode,
                SearchTime = took,
                NetworkTime = networkTime,
                Url = apiCallDetails?.Uri?.AbsoluteUri,
                Body = body
            };

            _logger.Debug($"ElasticsearchQuery: {identifier}", logEntry);
        }

        private void ValidateResponse(IResponse response)
        {
            var status = response?.ApiCall?.HttpStatusCode;
            if (response?.ApiCall == null || status == null)
            {
                SendLog(response?.ApiCall, null, 0, "Invalid response checking index");
                var reason = string.Empty;

                foreach (var message in response?.ApiCall?.OriginalException.InnerException?.Data.Values)
                {
                    reason = $"{reason}, {message}";
                }

                throw new ConnectionException($"The response from elastic search was not 200 : {response?.ApiCall?.OriginalException.Message} -> {reason}, {response?.ApiCall?.DebugInformation}", response?.ApiCall?.OriginalException);
            }

            if (!response.ApiCall.Success)
            {
                switch (status.Value)
                {
                    case (int)HttpStatusCode.OK:
                        return;
                    case (int)HttpStatusCode.Unauthorized:
                        throw new UnauthorizedAccessException("The request to elasticsearch was unauthorised", response.ApiCall.OriginalException);
                    default:
                        throw new HttpException(status.Value, $"The response from elastic search was {response.ApiCall.HttpStatusCode}\n {response.ApiCall.DebugInformation}", response.ApiCall.OriginalException);
                }
            }
        }
    }

    internal class TooManyRequestsException : Exception
    {
    }

    internal class FailedToPingSpecifiedNodeException : Exception
    {
    }
}