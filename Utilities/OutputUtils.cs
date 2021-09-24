using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SortedIncome
{
    public static class OutputUtils
    {
        private static readonly List<string> outputs = new List<string>();

        public static void DoOutputForException(Exception e)
        {
            string[] stackTrace = e.StackTrace.Split('\n');
            string location = "STACK TRACE\n";
            for (int i = 0; i <= 3; i++)
            {
                string line = stackTrace.ElementAtOrValue(i, null);
                if (!(line is null))
                {
                    location += "\n    " + line.Substring(line.IndexOf("at"));
                    break;
                }
            }
            string[] messageLines = e.Message.Split('\n');
            string message = "MESSAGE\n";
            for (int i = 0; i <= messageLines.Length; i++)
            {
                string line = messageLines.ElementAtOrValue(i, null);
                if (!(line is null))
                {
                    message += "\n    " + messageLines[i];
                }
            }
            string output = location + "\n\n" + message +
                "\n\nBUG REPORTING: The easiest way to report this error is to snap an image of this message box with Snipping Tool or Lightshot," +
                "upload the image to imgur.com, and paste the link to the image in a new bug report on Nexus Mods (along with any helpful details)." +
                "\n\nNOTE: This is not a game crash; press OK to continue playing.";
            if (!outputs.Contains(output))
            {
                outputs.Add(output);
                MessageBox.Show(output, caption: "Sorted Income encountered an exception", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
                //InformationManager.DisplayMessage(new InformationMessage(output, Colors.Red, "SortedIncome"));
            }
        }

        public static TextObject GetString(this DefaultClanFinanceModel instance, string str)
        {
            return (TextObject)typeof(DefaultClanFinanceModel).GetProperty(str)?.GetValue(instance);
        }

        public static TextObject AsTextObject(this string str)
        {
            return new TextObject("{=!}" + str);
        }
    }
}