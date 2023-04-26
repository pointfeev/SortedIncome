using SortedIncome.Properties;

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using TaleWorlds.ModuleManager;

namespace SortedIncome.Utilities;

internal enum OutputType { Initialization, Exception, FinalizerException }

internal static class OutputUtils
{
    private static readonly HashSet<string> Outputs = new();

    internal static void DoOutput(StringBuilder output, OutputType outputType = OutputType.Exception)
    {
        output = output.AppendLine().AppendLine().Append("Module version: " + ModuleHelper.GetModuleInfo(AssemblyInfo.Id).Version);
        output = output.AppendLine().Append("Game version: " + ModuleHelper.GetModuleInfo("Native").Version);
        switch (outputType)
        {
            case OutputType.Initialization:
                break;
            case OutputType.Exception:
                output = output.AppendLine().AppendLine()
                   .Append("BUG REPORTING: The easiest way to report this error is to snap an image of this message box with Snipping Tool or Lightshot, ")
                   .Append(
                        "upload the image to imgur.com, and paste the link to the image in a new bug report on Nexus Mods (along with any helpful details).");
                break;
            case OutputType.FinalizerException:
                output = output.AppendLine().AppendLine()
                   .Append("BUG REPORTING: This exception was caught from a finalizer, which likely means it was not caused by " + AssemblyInfo.Name + " itself, ")
                   .Append(" but more likely caused instead by either a different mod, a bad mod interaction, or the game itself.");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(outputType), outputType, "Bad output type");
        }
        output = output.AppendLine().AppendLine().Append("NOTE: This is not a game crash; press OK to continue playing.");
        string builtOutput = output.ToString();
        if (!Outputs.Add(builtOutput))
            return;
        _ = MessageBox.Show(builtOutput,
            AssemblyInfo.Name + outputType switch
            {
                OutputType.Initialization => " failed to initialize", OutputType.Exception => " encountered an exception",
                OutputType.FinalizerException => " caught an exception from a finalizer",
                _ => throw new ArgumentOutOfRangeException(nameof(outputType), outputType, "Bad output type")
            }, MessageBoxButtons.OK, outputType is OutputType.Exception ? MessageBoxIcon.Error : MessageBoxIcon.Warning);
    }

    private static StringBuilder GetOutputForException(Exception e)
    {
        StringBuilder output = new();
        int stackDepth = 0;
        while (e is not null)
        {
            if (stackDepth > 10)
                break;
            if (output.Length > 0)
                _ = output.Append("\n\n");
            string[] stackTrace = e.StackTrace?.Split('\n');
            if (stackTrace is not null && stackTrace.Length > 0)
            {
                _ = output.Append(e.GetType() + (e.Message is not null ? ": " + e.Message : ""));
                foreach (string line in stackTrace)
                {
                    int atNum = line.IndexOf("at ", StringComparison.Ordinal);
                    int inNum = line.IndexOf("in ", StringComparison.Ordinal);
                    int siNum = line.LastIndexOf(AssemblyInfo.Id + @"\", StringComparison.Ordinal);
                    int lineNum = line.LastIndexOf(":line ", StringComparison.Ordinal);
                    if (atNum != -1)
                        _ = output.Append("\n    " + (inNum != -1 ? line.Substring(atNum, inNum - atNum) : line.Substring(atNum)) + (inNum != -1
                            ? "\n        " + (siNum != -1
                                ? "in " + (lineNum != -1
                                    ? line.Substring(siNum, lineNum - siNum) + "\n            on " + line.Substring(lineNum + 1)
                                    : line.Substring(siNum))
                                : line.Substring(inNum))
                            : null));
                }
            }
            e = e.InnerException;
            stackDepth++;
        }
        return output;
    }

    internal static void DoOutputForException(Exception e) => DoOutput(GetOutputForException(e));

    internal static void DoOutputForFinalizer(Exception e) => DoOutput(GetOutputForException(e), OutputType.FinalizerException);
}