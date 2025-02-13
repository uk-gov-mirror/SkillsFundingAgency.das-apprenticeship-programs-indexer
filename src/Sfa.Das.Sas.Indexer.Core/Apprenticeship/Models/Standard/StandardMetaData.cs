﻿using System;
using System.Collections.Generic;
using Sfa.Das.Sas.Indexer.Core.Apprenticeship.Models;

namespace Sfa.Das.Sas.Indexer.Core.Models
{
    public class StandardMetaData : IIndexEntry
    {
        public int Id { get; set; }

        public bool Published { get; set; }

        public string Title { get; set; }

        public IEnumerable<string> JobRoles { get; set; }

        public IEnumerable<string> Keywords { get; set; }

        public int NotionalEndLevel { get; set; }

        public int FundingCap { get; set; }

        public int Duration { get; set; }

        public TypicalLength TypicalLength { get; set; }

        public string EntryRequirements { get; set; }

        public string WhatApprenticesWillLearn { get; set; }

        public string Qualifications { get; set; }

        public string ProfessionalRegistration { get; set; }

        public string OverviewOfRole { get; set; }

        public double SectorSubjectAreaTier1 { get; set; }

        public double SectorSubjectAreaTier2 { get; set; }

        public DateTime? EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public DateTime? LastDateForNewStarts { get; set; }

        public int SectorCode { get; set; }

        public List<FundingPeriod> FundingPeriods { get; set; }

        public bool RegulatedStandard { get; set; }
    }
}