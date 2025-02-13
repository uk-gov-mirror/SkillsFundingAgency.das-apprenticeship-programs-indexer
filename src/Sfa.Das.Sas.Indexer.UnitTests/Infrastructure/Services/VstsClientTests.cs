﻿namespace Sfa.Das.Sas.Indexer.UnitTests.Infrastructure.Services
{
    using Moq;
    using NUnit.Framework;
    using SFA.DAS.NLog.Logger;
    using Sfa.Das.Sas.Indexer.ApplicationServices.Shared.Settings;
    using Sfa.Das.Sas.Indexer.ApplicationServices.Shared.Utility;
    using Sfa.Das.Sas.Indexer.Infrastructure.Services;

    [TestFixture]
    public class VstsClientTests
    {
        [Test]
        public void ShouldDownloadAString()
        {
            var settings = Mock.Of<IAppServiceSettings>(x => x.VstsGitGetFilesUrlFormat == "{0}");
            var mockHttp = new Mock<IHttpGet>();

            var sut = new VstsClient(settings, mockHttp.Object, Mock.Of<IHttpPost>(), Mock.Of<ILog>());

            sut.GetFileContent("some/path");

            mockHttp.Verify(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
        }
    }
}