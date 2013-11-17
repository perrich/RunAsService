using System;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using Perrich.RunAsService.Process;

namespace Perrich.RunAsService.tests
{
    [TestFixture]
    class CommandTest
    {
        private const string Executable = "sample.exe";
        private const string Parameters = "param";
        private const string Name = "my sample";
        private const bool KillChildren = true;

        private IProcessManager _processManager;
        private IProcessWrapper _process;

        private Command _command;

        private void InitStartInternalUpdates()
        {
            A.CallTo(() => _process.IsStarted).Returns(true);
            A.CallTo(() => _process.Id).Returns(10000);
        }

        [SetUp]
        public void Init()
        {
            _process = A.Fake<IProcessWrapper>();
            _processManager = A.Fake<IProcessManager>();
            A.CallTo(() => _processManager.GetProcess(Executable, Parameters)).Returns(_process);
            A.CallTo(() => _process.Start()).Invokes(x => InitStartInternalUpdates()).Returns(true);

            _command = new Command(_processManager, Name, Executable, Parameters, KillChildren);
        }

        [Test]
        public void ShouldStartTheProcess()
        {
            _command.Start();
            A.CallTo(() => _process.Start()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ShouldStartTwiceWithoutStopWillRestartTheProcess()
        {
            _command.Start();
            A.CallTo(() => _process.Start()).MustHaveHappened(Repeated.Exactly.Once);
            _command.Start();
            AssertProcessHasBeenKilled();
            A.CallTo(() => _process.Start()).MustHaveHappened(Repeated.Exactly.Twice);
        }

        [Test]
        public void ShouldForwardExitedEvent()
        {
            bool called = false;
            _command.CommandExited += ((x, y) => called = true);

            _command.Start();
            _process.Exited += Raise.With(EventArgs.Empty).Now;
            
            Assert.True(called);
        }

        [Test]
        public void ShouldStopTheProcessIfCommandAlreadyStarted()
        {
            _command.Start();
            _command.Stop();
            AssertProcessHasBeenKilled();
            A.CallTo(() => _process.Dispose()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ShouldOnlyDisposeTheProcessIfCommandAlreadyStartedButNotTheProcess()
        {
            _command.Start();
            A.CallTo(() => _process.IsStarted).Returns(false);
            _command.Stop();
            AssertProcessHasNotBeenKilled();
            A.CallTo(() => _process.Dispose()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ShouldNotStopTheProcessIfCommandNotStarted()
        {
            _command.Stop();
            AssertProcessHasNotBeenKilled();
            A.CallTo(() => _process.Dispose()).MustNotHaveHappened();
        }

        [Test]
        public void ShouldStopTheProcessIfCommandAlreadyStartedAndDisposeCommand()
        {
            _command.Start();
            _command.Dispose();
            AssertProcessHasBeenKilled();
            A.CallTo(() => _process.Dispose()).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void ShouldToStringContainsInfos()
        {
            var str = _command.ToString();
            str.Should().Contain(Executable);
            str.Should().Contain(Name);
            str.Should().Contain(Parameters);
        }

        private void AssertProcessHasBeenKilled()
        {
            A.CallTo(() => _process.IsStarted).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _processManager.KillProcess(_process, KillChildren)).MustHaveHappened(Repeated.Exactly.Once);
        }

        private void AssertProcessHasNotBeenKilled()
        {
            A.CallTo(() => _processManager.KillProcess(_process, KillChildren)).MustNotHaveHappened();
        }

        [Test]
        [ExpectedException("System.ObjectDisposedException")]
        public void ShouldNotStartIfDisposed()
        {
            _command.Dispose();
            _command.Start();
        }

        [Test]
        [ExpectedException("System.ObjectDisposedException")]
        public void ShouldNotStopIfDisposed()
        {
            _command.Dispose();
            _command.Stop();
        }

        [Test]
        public void ShouldNotStopAndCheckProcessIfCannotBeStarted()
        {
            A.CallTo(() => _process.Start()).Throws(new Exception("failed !"));
            _command.Start();
            // Process should be removed because it is not started
            A.CallTo(() => _process.Dispose()).MustHaveHappened(Repeated.Exactly.Once); 
            _command.Stop();
            A.CallTo(() => _process.IsStarted).MustNotHaveHappened();
            A.CallTo(() => _process.Dispose()).MustHaveHappened(Repeated.Exactly.Once); 
        }
    }
}
