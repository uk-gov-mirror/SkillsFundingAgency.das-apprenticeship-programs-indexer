﻿using System.Configuration;
using Microsoft.Azure;
using Sfa.Das.Sas.Indexer.ApplicationServices.Provider.Services;
using Sfa.Das.Sas.Indexer.ApplicationServices.Shared.Settings;

namespace Sfa.Das.Sas.Indexer.ApplicationServices.Provider.Settings
{
    public class ProviderIndexSettings : IIndexSettings<IMaintainProviderIndex>
    {
        public string IndexesAlias => ConfigurationManager.AppSettings["ProviderIndexAlias"];
    }
}