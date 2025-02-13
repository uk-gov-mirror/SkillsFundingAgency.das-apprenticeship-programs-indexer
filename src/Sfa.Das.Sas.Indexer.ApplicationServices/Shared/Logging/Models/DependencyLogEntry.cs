﻿using SFA.DAS.NLog.Logger;

namespace Sfa.Das.Sas.Indexer.ApplicationServices.Shared.Logging.Models
{
    public class DependencyLogEntry : ILogEntry
    {
        public string Name => "Dependency";

        public string Identifier { get; set; }

        public int ResponseCode { get; set; }

        public double ResponseTime { get; set; }

        public string Url { get; set; }
    }
}
