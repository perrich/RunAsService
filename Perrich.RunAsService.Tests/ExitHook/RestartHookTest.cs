using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Perrich.RunAsService.ExitHook;

namespace Perrich.RunAsService.Tests.ExitHook
{
    [TestFixture]
    class RestartHookTest
    {
        private RestartHook _hook;
        private Perrich.RunAsService.XmlConfig.XmlConfig _settings;
        private IRunAsService _service;

        [SetUp]
        public void Init()
        {
            _service = A.Fake<IRunAsService>();
            A.CallTo(() => _service.IsStopped).Returns(false);

            _settings = new Perrich.RunAsService.XmlConfig.XmlConfig();
            _settings.LoadXmlFromString("<configuration><restart><times value=\"3\" /></restart></configuration>");
            _hook = new RestartHook();
        }

        [Test]
        public void ShouldCallStartCommandIgnoringRemainingTimesIfNotConfiguredAsIs()
        {
            _settings.LoadXmlFromString("<configuration></configuration>");
            Assert.True(_hook.Init(_settings, _service));

            AssertSuccessfullyLaunched(RestartHook.InfiniteTimes, Repeated.Exactly.Once);
        }

        [Test]
        public void ShouldNotBeInitializedIfParametersIsWrong()
        {
            _settings.LoadXmlFromString("<configuration><restart><times value=\"a\" /></restart></configuration>");
            Assert.False(_hook.Init(_settings, _service));
            Assert.False(_hook.Initialized);

            AssertLaunchedWithError(0, Repeated.Never); // 0 as never well initialized
            Assert.False(_hook.Initialized);
        }

        [Test]
        public void ShouldCallStartCommandExactlyTheConfiguredTimes()
        {
            Assert.True(_hook.Init(_settings, _service));
            _hook.RemainingTimes.Should().Be(3);

            AssertSuccessfullyLaunched(2, Repeated.Exactly.Once);
            AssertSuccessfullyLaunched(1, Repeated.Exactly.Twice);
            AssertSuccessfullyLaunched(0, Repeated.Exactly.Times(3));

            // Call Stop() and not StartCommand() as only three times was authorized and service is not stopped
            AssertSuccessfullyLaunched(0, Repeated.Exactly.Times(3));
            A.CallTo(() => _service.Stop()).MustHaveHappened(Repeated.Exactly.Once);

            // Call nothing as only three times was authorized and service is stopped...
            A.CallTo(() => _service.IsStopped).Returns(true);
            AssertLaunchedWithError(0, Repeated.Exactly.Times(3));
            A.CallTo(() => _service.Stop()).MustHaveHappened(Repeated.Exactly.Once);
        }

        private void AssertSuccessfullyLaunched(int remainingTimes, Repeated currentCalls)
        {
            Assert.True(_hook.Launch());
            A.CallTo(() => _service.StartCommand()).MustHaveHappened(currentCalls);
            _hook.RemainingTimes.Should().Be(remainingTimes);
        }

        private void AssertLaunchedWithError(int remainingTimes, Repeated currentCalls)
        {
            Assert.False(_hook.Launch());
            A.CallTo(() => _service.StartCommand()).MustHaveHappened(currentCalls);
            Assert.AreEqual(remainingTimes, _hook.RemainingTimes);
        }
    }
}
