using System;
using System.Collections.Generic;
using System.Windows.Forms;

using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SortedIncome
{
    internal static class OutputUtils
    {
        private static readonly List<string> outputs = new List<string>();

        internal static void DoOutputForException(Exception e)
        {
            string[] stackTrace = e.StackTrace?.Split('\n');
            string location = "STACK TRACE\n";
            if (stackTrace is null || stackTrace.Length == 0)
            {
                location = string.Empty;
            }
            else
            {
                for (int i = 0; i <= 5; i++)
                {
                    string line = stackTrace.ElementAtOrValue(i, null);
                    if (!(line is null))
                    {
                        location += "\n    " + line.Substring(line.IndexOf("at"));
                    }
                }
            }
            string[] messageLines = e.Message?.Split('\n');
            string message = "MESSAGE\n";
            if (messageLines is null || messageLines.Length == 0)
            {
                message = string.Empty;
            }
            else
            {
                for (int i = 0; i <= messageLines.Length; i++)
                {
                    string line = messageLines.ElementAtOrValue(i, null);
                    if (!(line is null))
                    {
                        message += "\n    " + messageLines[i];
                    }
                }
            }
            string output = location + (location.Length == 0 ? string.Empty : "\n\n") + message + (message.Length == 0 ? string.Empty : "\n\n") +
                "BUG REPORTING: The easiest way to report this error is to snap an image of this message box with Snipping Tool or Lightshot, " +
                "upload the image to imgur.com, and paste the link to the image in a new bug report on Nexus Mods (along with any helpful details)." +
                "\n\nNOTE: This is not a game crash; press OK to continue playing.";
            if (!outputs.Contains(output))
            {
                outputs.Add(output);
                MessageBox.Show(output, caption: "Sorted Income encountered an exception", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
            }
        }

        internal static TextObject GetString(this DefaultClanFinanceModel instance, string str) => (TextObject)typeof(DefaultClanFinanceModel).GetProperty(str)?.GetValue(instance);

        internal static TextObject AsTextObject(this string str) => new TextObject("{=!}" + str);
    }
}