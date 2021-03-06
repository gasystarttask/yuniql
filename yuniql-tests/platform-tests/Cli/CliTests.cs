using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Diagnostics;
using System.IO;

namespace Yuniql.PlatformTests
{
    [TestClass]
    public class CliTests : TestBase
    {
        private TestConfiguration _testConfiguration;
        private CliExecutionService _executionService;

        public void SetupWithEmptyWorkspace()
        {
            _testConfiguration = base.ConfigureWithEmptyWorkspace();
            _executionService = new CliExecutionService(_testConfiguration.CliProcessPath);
        }

        public void SetupWorkspaceWithSampleDb()
        {
            _testConfiguration = base.ConfigureWorkspaceWithSampleDb();
            _executionService = new CliExecutionService(_testConfiguration.CliProcessPath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testConfiguration.WorkspacePath))
                Directory.Delete(_testConfiguration.WorkspacePath, true);
        }

        [DataTestMethod]
        [DataRow("init", "")]
        [DataRow("init", "-d")]
        public void Test_Cli_init(string command, string arguments)
        {
            //arrange
            SetupWithEmptyWorkspace();

            //act & assert
            var result = _executionService.Run(command, _testConfiguration.WorkspacePath, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("vnext", "")]
        [DataRow("vnext", "-d")]
        [DataRow("vnext", "-d -m")]
        [DataRow("vnext", "-d -m -f test-vminor-script.sql")]
        [DataRow("vnext", "-d --minor -f test-vminor-script.sql")]
        [DataRow("vnext", "-d --minor --file test-vminor-script.sql")]
        [DataRow("vnext", "-d -M")]
        [DataRow("vnext", "-d --Major")]
        [DataRow("vnext", "-d --Major -file test-vmajor-script.sql")]
        public void Test_Cli_vnext(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run(command, _testConfiguration.WorkspacePath, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("run", "-a -d -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"")]
        [DataRow("run", "-a -d --command-timeout 10 -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"")]
        [DataRow("run", "-a -d -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"")]
        [DataRow("run", "--autocreate-db -d -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"")]
        [DataRow("run", "-a -d -t v1.00 -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"")]
        [DataRow("run", "-a -d --target-version v1.00 -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"")]
        [DataRow("run", "-a -d -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"")]
        [DataRow("run", "-a -d -k \"VwColumnPrefix1=Vw1\" -k \"VwColumnPrefix2=Vw2\" -k \"VwColumnPrefix3=Vw3\" -k \"VwColumnPrefix4=Vw4\"")]
        [DataRow("run", "-a -d --delimiter \",\" -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"")]
        [DataRow("run", "-a -d --schema \"my_schema\"")]
        [DataRow("run", "-a -d --table \"my_versions\" ")]
        [DataRow("run", "-a -d --schema \"my_schema\" --table \"my_versions\" ")]
        public void Test_Cli_run(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("verify", "-d -t v1.00")]
        [DataRow("verify", "-d --target-version v1.00")]
        [DataRow("verify", "-d --delimiter ,")]
        [DataRow("verify", "-d --command-timeout 10")]
        [DataRow("verify", "-d --environment DEV")]
        [DataRow("verify", "-d -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"")]
        public void Test_Cli_verify(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "-a -t v0.00");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("verify", "-d -t v1.00")]
        [DataRow("verify", "-d --target-version v1.00")]
        [DataRow("verify", "-d --delimiter ,")]
        [DataRow("verify", "-d --command-timeout 10")]
        [DataRow("verify", "-d --environment DEV")]
        [DataRow("verify", "-d -k \"VwColumnPrefix1=Vw1,VwColumnPrefix2=Vw2,VwColumnPrefix3=Vw3,VwColumnPrefix4=Vw4\"")]
        public void Test_Cli_verify_With_Custom_Schema(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "-a -t v0.00 --schema \"my_schema\" --table \"my_versions\" ");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "--schema \"my_schema\" --table \"my_versions\" " +arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("info", "")]
        [DataRow("info", "-d")]
        [DataRow("info", "-d --command-timeout 10")]
        public void Test_Cli_info(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "-a -d");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }

        [DataTestMethod]
        [DataRow("info", "")]
        [DataRow("info", "-d")]
        [DataRow("info", "-d --command-timeout 10")]
        public void Test_Cli_info_With_Custom_Schema(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "-a --schema \"my_schema\" --table \"my_versions\" -d");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "--schema \"my_schema\" --table \"my_versions\" " + arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }


        [DataTestMethod]
        [DataRow("erase", "")]
        [DataRow("erase", "-d")]
        [DataRow("erase", "-d --force")]
        [DataRow("erase", "-d --environment DEV")]
        [DataRow("erase", "-d --command-timeout 10")]
        public void Test_Cli_erase(string command, string arguments)
        {
            //arrange
            SetupWorkspaceWithSampleDb();

            //act & assert
            var result = _executionService.Run("run", _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, "-a -d");
            result.Contains($"Failed to execute run").ShouldBeFalse();

            //act & assert
            result = _executionService.Run(command, _testConfiguration.WorkspacePath, _testConfiguration.ConnectionString, _testConfiguration.Platform, arguments);
            result.Contains($"Failed to execute {command}").ShouldBeFalse();
        }
    }
}
