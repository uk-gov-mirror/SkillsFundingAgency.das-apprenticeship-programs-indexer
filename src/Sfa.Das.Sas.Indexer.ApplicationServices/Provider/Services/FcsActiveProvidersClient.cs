﻿using System.Collections.Generic;

namespace Sfa.Das.Sas.Indexer.ApplicationServices.Provider.Services
{
    using System.Linq;
    using System.Threading.Tasks;
    using MediatR;
    using SFA.DAS.NLog.Logger;
    using Sfa.Das.Sas.Indexer.ApplicationServices.Provider.Models.Fsc;
    using Sfa.Das.Sas.Indexer.ApplicationServices.Shared.MetaData;
    using Sfa.Das.Sas.Indexer.ApplicationServices.Shared.Settings;
    using Sfa.Das.Sas.Indexer.ApplicationServices.Shared.Utility;
    using Sfa.Das.Sas.Indexer.Core.Provider.Models;

    public class FcsActiveProvidersClient : IAsyncRequestHandler<FcsProviderRequest, FcsProviderResult>
    {
        private readonly IAppServiceSettings _appServiceSettings;

        private readonly IConvertFromCsv _convertFromCsv;
        private readonly ILog _logger;

        private readonly IVstsClient _vstsClient;

        public FcsActiveProvidersClient(IVstsClient vstsClient, IAppServiceSettings appServiceSettings, IConvertFromCsv convertFromCsv, ILog logger)
        {
            _vstsClient = vstsClient;
            _appServiceSettings = appServiceSettings;
            _convertFromCsv = convertFromCsv;
            _logger = logger;
        }

        public async Task<FcsProviderResult> Handle(FcsProviderRequest message)
        {
            var loadProvidersFromVsts = await _vstsClient.GetFileContentAsync($"fcs/{_appServiceSettings.EnvironmentName}/fcs-active.csv");
            var records = _convertFromCsv.CsvTo<ActiveProviderCsvRecord>(loadProvidersFromVsts);
            _logger.Debug($"Retrieved {records.Count} providers on the FCS list", new Dictionary<string, object> { { "TotalCount", records.Count } });
            return new FcsProviderResult { Providers = records.Select(x => x.UkPrn) };
        }
    }
}