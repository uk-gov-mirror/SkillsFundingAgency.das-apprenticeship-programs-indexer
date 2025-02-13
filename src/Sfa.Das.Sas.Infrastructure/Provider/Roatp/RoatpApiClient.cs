﻿using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Sfa.Das.Sas.Indexer.Infrastructure.Provider.Roatp
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using SFA.DAS.NLog.Logger;
    using Sfa.Das.Sas.Indexer.ApplicationServices.Provider.Models;
    using Sfa.Das.Sas.Indexer.ApplicationServices.Provider.Services;
    using Sfa.Das.Sas.Indexer.ApplicationServices.Shared.Settings;

    public class RoatpApiClient: IRoatpApiClient
    {
        private const string downloadPath = "api/v1/download/roatp-summary";
        private readonly IAppServiceSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ILog _log;
        private readonly ITokenService _tokenService;

        public RoatpApiClient(IAppServiceSettings settings, ILog log, ITokenService tokenService)
        {
            _settings = settings;
            _log = log;
            _tokenService = tokenService;
            var baseUrl = _settings.RoatpApiClientBaseUrl;
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };

        }

        public async Task<List<RoatpResult>> GetRoatpSummary()
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _tokenService.GetRoatpToken());
                _log.Info("Gathering roatp details from API");
                var response = await _httpClient.GetAsync(downloadPath);
                return await response.Content.ReadAsAsync<List<RoatpResult>>();
            }
            catch (Exception e)
            {
                _log.Error(e, $"Error gathering roatp details from API: [{e.Message}]");
                throw;
            }
        }
    }
}
