using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Perrich.RunAsService.ExitHook;

namespace Perrich.RunAsService.tests
{
    [TestFixture]
    class HookRepositoryTest
    {
        private XmlConfig.XmlConfig _settings;
        private IRunAsService _service;

        private HookRepository _repository;

        [SetUp]
        public void Init()
        {
            _service = A.Fake<IRunAsService>();
            _settings = new XmlConfig.XmlConfig();
            _settings.LoadXmlFromString("<configuration><restart><times value=\"2\" /></restart></configuration>");

            _repository = new HookRepository("RestartHook", _settings, _service);
            _repository.Hooks.Should().HaveCount(1);
            _repository.Hooks[0].Should().BeOfType<RestartHook>();
        }

        [Test]
        public void ShouldContainsRequestedHooksInTheSameOrder()
        {
            _repository = new HookRepository("StopHook,RestartHook", _settings, _service);
            _repository.Hooks.Should().HaveCount(2);
            _repository.Hooks[0].Should().BeOfType<StopHook>();
            _repository.Hooks[1].Should().BeOfType<RestartHook>();
        }

        [Test]
        public void ShouldIgnoreUnknownHook()
        {
            _repository = new HookRepository("StopHook,UnknownHook", _settings, _service);
            _repository.Hooks.Should().HaveCount(1);
            _repository.Hooks[0].Should().BeOfType<StopHook>();
        }

        [Test]
        public void ShouldAllowFullyDefinedHook()
        {
            _repository = new HookRepository(typeof(StopHook).FullName, _settings, _service);
            _repository.Hooks.Should().HaveCount(1);
            _repository.Hooks[0].Should().BeOfType<StopHook>();
        }

        [Test]
        public void ShouldResetHookConfiguration()
        {
            var hook = (RestartHook)_repository.Hooks[0];
            hook.RemainingTimes.Should().Be(2);
            hook.Launch();
            hook.RemainingTimes.Should().Be(1);

            _repository.Reset();

            hook = (RestartHook)_repository.Hooks[0];
            hook.RemainingTimes.Should().Be(2);
        }
    }
}
