using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using Nest;
using SFA.DAS.NLog.Logger;
using Sfa.Das.Sas.Indexer.ApplicationServices.Shared;
using Sfa.Das.Sas.Indexer.ApplicationServices.Shared.MetaData;
using Sfa.Das.Sas.Indexer.ApplicationServices.Shared.Services;
using Sfa.Das.Sas.Indexer.ApplicationServices.Shared.Utility;
using Sfa.Das.Sas.Indexer.Infrastructure.Elasticsearch;
using Sfa.Das.Sas.Indexer.Infrastructure.Elasticsearch.Configuration;
using Sfa.Das.Sas.Indexer.Infrastructure.Services;
using Sfa.Das.Sas.Indexer.Infrastructure.Settings;
using Sfa.Das.Sas.Indexer.Infrastructure.Shared.Elasticsearch;
using Sfa.Das.Sas.Indexer.Infrastructure.Shared.Services;
using StructureMap;

namespace Sfa.Das.Sas.Indexer.Infrastructure.Shared.DependencyResolution
{
    public class InfrastructureRegistry : Registry
    {
        public InfrastructureRegistry()
        {
            // Shared
            For<IElasticsearchConfiguration>().Use<ElasticsearchConfiguration>();
            For<IElasticsearchSettings>().Use<ElasticsearchSettings>();
            For<IConvertFromCsv>().Use<CsvService>();
            For<IVstsClient>().Use<VstsClient>();
            For<IHttpGetFile>().Use<HttpService>();
            For<IHttpGet>().Use<HttpService>();
            For<IHttpPost>().Use<HttpService>();
            For<IInfrastructureSettings>().Use<InfrastructureSettings>();
            For<ILog>().Use(x => new NLogLogger(x.ParentType, new ConsoleLoggingContext(), GetProperties())).AlwaysUnique();
            For<IUnzipStream>().Use<ZipFileExtractor>();
            For<IElasticsearchMapper>().Use<ElasticsearchMapper>();
            For<IElasticClient>().Use<ElasticClient>();
            For<IElasticsearchCustomClient>().Use<ElasticsearchCustomClient>();
            For<IIndexerServiceFactory>().Use<IndexerServiceFactory>();
            For<IMonitoringService>().Use<MonitoringService>();
            For<IOrganisationTypeProcessor>().Use<OrganisationTypeProcessor>();

            if (Debugger.IsAttached)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
            }
        }

        private IDictionary<string, object> GetProperties()
        {
            var properties = new Dictionary<string, object>();
            properties.Add("Version", GetVersion());
            return properties;
        }

        private string GetVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.ProductVersion;
        }
    }
}