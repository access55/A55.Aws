global using System;
global using System.ComponentModel;
global using System.Linq;
global using Helpers;
global using Nuke.Common;
global using Nuke.Common.Execution;
global using Nuke.Common.IO;
global using Nuke.Common.ProjectModel;
global using Nuke.Common.Tooling;
global using Nuke.Common.Tools.Docker;
global using Nuke.Common.Tools.DotNet;
global using Nuke.Common.Tools.EntityFramework;
global using Nuke.Common.Tools.ReportGenerator;
global using Nuke.Common.Utilities.Collections;
global using Serilog;
global using static Helpers.Commands;
global using static Nuke.Common.EnvironmentInfo;
global using static Nuke.Common.IO.FileSystemTasks;
global using static Nuke.Common.IO.PathConstruction;
global using static Nuke.Common.Tools.Docker.DockerTasks;
global using static Nuke.Common.Tools.DotNet.DotNetTasks;
global using static Nuke.Common.Tools.EntityFramework.EntityFrameworkTasks;
global using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;
global using static Nuke.Common.Utilities.StringExtensions;
using System.Reflection;
using JetBrains.Annotations;
using static Nuke.Common.IO.SerializationTasks;

[TypeConverter(typeof(TypeConverter<Configuration>))]
public class Configuration : Enumeration
{
    public static Configuration Debug = new() { Value = nameof(Debug) };
    public static Configuration Release = new() { Value = nameof(Release) };

    public static implicit operator string(Configuration configuration) => configuration.Value;
}

public record Sdk(Version Version, string RollForward);

public record GlobalJson(Sdk Sdk);

[PublicAPI]
[UsedImplicitly(ImplicitUseKindFlags.Assign)]
public class GlobalJsonAttribute : ParameterAttribute
{
    readonly AbsolutePath filePath;
    public override bool List { get; set; } = false;

    public GlobalJsonAttribute()
    {
        filePath = NukeBuild.RootDirectory / "global.json";
        Assert.FileExists(filePath);
    }

    public override object GetValue(MemberInfo member, object instance)
        => JsonDeserializeFromFile<GlobalJson>(filePath);
}
