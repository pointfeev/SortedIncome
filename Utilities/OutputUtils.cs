using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace SortedIncome
{
    public static class OutputUtils
    {
        private static readonly List<string> outputs = new List<string>();
        public static void DoOutputForException(Exception e)
        {
            string[] messageLines = e.Message.Split('\n');
            string message = string.Empty;
            for (int i = 0; i <= messageLines.Length; i++)
            {
                string line = messageLines.ElementAtOrValue(i, null);
                if (!(line is null))
                {
                    message += "\n    " + messageLines[i];
                }
            }
            string[] stackTrace = e.StackTrace.Split('\n');
            string location = string.Empty;
            for (int i = 0; i <= 3; i++)
            {
                string line = stackTrace.ElementAtOrValue(i, null);
                if (!(line is null) && !line.Contains("ThrowHelper"))
                {
                    location = line.Substring(line.IndexOf("at"));
                    break;
                }
            }
            string output = "SortedIncome encountered an exception " + location + message;
            if (!outputs.Contains(output))
            {
                outputs.Add(output);
                InformationManager.DisplayMessage(new InformationMessage(output, Colors.Red, "SortedIncome"));
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
