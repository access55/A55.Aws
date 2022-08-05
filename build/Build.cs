class BuildProject : NukeBuild
{
    public static int Main() => Execute<BuildProject>();

    [Solution] readonly Solution Solution;
    [GlobalJson] readonly GlobalJson GlobalJson;

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter] readonly string TestResultFile = "test_result.xml";

    Target Setup => _ => _
        .Description("Setup the environment for dev")
        .DependsOn(PreCommit)
        .Triggers(RecreateDb, Report);

    Target Clean => _ => _
        .Description("Clean project directories")
        .Executes(() => RootDirectory
            .GlobFiles("**/bin", "**/obj", "**/TestResults")
            .ForEach(EnsureCleanDirectory));

    Target Restore => _ => _
        .Description("Run dotnet restore in every project")
        .DependsOn(Clean)
        .Executes(() => DotNetRestore(s => s
            .SetProjectFile(Solution)));

    Target Build => _ => _
        .Description("Builds Solution")
        .DependsOn(Restore)
        .Executes(() =>
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoLogo()
                .EnableNoRestore()));

    Target TestUnit => _ => _
        .Description("Run unit tests")
        .DependsOn(Build)
        .Executes(() => Solution
            .GetProjects("*.Tests.Unit")
            .ForEach(project =>
                DotNetTest(s => s
                    .EnableNoBuild()
                    .EnableNoRestore()
                    .SetConfiguration(Configuration)
                    .SetProjectFile(project))
            ));

    Target TestIntegration => _ => _
        .Description("Run integration tests")
        .DependsOn(Build)
        .Executes(() => Solution
            .GetProjects("*.Tests.Integration")
            .ForEach(project =>
                DotNetTest(s => s
                    .EnableNoBuild()
                    .EnableNoRestore()
                    .SetConfiguration(Configuration)
                    .SetProjectFile(project))
            ));

    Target Test => _ => _
        .Description("Run all tests")
        .DependsOn(TestUnit, TestIntegration);

    Target TestCoverage => _ => _
        .Description("Run tests with coverage")
        .DependsOn(Build, Db)
        .Executes(() => DotNetTest(s => s
            .EnableNoBuild()
            .EnableNoRestore()
            .SetConfiguration(Configuration)
            .SetProjectFile(Solution)
            .SetLoggers($"trx;LogFileName={TestResultFile}")
            .SetSettingsFile(RootDirectory / "coverlet.runsettings")
        ));

    Target Lint => _ => _
        .Description("Check for codebase formatting and analysers")
        .DependsOn(Build)
        .Executes(() => DotNet($"format -v normal --no-restore --verify-no-changes \"{Solution.Path}\""));

    Target Format => _ => _
        .Description("Try fix codebase formatting and analysers")
        .DependsOn(Build)
        .Executes(() => DotNet($"format -v normal --no-restore \"{Solution.Path}\""));

    Target PreCommit => _ => _
        .Description("Install dotnet-format PreCommit ")
        .Executes(GitHooks.InstallPreCommit);

    [Parameter("Don't open the coverage report")]
    readonly bool NoBrowse;

    Target Report => _ => _
        .Description("Run tests and generate coverage report")
        .DependsOn(TestCoverage)
        .Triggers(GenerateReport, BrowseReport);


    AbsolutePath CoverageFiles => RootDirectory / "tests" / "**" / "coverage.cobertura.xml";
    AbsolutePath TestReportDirectory => RootDirectory / "TestReport";

    Target GenerateReport => _ => _
        .Description("Generate test coverage report")
        .After(TestCoverage)
        .OnlyWhenDynamic(() => CoverageFiles.GlobFiles().Any())
        .Executes(() =>
            ReportGenerator(r => r
                .LocalTool("reportgenerator")
                .SetReports(CoverageFiles)
                .SetTargetDirectory(TestReportDirectory)
                .SetReportTypes(ReportTypes.Cobertura, ReportTypes.Html, ReportTypes.Clover)));

    Target BrowseReport => _ => _
        .Description("Open coverage report")
        .OnlyWhenStatic(() => !NoBrowse && !DotnetRunningInContainer)
        .After(GenerateReport, GenerateBadges)
        .Unlisted()
        .Executes(() =>
        {
            var path = TestReportDirectory / "index.htm";
            Assert.FileExists(path);
            BrowseHtml(path.DoubleQuoteIfNeeded());
        });

    Target GenerateBadges => _ => _
        .Description("Generate cool badges for readme")
        .After(TestCoverage)
        .Requires(() => CoverageFiles.GlobFiles().Any())
        .Executes(() =>
        {
            var output = RootDirectory / "Badges";
            EnsureCleanDirectory(output);
            Badges.ForCoverage(output, CoverageFiles);
            Badges.ForDotNetVersion(output, GlobalJson);
            Badges.ForTests(output, TestResultFile);
        });

    Target CleanAll => _ => _
        .DependsOn(Clean)
        .Description("Clean directory with git ignored files")
        .Executes(() => Git("clean -Xfd", RootDirectory));

    protected override void OnBuildInitialized() => DotNetToolRestore();
}
