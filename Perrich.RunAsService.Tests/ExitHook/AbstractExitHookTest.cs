using FakeItEasy;
using NUnit.Framework;
using Perrich.RunAsService.ExitHook;

namespace Perrich.RunAsService.Tests.ExitHook
{
    [TestFixture]
    class AbstractExitHookTest
    {
        private IExitHook _hook;
        private Perrich.RunAsService.XmlConfig.XmlConfig _settings;
        private IRunAsService _service;

        private class EmptyHook : AbstractExitHook
        {
            protected override bool Execute()
            {
                return true;
            }

            protected override bool Configure(Perrich.RunAsService.XmlConfig.XmlConfig settings)
            {
                return true;
            }
        }

        [SetUp]
        public void Init()
        {
            _service = A.Fake<IRunAsService>();
            A.CallTo(() => _service.IsStopped).Returns(false);

            _settings = new Perrich.RunAsService.XmlConfig.XmlConfig();
            _hook = new EmptyHook();
        }

        [Test]
        public void ShouldBeInitializedIfParametersIsNotNull()
        {
            Assert.True(_hook.Init(_settings, _service));
        }

        [Test]
        [ExpectedException("System.ArgumentNullException")]
        public void ShoulRaiseExceptionIfSettingIsNull()
        {
            Assert.True(_hook.Init(null, _service));
        }

        [Test]
        [ExpectedException("System.ArgumentNullException")]
        public void ShoulRaiseExceptionIfServiceIsNull()
        {
            Assert.True(_hook.Init(_settings, null));
        }
    }
}
