using System;
using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Perrich.RunAsService.ExitHook;

namespace Perrich.RunAsService.tests
{
    [TestFixture]
    internal class RunAsServiceTest
    {
        #region Setup/Teardown

        [SetUp]
        public void Init()
        {
            // provide a good configuration
            _settings = new XmlConfig.XmlConfig();
            _settings.LoadXmlFromString("<configuration><description>" + Description + "</description><displayName>" +
                                        DisplayName + "</displayName><name>" + Name + "</name><executable>" + Executable +
                                        "</executable></configuration>");

            // Set the fake process related to the service
            _command = A.Fake<ICommand>();
            _commandBuilder = A.Fake<CommandBuilder>();
            A.CallTo(() => _commandBuilder.BuildCommand(_settings, Name)).Returns(_command);

            // Make a hook and add it in the repository
            _hook = A.Fake<IExitHook>();
            _repository = A.Fake<HookRepository>();
            A.CallTo(() => _repository.Hooks).Returns(new List<IExitHook> {_hook});

            _service = new RunAsService(_settings, _repository, _commandBuilder);
        }

        #endregion

        private const string Executable = "sample.exe";
        private const string Name = "My name";
        private const string DisplayName = "displayName";
        private const string Description = "description";

        private XmlConfig.XmlConfig _settings;
        private RunAsService _service;

        private CommandBuilder _commandBuilder;
        private ICommand _command;
        private HookRepository _repository;
        private IExitHook _hook;

        [Test]
        public void ShouldHaveSettingsAsProperties()
        {
            _service.ServiceName.Should().Be(Name);
            _service.DisplayName.Should().Be(DisplayName);
            _service.Description.Should().Be(Description);
        }

        [Test]
        public void ShouldStartCommand()
        {
            _service.InternalStart();
            _service.IsStopped.Should().Be(false);
        }

        [Test]
        [ExpectedException("System.InvalidOperationException")]
        public void ShouldStartCommandCannotBeCalledTwice()
        {
            _service.InternalStart();
            _service.InternalStart();
        }

        [Test]
        public void ShouldCommandExitedCallHooks()
        {
            _service.InternalStart();
            _command.CommandExited += Raise.With(EventArgs.Empty).Now;
            A.CallTo(() => _hook.Launch()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ShouldCommandExitedAllowsNewCommandStart()
        {
            _service.InternalStart();
            _command.CommandExited += Raise.With(EventArgs.Empty).Now;
            _service.InternalStart();
        }

        [Test]
        public void ShouldStop()
        {
            _service.InternalStart();
            _service.InternalStop();
            _service.IsStopped.Should().Be(true);
        }

        [Test]
        public void ShouldStopWorksEvenIfNothingIsStarted()
        {
            _service.IsStopped.Should().Be(true);
            _service.InternalStop();
            _service.IsStopped.Should().Be(true);
        }

        [Test]
        public void ShouldDisposeStopAndDispose()
        {
            _service.InternalStart();
            _service.Dispose();
            A.CallTo(() => _command.Dispose()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        [ExpectedException("System.ObjectDisposedException")]
        public void ShouldNotStartIfDisposed()
        {
            _service.Dispose();
            _service.InternalStart();
        }
    }
}