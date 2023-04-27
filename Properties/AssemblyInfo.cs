using System.Reflection;
using System.Runtime.InteropServices;
using ASMXML;
using SortedIncome;
using SortedIncome.Properties;

[assembly: ComVisible(false)]
[assembly: AssemblyProduct(AssemblyInfo.Name)]
[assembly: AssemblyTitle(AssemblyInfo.Id)]
[assembly: AssemblyCopyright("2021, pointfeev (https://github.com/pointfeev)")]
[assembly: AssemblyFileVersion(AssemblyInfo.Version)]
[assembly: AssemblyVersion(AssemblyInfo.Version)]
[assembly: ModuleId(AssemblyInfo.Id)]
[assembly: ModuleName(AssemblyInfo.Name)]
[assembly: ModuleVersion("v" + AssemblyInfo.Version)]
[assembly: ModuleDefault(true)]
[assembly: ModuleCategory("Singleplayer")]
[assembly: ModuleType("Community")]
[assembly: ModuleDependedModule("Bannerlord.Harmony", "v" + AssemblyInfo.HarmonyVersion)]
[assembly: ModuleDependedModule("Native", "v" + AssemblyInfo.MinimumGameVersion)]
[assembly: ModuleDependedModule("SandBoxCore", "v" + AssemblyInfo.MinimumGameVersion)]
[assembly: ModuleDependedModule("Sandbox", "v" + AssemblyInfo.MinimumGameVersion)]
[assembly: ModuleDependedModule("StoryMode", "v" + AssemblyInfo.MinimumGameVersion)]
[assembly: ModuleDependedModule("CustomBattle", "v" + AssemblyInfo.MinimumGameVersion, true)]
[assembly: ModuleDependedModule("BirthAndDeath", "v" + AssemblyInfo.MinimumGameVersion, true)]
[assembly: ModuleSubModule(AssemblyInfo.Id, AssemblyInfo.Id + ".dll", AssemblyInfo.Id + "." + nameof(SubModule))]

namespace SortedIncome.Properties;

internal static class AssemblyInfo
{
    internal const string Id = "SortedIncome";
    internal const string Name = "Aggregated Income";
    internal const string Version = "4.2.5";
    internal const string HarmonyVersion = "2.3.0";
    internal const string MinimumGameVersion = "1.0.0";
}