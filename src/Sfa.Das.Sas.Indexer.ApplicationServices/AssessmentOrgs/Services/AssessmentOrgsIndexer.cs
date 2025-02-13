using Sfa.Das.Sas.Indexer.Core.Shared.Models;

﻿namespace Sfa.Das.Sas.Indexer.ApplicationServices.AssessmentOrgs.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SFA.DAS.NLog.Logger;
    using Sfa.Das.Sas.Indexer.ApplicationServices.Apprenticeship.Services;
    using Sfa.Das.Sas.Indexer.ApplicationServices.Shared;
    using Sfa.Das.Sas.Indexer.ApplicationServices.Shared.Settings;
    using Sfa.Das.Sas.Indexer.Core.AssessmentOrgs.Models;
    using Sfa.Das.Sas.Indexer.Core.Services;

    public sealed class AssessmentOrgsIndexer : IGenericIndexerHelper<IMaintainAssessmentOrgsIndex>
    {
        private readonly IIndexSettings<IMaintainAssessmentOrgsIndex> _settings;
        private readonly IMaintainAssessmentOrgsIndex _assessmentOrgsIndexMaintainer;
        private readonly IMetaDataHelper _metaDataHelper;
        private readonly ILog _log;

        public AssessmentOrgsIndexer(
            IIndexSettings<IMaintainAssessmentOrgsIndex> settings,
            IMaintainAssessmentOrgsIndex assessmentOrgsIndexMaintainer,
            IMetaDataHelper metaDataHelper,
            ILog log)
        {
            _settings = settings;
            _assessmentOrgsIndexMaintainer = assessmentOrgsIndexMaintainer;
            _metaDataHelper = metaDataHelper;
            _log = log;
        }

        public async Task<IndexerResult> IndexEntries(string indexName)
        {
            _log.Debug("Retrieving Assessment Orgs data");
            var assessmentOrgsData = _metaDataHelper.GetAssessmentOrganisationsData();

            if (assessmentOrgsData == null)
            {
                _log.Warn("Assessment Orgs were null");
                return new IndexerResult
                {
                    IsSuccessful = false,
                    TotalCount = 0
                };
            }

            var totalAmountDocuments = GetTotalAmountDocumentsToBeIndexed(assessmentOrgsData);

            _log.Debug("Indexing Assessment Orgs data into index");
            IndexOrganisations(indexName, assessmentOrgsData.Organisations);
            IndexStandardOrganisationsData(indexName, assessmentOrgsData.StandardOrganisationsData);
            _log.Debug("Completed indexing Assessment Orgs data");

            return new IndexerResult
            {
                IsSuccessful = IsIndexCorrectlyCreated(indexName, totalAmountDocuments),
                TotalCount = totalAmountDocuments
            };
        }

        public bool CreateIndex(string indexName)
        {
            // If it already exists and is empty, let's delete it.
            if (_assessmentOrgsIndexMaintainer.IndexExists(indexName))
            {
                _log.Warn("Index already exists, deleting and creating a new one");

                _assessmentOrgsIndexMaintainer.DeleteIndex(indexName);
            }

            // create index
            _assessmentOrgsIndexMaintainer.CreateIndex(indexName);

            return _assessmentOrgsIndexMaintainer.IndexExists(indexName);
        }

        public bool IsIndexCorrectlyCreated(string indexName, int totalAmountDocuments)
        {
            return _assessmentOrgsIndexMaintainer.IndexIsCompletedAndContainsDocuments(indexName, totalAmountDocuments);
        }

        public void ChangeUnderlyingIndexForAlias(string newIndexName)
        {
            if (!_assessmentOrgsIndexMaintainer.AliasExists(_settings.IndexesAlias))
            {
                _log.Warn("Alias doesn't exists, creating a new one...");

                _assessmentOrgsIndexMaintainer.CreateIndexAlias(_settings.IndexesAlias, newIndexName);
            }

            _assessmentOrgsIndexMaintainer.SwapAliasIndex(_settings.IndexesAlias, newIndexName);
        }

        public bool DeleteOldIndexes(DateTime scheduledRefreshDateTime)
        {
            var today = IndexerHelper.GetIndexNameAndDateExtension(scheduledRefreshDateTime, _settings.IndexesAlias, "yyyy-MM-dd");
            var oneDayAgo = IndexerHelper.GetIndexNameAndDateExtension(scheduledRefreshDateTime.AddDays(-1), _settings.IndexesAlias, "yyyy-MM-dd");

            return _assessmentOrgsIndexMaintainer.DeleteIndexes(x =>
                !(x.StartsWith(today, StringComparison.InvariantCultureIgnoreCase) ||
                  x.StartsWith(oneDayAgo, StringComparison.InvariantCultureIgnoreCase)) &&
                x.StartsWith(_settings.IndexesAlias, StringComparison.InvariantCultureIgnoreCase));
        }

        private int GetTotalAmountDocumentsToBeIndexed(AssessmentOrganisationsDTO assessmentOrgsData)
        {
            return assessmentOrgsData.StandardOrganisationsData.Count + assessmentOrgsData.Organisations.Count;
        }

        private void IndexOrganisations(string indexName, List<Organisation> organisations)
        {
            try
            {
                _log.Debug("Indexing " + organisations.Count + " organisations into Assessment Organisations index");

                _assessmentOrgsIndexMaintainer.IndexOrganisations(indexName, organisations);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error indexing Assessment Organisations");
            }
        }

        private void IndexStandardOrganisationsData(string indexName, List<StandardOrganisationsData> standardOrganisationsData)
        {
            try
            {
                _log.Debug("Indexing " + standardOrganisationsData.Count + " standard organisations data into Assessment Organisations index");

                _assessmentOrgsIndexMaintainer.IndexStandardOrganisationsData(indexName, standardOrganisationsData);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error indexing Standard Organisations data");
            }
        }
    }
}