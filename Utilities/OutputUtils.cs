using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace SortedIncome.Utilities
{
    internal static class OutputUtils
    {
        private static readonly List<string> Outputs = new List<string>();

        private static void DoOutput(StringBuilder output)
        {
            string outputString = output.AppendLine().AppendLine()
                                        .Append(
                                             "BUG REPORTING: The easiest way to report this error is to snap an image of this message box with Snipping Tool or Lightshot, ")
                                        .Append(
                                             "upload the image to imgur.com, and paste the link to the image in a new bug report on Nexus Mods (along with any helpful details).")
                                        .AppendLine().AppendLine().Append("NOTE: This is not a game crash; press OK to continue playing.").ToString();
            if (Outputs.Contains(outputString))
                return;
            Outputs.Add(outputString);
            _ = MessageBox.Show(outputString, "Aggregated Income encountered an exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        internal static void DoCustomOutput(string output) => DoOutput(new StringBuilder(output));

        internal static void DoOutputForException(Exception e)
        {
            StringBuilder output = new StringBuilder();
            int stackDepth = 0;
            while (!(e is null))
            {
                if (stackDepth > 10)
                    break;
                if (output.Length > 0)
                    _ = output.Append("\n\n");
                string[] stackTrace = e.StackTrace?.Split('\n');
                if (!(stackTrace is null) && stackTrace.Length > 0)
                {
                    _ = output.Append(e.GetType() + (!(e.Message is null) ? ": " + e.Message : ""));
                    foreach (string line in stackTrace)
                    {
                        int atNum = line.IndexOf("at ", StringComparison.Ordinal);
                        int inNum = line.IndexOf("in ", StringComparison.Ordinal);
                        int siNum = line.LastIndexOf(@"SortedIncome\", StringComparison.Ordinal);
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
            DoOutput(output);
        }
    }
}