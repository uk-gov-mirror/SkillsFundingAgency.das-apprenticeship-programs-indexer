﻿using System;
using System.Collections.Generic;

namespace Sfa.Das.Sas.Indexer.Infrastructure.Settings
{
    public interface IInfrastructureSettings
    {
        string ElasticsearchUsername { get; }

        string ElasticsearchPassword { get; }

        string CourseDirectoryUri { get; }

        string CourseDirectoryApiKey { get; }

        string EnvironmentName { get; }

        IEnumerable<Uri> ElasticServerUrls { get; }

        string ApplicationName { get; }

        string AchievementRateDataBaseConnectionString { get; }

        string FrameworkIdFormat { get; }

        string UkrlpStakeholderId { get; }

        string UkrlpProviderStatus { get; }

        string UkrlpServiceEndpointUrl { get; }

        double HttpClientTimeout { get; }

        string EstablishmentUsername { get; }

        string EstablishmentPassword { get; }

        string EmployerSatisfactionRatesTableName { get; }

        string LearnerSatisfactionRatesTableName { get; }

        bool UseStoredProc { get; }
    }
}