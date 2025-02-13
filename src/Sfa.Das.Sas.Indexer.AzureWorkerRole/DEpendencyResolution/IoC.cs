﻿// --------------------------------------------------------------------------------------------------------------------
// <auto-generated>
// <copyright file="IoC.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Sfa.Das.Sas.Indexer.ApplicationServices.DependencyResolution;
using Sfa.Das.Sas.Indexer.ApplicationServices.Shared.DependencyResolution;
using Sfa.Das.Sas.Indexer.Infrastructure.AssessmentOrgs.DependencyResolution;
using Sfa.Das.Sas.Indexer.Infrastructure.DependencyResolution;
using Sfa.Das.Sas.Indexer.Infrastructure.Lars.DependencyResolution;
using Sfa.Das.Sas.Indexer.Infrastructure.Provider.DependencyResolution;
using Sfa.Das.Sas.Indexer.Infrastructure.Shared.DependencyResolution;
using Sfa.Das.Sas.Tools.MetaDataCreationTool.DependencyResolution;
using StructureMap;

namespace Sfa.Das.Sas.Indexer.AzureWorkerRole.DependencyResolution
{
    public static class IoC
    {
        public static IContainer Initialize()
        {
            return new Container(c =>
            {
                // Startup
                c.AddRegistry<MediatrRegistry>();
                c.AddRegistry<IndexerRegistry>();
                c.AddRegistry<SharedApplicationServicesRegistry>();
                c.AddRegistry<InfrastructureRegistry>();
                c.AddRegistry<MetaDataCreationRegistry>();

                // ApprenticeshipIndexer
                c.AddRegistry<ApprenticeshipInfrastructureRegistry>();
                c.AddRegistry<ApprenticeshipApplicationServicesRegistry>();

                // Provider
                c.AddRegistry<ProviderApplicationServicesRegistry>();
                c.AddRegistry<ProviderInfrastructureRegistry>();
                
                // Lars
                c.AddRegistry<LarsApplicationServicesRegistry>();
                c.AddRegistry<LarsInfrastructureRegistry>();

                // Assessment Orgs
                c.AddRegistry<AssessmentOrgsApplicationServicesRegistry>();
                c.AddRegistry<AssessmentOrgsInfrastructureRegistry>();
            });
        }
    }
}
