﻿namespace Sfa.Das.Sas.Indexer.UnitTests.Infrastructure.Elasticsearch
{
    using FluentAssertions;
    using Moq;
    using Nest;
    using NUnit.Framework;
    using SFA.DAS.NLog.Logger;
    using Sfa.Das.Sas.Indexer.Infrastructure.Elasticsearch;
    using Sfa.Das.Sas.Indexer.Infrastructure.Provider.Models.ElasticSearch;
    using Sfa.Das.Sas.Indexer.Infrastructure.Shared.Settings;

    [TestFixture]
    public class BulkProviderClientTests
    {
        [TestCase(3999, 4)]
        [TestCase(0, 0)]
        [TestCase(4000, 4)]
        [TestCase(4001, 5)]
        [TestCase(16000, 16)]
        public void BatchSizeTest(int provideCount, int tasks)
        {
            var sut = new BulkProviderClient("testindex", Mock.Of<IElasticsearchCustomClient>());

            for (int i = 0; i < provideCount; i++)
            {
                var frameworkProvider = new ProviderDocument(ElasticsearchDocumentTypes.PROVIDER_FRAMEWORK_DOCUMENT);
                sut.Index<ProviderDocument>(c => c.Document(frameworkProvider));
            }

            sut.GetTasks().Count.Should().Be(tasks);
        }

        [TestCase(3999, 4)]
        [TestCase(0, 0)]
        [TestCase(4000, 4)]
        [TestCase(4001, 5)]
        [TestCase(16000, 16)]
        public void ShouldCallClient(int provideCount, int callCount)
        {
            var mockElasticCustomClient = new Mock<ElasticsearchCustomClient>(Mock.Of<IElasticsearchClientFactory>(), Mock.Of<ILog>());

            var sut = new BulkProviderClient("testindex", mockElasticCustomClient.Object);

            for (int i = 0; i < provideCount; i++)
            {
                var frameworkProvider = new ProviderDocument(ElasticsearchDocumentTypes.PROVIDER_FRAMEWORK_DOCUMENT);
                sut.Index<ProviderDocument>(c => c.Document(frameworkProvider));
            }

            sut.GetTasks();

            mockElasticCustomClient.Verify(x => x.BulkAsync(It.IsAny<IBulkRequest>(), It.IsAny<string>()), Times.Exactly(callCount));
        }
    }
}
