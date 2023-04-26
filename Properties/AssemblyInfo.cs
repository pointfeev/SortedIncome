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
[assembly: ModuleCategory("Singleplayer")]
[assembly: ModuleType("Community")]
[assembly: ModuleUrl("https://www.nexusmods.com/mountandblade2bannerlord/mods/3304")]
[assembly: ModuleDependedModule("Bannerlord.Harmony", "v2.2.2")]
[assembly: ModuleDependedModule("Native", "v" + SubModule.MinimumGameVersion)]
[assembly: ModuleDependedModule("SandBoxCore", "v" + SubModule.MinimumGameVersion)]
[assembly: ModuleDependedModule("Sandbox", "v" + SubModule.MinimumGameVersion)]
[assembly: ModuleDependedModule("StoryMode", "v" + SubModule.MinimumGameVersion)]
[assembly: ModuleDependedModule("CustomBattle", "v" + SubModule.MinimumGameVersion, true)]
[assembly: ModuleDependedModule("BirthAndDeath", "v" + SubModule.MinimumGameVersion, true)]
[assembly: ModuleSubModule(SubModule.Id, SubModule.Id + ".dll", SubModule.Id + "." + nameof(SubModule))]