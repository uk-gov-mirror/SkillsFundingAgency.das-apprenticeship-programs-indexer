﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.NLog.Logger;
using Sfa.Das.Sas.Indexer.ApplicationServices.Shared.Logging.Models;
using Sfa.Das.Sas.Indexer.Core.Provider.Models;
using Sfa.Das.Sas.Indexer.Infrastructure.Provider.CourseDirectory;
using Sfa.Das.Sas.Indexer.Infrastructure.Settings;

namespace Sfa.Das.Sas.Indexer.Infrastructure.CourseDirectory
{
    public sealed class CourseDirectoryClient : IAsyncRequestHandler<CourseDirectoryRequest, CourseDirectoryResult>
    {
        private readonly IInfrastructureSettings _settings;
        private readonly ICourseDirectoryProviderDataService _courseDirectoryProviderDataService;

        private readonly ILog _logger;

        public CourseDirectoryClient(
            IInfrastructureSettings settings,
            ICourseDirectoryProviderDataService courseDirectoryProviderDataService,
            ILog logger)
        {
            _settings = settings;
            _courseDirectoryProviderDataService = courseDirectoryProviderDataService;
            _logger = logger;
        }

        public async Task<CourseDirectoryResult> Handle(CourseDirectoryRequest message)
        {
            _logger.Debug("Starting to retrieve Course Directory Providers");
            var stopwatch = Stopwatch.StartNew();
            _courseDirectoryProviderDataService.BaseUri = new Uri(_settings.CourseDirectoryUri);

            if (!string.IsNullOrWhiteSpace(_settings.CourseDirectoryApiKey))
            {
                _courseDirectoryProviderDataService.Credentials = new ApiKeyServiceClientCredentials(_settings.CourseDirectoryApiKey);
            }

            var responseAsync = await _courseDirectoryProviderDataService.BulkprovidersWithOperationResponseAsync();

            _logger.Debug(
    $"Retrieved {responseAsync.Body.Count} providers from course directory",
    new TimingLogEntry { ElaspedMilliseconds = stopwatch.Elapsed.TotalMilliseconds });

            _courseDirectoryProviderDataService.Dispose();

            return new CourseDirectoryResult { Providers = responseAsync.Body };
        }
    }
}