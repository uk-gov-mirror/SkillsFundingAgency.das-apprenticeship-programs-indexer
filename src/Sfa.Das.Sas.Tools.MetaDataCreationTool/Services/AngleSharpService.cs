﻿using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Parser.Html;
using SFA.DAS.NLog.Logger;
using Sfa.Das.Sas.Indexer.ApplicationServices.Shared.Logging.Metrics;
using Sfa.Das.Sas.Indexer.ApplicationServices.Shared.Utility;
using Sfa.Das.Sas.Tools.MetaDataCreationTool.Services.Interfaces;

namespace Sfa.Das.Sas.Tools.MetaDataCreationTool.Services
{
    public class AngleSharpService : IAngleSharpService
    {
        private readonly IHttpGet _httpGet;

        private readonly ILog _logger;

        public AngleSharpService(IHttpGet httpGet, ILog logger)
        {
            _httpGet = httpGet;
            _logger = logger;
        }

        public IList<string> GetLinks(string fromUrl, string selector, string textInTitle)
        {
            if (string.IsNullOrEmpty(fromUrl))
            {
                return new List<string>();
            }

            try
            {
                var timing = ExecutionTimer.GetTiming(() => _httpGet.Get(fromUrl, null, null));
                _logger.Debug("Downloaded standard page", new Dictionary<string, object> { { "Url", fromUrl } });
                var parser = new HtmlParser();
                var result = parser.Parse(timing.Result);
                var all = result.QuerySelectorAll(selector);

                return all.Where(x => x.TextContent.Contains(textInTitle)).Select(x => x.GetAttribute("href")).ToList();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "AngleSharp");
                return null;
            }
        }
    }
}