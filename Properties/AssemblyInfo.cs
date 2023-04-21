using System.Reflection;
using System.Runtime.InteropServices;
using Bannerlord.AutomaticSubModuleXML;
using SortedIncome;

[assembly: ComVisible(false)]
[assembly: AssemblyProduct(SubModule.Name)]
[assembly: AssemblyTitle(SubModule.Id)]
[assembly: AssemblyCopyright("2021, pointfeev (https://github.com/pointfeev)")]
[assembly: AssemblyFileVersion(SubModule.Version)]
[assembly: AssemblyVersion(SubModule.Version)]
[assembly: ModuleId(SubModule.Id)]
[assembly: ModuleName(SubModule.Name)]
[assembly: ModuleVersion("v" + SubModule.Version)]
[assembly: ModuleDefault(true)]
[assembly: ModuleCategory(ModuleCategoryValue.Singleplayer)]
[assembly: ModuleType(ModuleTypeValue.Community)]
[assembly: ModuleUrl("https://www.nexusmods.com/mountandblade2bannerlord/mods/3320")]
[assembly: ModuleDependency("Bannerlord.Harmony", "v2.2.2")]
[assembly: ModuleDependency("Native", "v" + SubModule.MinimumGameVersion)]
[assembly: ModuleDependency("SandBoxCore", "v" + SubModule.MinimumGameVersion)]
[assembly: ModuleDependency("Sandbox", "v" + SubModule.MinimumGameVersion)]
[assembly: ModuleDependency("StoryMode", "v" + SubModule.MinimumGameVersion)]
[assembly: ModuleDependency("CustomBattle", "v" + SubModule.MinimumGameVersion, true)]
[assembly: ModuleDependency("BirthAndDeath", "v" + SubModule.MinimumGameVersion, true)]
[assembly:
    ModuleSubModule(SubModule.Id, SubModule.Id + ".dll", SubModule.Id + "." + nameof(SubModule),
        new[] { "DedicatedServerType", "none", "IsNoRenderModeElement", "false" })]