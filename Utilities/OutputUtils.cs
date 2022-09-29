using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Localization;

namespace SortedIncome
{
    internal static class OutputUtils
    {
        private static readonly List<string> outputs = new List<string>();

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
                    _ = output.Append(e.GetType() + (!(e.Message is null) ? (": " + e.Message) : ""));
                    for (int i = 0; i < stackTrace.Length; i++)
                    {
                        string line = stackTrace[i];
                        int atNum = line.IndexOf("at ");
                        int inNum = line.IndexOf("in ");
                        int siNum = line.LastIndexOf(@"SortedIncome\");
                        int lineNum = line.LastIndexOf(":line ");
                        if (!(line is null) && atNum != -1)
                            _ = output.Append("\n    " + (inNum != -1 ? line.Substring(atNum, inNum - atNum) : line.Substring(atNum))
                                + (inNum != -1 ? ("\n        "
                                    + (siNum != -1 ? ("in "
                                        + (lineNum != -1 ? line.Substring(siNum, lineNum - siNum)
                                            + "\n            on " + line.Substring(lineNum + 1)
                                        : line.Substring(siNum)))
                                    : line.Substring(inNum)))
                                : null));
                    }
                }
                e = e.InnerException;
                stackDepth++;
            }
            string outputString = output.ToString() + "\n\n" +
                "BUG REPORTING: The easiest way to report this error is to snap an image of this message box with Snipping Tool or Lightshot, " +
                "upload the image to imgur.com, and paste the link to the image in a new bug report on Nexus Mods (along with any helpful details)." +
                "\n\nNOTE: This is not a game crash; press OK to continue playing.";
            if (!outputs.Contains(outputString))
            {
                outputs.Add(outputString);
                _ = MessageBox.Show(outputString, caption: "Sorted Income encountered an exception", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
            }
        }

        internal static TextObject GetString(this DefaultClanFinanceModel instance, string str) => (TextObject)typeof(DefaultClanFinanceModel).GetProperty(str)?.GetValue(instance);

        internal static TextObject AsTextObject(this string str) => new TextObject("{=!}" + str);
    }
}