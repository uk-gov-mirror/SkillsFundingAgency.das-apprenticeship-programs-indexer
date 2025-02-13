﻿namespace Sfa.Das.Sas.Indexer.Infrastructure.Shared.Elasticsearch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ApplicationServices.Provider.Utility;
    using Apprenticeship.Models;
    using AssessmentOrgs.Models;
    using Core.Apprenticeship.Models;
    using Core.Apprenticeship.Models.Standard;
    using Core.AssessmentOrgs.Models;
    using Core.Exceptions;
    using Core.Extensions;
    using Core.Models;
    using Core.Models.Framework;
    using Core.Models.Provider;
    using Infrastructure.Elasticsearch;
    using Lars.Models;
    using Nest;
    using Provider.Models.ElasticSearch;
    using Settings;
    using Sfa.Das.Sas.Indexer.Infrastructure.Settings;
    using Sfa.Das.Sas.Indexer.Infrastructure.Shared.Services;
    using Address = Core.AssessmentOrgs.Models.Address;
    using CoreProvider = Core.Models.Provider.Provider;
    using JobRoleItem = Apprenticeship.Models.JobRoleItem;

    public class ElasticsearchMapper : IElasticsearchMapper
    {
        private readonly IInfrastructureSettings _settings;
        private readonly IOrganisationTypeProcessor _organisationTypeProcessor;

        public ElasticsearchMapper(IInfrastructureSettings settings, IOrganisationTypeProcessor organisationTypeProcessor)
        {
            _settings = settings;
            _organisationTypeProcessor = organisationTypeProcessor;
        }

        public ApprenticeshipDocument CreateStandardDocument(StandardMetaData standard)
        {
            return new ApprenticeshipDocument(ElasticsearchDocumentTypes.STANDARD_DOCUMENT)
            {
                StandardId = standard.Id,
                StandardIdKeyword = standard.Id.ToString(),
                Published = standard.Published,
                Title = standard.Title,
                JobRoles = standard.JobRoles,
                Keywords = standard.Keywords,
                Level = standard.NotionalEndLevel,
                FundingPeriods = standard.FundingPeriods,
                FundingCap = standard.FundingCap,
                Duration = standard.Duration,
                TypicalLength = standard.TypicalLength,
                OverviewOfRole = standard.OverviewOfRole,
                EntryRequirements = standard.EntryRequirements,
                WhatApprenticesWillLearn = standard.WhatApprenticesWillLearn,
                Qualifications = standard.Qualifications,
                ProfessionalRegistration = standard.ProfessionalRegistration,
                StandardSectorCode = standard.SectorCode,
                SectorSubjectAreaTier1 = standard.SectorSubjectAreaTier1,
                SectorSubjectAreaTier2 = standard.SectorSubjectAreaTier2,
                EffectiveFrom = standard.EffectiveFrom,
                EffectiveTo = standard.EffectiveTo,
                LastDateForNewStarts = standard.LastDateForNewStarts,
                RegulatedStandard = standard.RegulatedStandard
            };
        }

        public StandardLars CreateLarsStandardDocument(LarsStandard standard)
        {
            return new StandardLars
            {
                Id = standard.Id,
                Title = standard.Title,
                StandardSectorCode = standard.StandardSectorCode,
                NotionalEndLevel = standard.NotionalEndLevel,
                SectorSubjectAreaTier1 = standard.SectorSubjectAreaTier1,
                SectorSubjectAreaTier2 = standard.SectorSubjectAreaTier2,
                Duration = standard.Duration,
                FundingPeriods = standard.FundingPeriods,
                FundingCap = standard.FundingCap,
                EffectiveFrom = standard.EffectiveFrom,
                EffectiveTo = standard.EffectiveTo,
                LastDateForNewStarts = standard.LastDateForNewStarts,
                RegulatedStandard = standard.RegulatedStandard
            };
        }

        public ApprenticeshipDocument CreateFrameworkDocument(FrameworkMetaData frameworkMetaData)
        {
            // Trim off any whitespaces in the title or the Pathway Name
            frameworkMetaData.NasTitle = frameworkMetaData.NasTitle?.Trim();
            frameworkMetaData.PathwayName = frameworkMetaData.PathwayName?.Trim();

            return new ApprenticeshipDocument(ElasticsearchDocumentTypes.FRAMEWORK_DOCUMENT)
            {
                FrameworkId = string.Format(_settings.FrameworkIdFormat, frameworkMetaData.FworkCode, frameworkMetaData.ProgType, frameworkMetaData.PwayCode),
                Published = frameworkMetaData.Published,
                Title = CreateFrameworkTitle(frameworkMetaData.NasTitle, frameworkMetaData.PathwayName),
                FrameworkCode = frameworkMetaData.FworkCode,
                FrameworkName = frameworkMetaData.NasTitle,
                PathwayCode = frameworkMetaData.PwayCode,
                PathwayName = frameworkMetaData.PathwayName,
                ProgType = frameworkMetaData.ProgType,
                Level = MapToLevelFromProgType(frameworkMetaData.ProgType),
                JobRoleItems = frameworkMetaData.JobRoleItems?.Select(m => new JobRoleItem { Title = m.Title, Description = m.Description }),
                Keywords = frameworkMetaData.Keywords,
                FundingPeriods = frameworkMetaData.FundingPeriods,
                FundingCap = frameworkMetaData.FundingCap,
                Duration = frameworkMetaData.Duration,
                TypicalLength = frameworkMetaData.TypicalLength,
                ExpiryDate = frameworkMetaData.EffectiveTo,
                SectorSubjectAreaTier1 = frameworkMetaData.SectorSubjectAreaTier1,
                SectorSubjectAreaTier2 = frameworkMetaData.SectorSubjectAreaTier2,
                CompletionQualifications = frameworkMetaData.CompletionQualifications,
                EntryRequirements = frameworkMetaData.EntryRequirements,
                ProfessionalRegistration = frameworkMetaData.ProfessionalRegistration,
                FrameworkOverview = frameworkMetaData.FrameworkOverview,
                CompetencyQualification = frameworkMetaData.CompetencyQualification,
                KnowledgeQualification = frameworkMetaData.KnowledgeQualification,
                CombinedQualification = frameworkMetaData.CombinedQualification,
                EffectiveFrom = frameworkMetaData.EffectiveFrom,
                EffectiveTo = frameworkMetaData.EffectiveTo
            };
        }

        public FrameworkLars CreateLarsFrameworkDocument(FrameworkMetaData frameworkMetaData)
        {
            // Trim off any whitespaces in the title or the Pathway Name
            frameworkMetaData.NasTitle = frameworkMetaData.NasTitle?.Trim();
            frameworkMetaData.PathwayName = frameworkMetaData.PathwayName?.Trim();

            return new FrameworkLars
            {
                CombinedQualification = frameworkMetaData.CombinedQualification,
                CompetencyQualification = frameworkMetaData.CompetencyQualification,
                CompletionQualifications = frameworkMetaData.CompletionQualifications,
                EffectiveFrom = frameworkMetaData.EffectiveFrom,
                EffectiveTo = frameworkMetaData.EffectiveTo,
                EntryRequirements = frameworkMetaData.EntryRequirements,
                FrameworkOverview = frameworkMetaData.FrameworkOverview,
                FworkCode = frameworkMetaData.FworkCode,
                JobRoleItems = frameworkMetaData.JobRoleItems,
                Keywords = frameworkMetaData.Keywords,
                KnowledgeQualification = frameworkMetaData.KnowledgeQualification,
                NasTitle = frameworkMetaData.NasTitle,
                PathwayName = frameworkMetaData.PathwayName,
                ProfessionalRegistration = frameworkMetaData.ProfessionalRegistration,
                ProgType = frameworkMetaData.ProgType,
                PwayCode = frameworkMetaData.PwayCode,
                SectorSubjectAreaTier1 = frameworkMetaData.SectorSubjectAreaTier1,
                SectorSubjectAreaTier2 = frameworkMetaData.SectorSubjectAreaTier2,
                Duration = frameworkMetaData.Duration,
                FundingPeriods = frameworkMetaData.FundingPeriods,
                FundingCap = frameworkMetaData.FundingCap
            };
        }

        public FundingDocument CreateFundingMetaDataDocument(FundingMetaData fundingMetaData)
        {
            return new FundingDocument
            {
                EffectiveFrom = fundingMetaData.EffectiveFrom,
                EffectiveTo = fundingMetaData.EffectiveTo,
                FundingCategory = fundingMetaData.FundingCategory,
                LearnAimRef = fundingMetaData.LearnAimRef,
                RateWeighted = fundingMetaData.RateWeighted
            };
        }

        public FrameworkAimDocument CreateFrameworkAimMetaDataDocument(FrameworkAimMetaData frameworkAimMetaData)
        {
            return new FrameworkAimDocument
            {
                EffectiveFrom = frameworkAimMetaData.EffectiveFrom,
                EffectiveTo = frameworkAimMetaData.EffectiveTo,
                LearnAimRef = frameworkAimMetaData.LearnAimRef,
                FworkCode = frameworkAimMetaData.FworkCode,
                PwayCode = frameworkAimMetaData.PwayCode,
                ProgType = frameworkAimMetaData.ProgType,
                ApprenticeshipComponentType = frameworkAimMetaData.ApprenticeshipComponentType
            };
        }

        public LearningDeliveryDocument CreateLearningDeliveryMetaDataDocument(LearningDeliveryMetaData learningDeliveryMetaData)
        {
            return new LearningDeliveryDocument
            {
                EffectiveFrom = learningDeliveryMetaData.EffectiveFrom,
                EffectiveTo = learningDeliveryMetaData.EffectiveTo,
                LearnAimRef = learningDeliveryMetaData.LearnAimRef,
                LearnAimRefTitle = learningDeliveryMetaData.LearnAimRefTitle,
                LearnAimRefType = learningDeliveryMetaData.LearnAimRefType
            };
        }

        public ApprenticeshipFundingDocument CreateApprenticeshipFundingDocument(ApprenticeshipFundingMetaData apprenticeshipFunding)
        {
            return new ApprenticeshipFundingDocument
            {
                ProgType = apprenticeshipFunding.ProgType,
                ApprenticeshipCode = apprenticeshipFunding.ApprenticeshipCode,
                PwayCode = apprenticeshipFunding.PwayCode,
                ReservedValue1 = apprenticeshipFunding.ReservedValue1,
                ApprenticeshipType = apprenticeshipFunding.ApprenticeshipType,
                MaxEmployerLevyCap = apprenticeshipFunding.MaxEmployerLevyCap
            };
        }

        public ApprenticeshipComponentTypeDocument CreateApprenticeshipComponentTypeMetaDataDocument(ApprenticeshipComponentTypeMetaData apprenticeshipComponentTypeMetaData)
        {
            return new ApprenticeshipComponentTypeDocument
            {
                EffectiveTo = apprenticeshipComponentTypeMetaData.EffectiveTo,
                EffectiveFrom = apprenticeshipComponentTypeMetaData.EffectiveFrom,
                ApprenticeshipComponentType = apprenticeshipComponentTypeMetaData.ApprenticeshipComponentType,
                ApprenticeshipComponentTypeDesc = apprenticeshipComponentTypeMetaData.ApprenticeshipComponentTypeDesc,
                ApprenticeshipComponentTypeDesc2 = apprenticeshipComponentTypeMetaData.ApprenticeshipComponentTypeDesc2
            };
        }

        public AssessmentOrgsDocument CreateOrganisationDocument(Organisation organisation)
        {
            return new AssessmentOrgsDocument(ElasticsearchDocumentTypes.ORG_DOCUMENT)
            {
                EpaOrganisationIdentifier = organisation.EpaOrganisationIdentifier,
                EpaOrganisationIdentifierKeyword = organisation.EpaOrganisationIdentifier,
                OrganisationType = _organisationTypeProcessor.ProcessOrganisationType(organisation.OrganisationType),
                Email = organisation.Email,
                Phone = organisation.Phone,
                Address = new Address
                {
                    Primary = organisation.Address.Primary,
                    Secondary = organisation.Address.Secondary,
                    Street = organisation.Address.Street,
                    Town = organisation.Address.Town,
                    Postcode = organisation.Address.Postcode,
                },
                EpaOrganisation = organisation.EpaOrganisation,
                WebsiteLink = organisation.WebsiteLink,
                Ukprn = organisation.Ukprn
            };
        }

        public AssessmentOrgsDocument CreateStandardOrganisationDocument(StandardOrganisationsData standardOrganisationsData)
        {
            return new AssessmentOrgsDocument(ElasticsearchDocumentTypes.STANDARD_ORG_DOCUMENT)
            {
                EpaOrganisationIdentifier = standardOrganisationsData.EpaOrganisationIdentifier,
                EpaOrganisation = standardOrganisationsData.EpaOrganisation,
                OrganisationType = _organisationTypeProcessor.ProcessOrganisationType(standardOrganisationsData.OrganisationType),
                WebsiteLink = standardOrganisationsData.WebsiteLink,
                StandardCode = standardOrganisationsData.StandardCode,
                EffectiveTo = standardOrganisationsData.EffectiveTo,
                EffectiveFrom = standardOrganisationsData.EffectiveFrom,
                Email = standardOrganisationsData.Email,
                Phone = standardOrganisationsData.Phone,
                Address = new Address
                {
                    Primary = standardOrganisationsData.Address.Primary,
                    Secondary = standardOrganisationsData.Address.Secondary,
                    Street = standardOrganisationsData.Address.Street,
                    Town = standardOrganisationsData.Address.Town,
                    Postcode = standardOrganisationsData.Address.Postcode
                }
            };
        }

        public int MapToLevelFromProgType(int progType)
        {
            return ApprenticeshipLevelMapper.MapToLevel(progType);
        }

        public ProviderDocument CreateStandardProviderDocument(CoreProvider provider, StandardInformation standardInformation, DeliveryInformation deliveryInformation)
        {
            return CreateStandardProviderDocument(provider, standardInformation, new List<DeliveryInformation> { deliveryInformation });
        }

        public ProviderDocument CreateStandardProviderDocument(CoreProvider provider, StandardInformation standardInformation, IEnumerable<DeliveryInformation> deliveryInformation)
        {
            return CreateStandardProviderDocument(provider, standardInformation, deliveryInformation.ToList());
        }

        public ProviderDocument CreateFrameworkProviderDocument(CoreProvider provider, FrameworkInformation frameworkInformation, DeliveryInformation deliveryInformation)
        {
            return CreateFrameworkProviderDocument(provider, frameworkInformation, new List<DeliveryInformation> { deliveryInformation });
        }

        public ProviderDocument CreateProviderApiDocument(CoreProvider provider)
        {
          var providerDocument = new ProviderDocument(ElasticsearchDocumentTypes.PROVIDER_API_DOCUMENT)
          {
                Ukprn = provider.Ukprn,
                IsHigherEducationInstitute = provider.IsHigherEducationInstitute,
                NationalProvider = provider.NationalProvider,
                CurrentlyNotStartingNewApprentices = provider.CurrentlyNotStartingNewApprentices,
                ProviderName = provider.Name,
                Aliases = provider.Aliases,
                Addresses = provider.Addresses,
                IsEmployerProvider = provider.IsEmployerProvider,
                Website = provider.ContactDetails?.Website,
                Phone = provider.ContactDetails?.Phone,
                Email = provider.ContactDetails?.Email,
                EmployerSatisfaction = provider.EmployerSatisfaction,
                LearnerSatisfaction = provider.LearnerSatisfaction,
                MarketingInfo = provider.MarketingInfo,
                IsLevyPayerOnly = provider.IsLevyPayerOnly,
                IsNew = provider.IsNew,
                HasParentCompanyGuarantee = provider.HasParentCompanyGuarantee,
                ProviderFeedback = provider.ProviderFeedback
            };

            return providerDocument;
        }

        public ProviderDocument CreateFrameworkProviderDocument(CoreProvider provider, FrameworkInformation frameworkInformation, IEnumerable<DeliveryInformation> deliveryInformation)
        {
            return CreateFrameworkProviderDocument(provider, frameworkInformation, deliveryInformation.ToList());
        }

        private ProviderDocument CreateStandardProviderDocument(CoreProvider provider, StandardInformation standardInformation, List<DeliveryInformation> deliveryInformation)
        {
            try
            {
                var standardProvider = new ProviderDocument(ElasticsearchDocumentTypes.PROVIDER_STANDARD_DOCUMENT)
                {
                    StandardCode = standardInformation.Code,
	                RegulatedStandard = standardInformation.RegulatedStandard
                };

                PopulateDocumentSharedProperties(standardProvider, provider, standardInformation, deliveryInformation);

                return standardProvider;
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is NullReferenceException)
            {
                throw new MappingException("Unable to map to Standard Provider Document", ex);
            }
        }

        private ProviderDocument CreateFrameworkProviderDocument(CoreProvider provider, FrameworkInformation frameworkInformation, List<DeliveryInformation> deliveryInformation)
        {
            try
            {
                var frameworkProvider = new ProviderDocument(ElasticsearchDocumentTypes.PROVIDER_FRAMEWORK_DOCUMENT)
                {
                    FrameworkCode = frameworkInformation.Code,
                    PathwayCode = frameworkInformation.PathwayCode,
                    FrameworkId = string.Format(_settings.FrameworkIdFormat, frameworkInformation.Code, frameworkInformation.ProgType, frameworkInformation.PathwayCode),
                    Level = MapToLevelFromProgType(frameworkInformation.ProgType)
                };

                PopulateDocumentSharedProperties(frameworkProvider, provider, frameworkInformation, deliveryInformation);

                return frameworkProvider;
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is NullReferenceException)
            {
                throw new MappingException("Unable to map to Framework Provider Document", ex);
            }
        }

        private void PopulateDocumentSharedProperties(
            ProviderDocument documentToPopulate,
            CoreProvider provider,
            IApprenticeshipInformation apprenticeshipInformation,
            List<DeliveryInformation> deliveryLocations)
        {
            var locations = GetTrainingLocations(deliveryLocations);
            var firstLoc = deliveryLocations.FirstOrDefault();

            documentToPopulate.Id = Guid.NewGuid();
            documentToPopulate.Ukprn = provider.Ukprn;
            documentToPopulate.IsHigherEducationInstitute = provider.IsHigherEducationInstitute;
            documentToPopulate.HasNonLevyContract = provider.HasNonLevyContract;
            documentToPopulate.HasParentCompanyGuarantee = provider.HasParentCompanyGuarantee;
            documentToPopulate.CurrentlyNotStartingNewApprentices = provider.CurrentlyNotStartingNewApprentices;
            documentToPopulate.IsNew = provider.IsNew;
            documentToPopulate.ProviderName = provider.Name;
            documentToPopulate.LegalName = provider.LegalName;
            documentToPopulate.NationalProvider = provider.NationalProvider;
            documentToPopulate.ProviderMarketingInfo = EscapeSpecialCharacters(provider.MarketingInfo);
            documentToPopulate.ApprenticeshipMarketingInfo = EscapeSpecialCharacters(apprenticeshipInformation.MarketingInfo);
            documentToPopulate.Phone = apprenticeshipInformation.ContactInformation.Phone;
            documentToPopulate.Email = apprenticeshipInformation.ContactInformation.Email;
            documentToPopulate.ContactUsUrl = apprenticeshipInformation.ContactInformation.Website;
            documentToPopulate.ApprenticeshipInfoUrl = apprenticeshipInformation.InfoUrl;
            documentToPopulate.LearnerSatisfaction = provider.LearnerSatisfaction;
            documentToPopulate.EmployerSatisfaction = provider.EmployerSatisfaction;
            documentToPopulate.DeliveryModes = firstLoc == null ? new List<string>().ToArray() : GenerateListOfDeliveryModes(firstLoc.DeliveryModes);
            documentToPopulate.Website = firstLoc == null ? string.Empty : firstLoc.DeliveryLocation.Contact.Website;
            documentToPopulate.TrainingLocations = locations;
            documentToPopulate.LocationPoints = GetLocationPoints(deliveryLocations);

            documentToPopulate.OverallAchievementRate = GetRoundedValue(apprenticeshipInformation.OverallAchievementRate);
            documentToPopulate.NationalOverallAchievementRate = GetRoundedValue(apprenticeshipInformation.NationalOverallAchievementRate);
            documentToPopulate.OverallCohort = apprenticeshipInformation.OverallCohort;

            documentToPopulate.HasNonLevyContract = provider.HasNonLevyContract;
            documentToPopulate.IsLevyPayerOnly = provider.IsLevyPayerOnly;
            documentToPopulate.ProviderFeedback = provider.ProviderFeedback;
        }

        private static double? GetRoundedValue(double? value)
        {
            if (value != null)
            {
                return Math.Round((double) value);
            }

            return null;
        }

        private IEnumerable<GeoCoordinate> GetLocationPoints(IEnumerable<DeliveryInformation> deliveryLocations)
        {
            var points = new List<GeoCoordinate>();

            foreach (var location in deliveryLocations)
            {
                points.Add(new GeoCoordinate(
                    location.DeliveryLocation.Address?.GeoPoint?.Latitude ?? 0,
                    location.DeliveryLocation.Address?.GeoPoint?.Longitude ?? 0));
            }

            return points;
        }

        private List<TrainingLocation> GetTrainingLocations(IEnumerable<DeliveryInformation> deliveryLocations)
        {
            var locations = new List<TrainingLocation>();
            foreach (var loc in deliveryLocations)
            {
                locations.Add(
                    new TrainingLocation
                    {
                        LocationId = loc.DeliveryLocation.Id,
                        LocationName = loc.DeliveryLocation.Name,
                        Address =
                            new Infrastructure.Provider.Models.ElasticSearch.Address()
                            {
                                Address1 = EscapeSpecialCharacters(loc.DeliveryLocation.Address.Address1),
                                Address2 = EscapeSpecialCharacters(loc.DeliveryLocation.Address.Address2),
                                Town = EscapeSpecialCharacters(loc.DeliveryLocation.Address.Town),
                                County = EscapeSpecialCharacters(loc.DeliveryLocation.Address.County),
                                PostCode = loc.DeliveryLocation.Address.Postcode,
                            },
                        Location =
                            new CircleGeoShape(new GeoCoordinate(
                                        loc.DeliveryLocation.Address?.GeoPoint?.Latitude ?? 0,
                                        loc.DeliveryLocation.Address?.GeoPoint?.Longitude ?? 0),
                                        $"{loc.Radius}mi"),
                        LocationPoint = new GeoCoordinate(
                            loc.DeliveryLocation.Address?.GeoPoint?.Latitude ?? 0,
                            loc.DeliveryLocation.Address?.GeoPoint?.Longitude ?? 0)
                    });
            }

            return locations;
        }

        private static string[] GenerateListOfDeliveryModes(IEnumerable<ModesOfDelivery> deliveryModes)
        {
            return deliveryModes.Select(x => x.GetDescription()).ToArray();
        }

        private static string EscapeSpecialCharacters(string marketingInfo)
        {
            return marketingInfo?.Replace(Environment.NewLine, "\\r\\n").Replace("\n", "\\n").Replace("\"", "\\\"");
        }

        private string CreateFrameworkTitle(string framworkname, string pathwayName)
        {
            return $"{framworkname}: {pathwayName}";
        }
    }
}