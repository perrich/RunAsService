using FakeItEasy;
using NUnit.Framework;
using Perrich.RunAsService.ExitHook;

namespace Perrich.RunAsService.Tests.ExitHook
{
    [TestFixture]
    class StopHookTest
    {
        private IExitHook _hook;
        private Perrich.RunAsService.XmlConfig.XmlConfig _settings;
        private IRunAsService _service;

        [SetUp]
        public void Init()
        {
            _service = A.Fake<IRunAsService>();
            A.CallTo(() => _service.IsStopped).Returns(false);

            _settings = new Perrich.RunAsService.XmlConfig.XmlConfig();
            _hook = new StopHook();
            Assert.True(_hook.Init(_settings, _service));
        }

        [Test]
        public void ShouldCallServiceStop()
        {
            Assert.True(_hook.Launch());
            A.CallTo(() => _service.Stop()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ShouldNotCallServiceStopIfServiceAlreadyStopped()
        {
            A.CallTo(() => _service.IsStopped).Returns(true);
            Assert.False(_hook.Launch());
            A.CallTo(() => _service.Stop()).MustNotHaveHappened();
        }
    }
}
