﻿namespace Sfa.Das.Sas.Indexer.UnitTests.ApplicationServices.Provider
{
    using System.Linq;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;
    using SFA.DAS.NLog.Logger;
    using Sfa.Das.Sas.Indexer.ApplicationServices.Provider.Models.Fsc;
    using Sfa.Das.Sas.Indexer.ApplicationServices.Provider.Services;
    using Sfa.Das.Sas.Indexer.ApplicationServices.Shared.MetaData;
    using Sfa.Das.Sas.Indexer.ApplicationServices.Shared.Settings;
    using Sfa.Das.Sas.Indexer.ApplicationServices.Shared.Utility;
    using Sfa.Das.Sas.Indexer.Core.Services;

    [TestFixture]
    public class FcsActiveProvidersClientTest
    {
        [Test]
        public void ShouldGetFcsActiveProviders()
        {
            var moqVstsClient = new Mock<IVstsClient>();
            var moqIProvideSettings = new Mock<IProvideSettings>();
            var appsettings = new AppServiceSettings(moqIProvideSettings.Object);
            var moqIConvertFromCsv = new Mock<IConvertFromCsv>();

            moqVstsClient.Setup(m => m.GetFileContent(It.IsAny<string>())).Returns(string.Empty);
            moqIConvertFromCsv.Setup(m => m.CsvTo<ActiveProviderCsvRecord>(It.IsAny<string>())).Returns(new[] { new ActiveProviderCsvRecord { UkPrn = 26 }, new ActiveProviderCsvRecord { UkPrn = 126 } });

            var client = new FcsActiveProvidersClient(moqVstsClient.Object, appsettings, moqIConvertFromCsv.Object, Mock.Of<ILog>());
            var result = client.Handle(null);

            result.Result.Providers.Count().Should().Be(2);
            result.Result.Providers.All(m => new[] { 26, 126 }.Contains(m)).Should().BeTrue();
        }
    }
}