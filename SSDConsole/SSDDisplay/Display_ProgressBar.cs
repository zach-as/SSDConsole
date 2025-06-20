using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDConsole.SSDDisplay
{
    internal static partial class Display
    {
        // Returns true if there is an active progress bar
        internal static bool InProgress()
        {
            return currentProgressBar is not null && !currentProgressBar.IsFinished();
        }

        // this will start a progress bar which will be monitored and displayed on the console
        internal static void StartProgressBar(string message, int? max_progress, params ProgressBarInfo[] info)
        {
            if (InProgress())
            {
                Interrupt("Starting progress bar when one is already underway. Overriding existing progress bar.",
                                MessageSeverity.Warning);
            }
            currentProgressBar = new ProgressBar(message, max_progress, info);
        }
        internal static void StartProgressBar(string message, params ProgressBarInfo[] info)
            => StartProgressBar(message, null, info);

        internal static void StopProgressBar()
        {
            if (currentProgressBar is null)
            {
                Print("StopProgressBar() called, but no progress bar is active.", MessageSeverity.Warning);
                return;
            }
            currentProgressBar.Finish();
        }
        internal static void UpdateProgressBar(int amount, string? id = null)
        {
            if (currentProgressBar is null)
            {
                Print("UpdateProgressBar called, but no progress bar is active.", MessageSeverity.Warning);
                return;
            }
            currentProgressBar.Update(amount, id);
        }
        internal static void UpdateProgressBar(string? id)
            => UpdateProgressBar(1, id);
        internal static void UpdateProgressBar()
            => UpdateProgressBar(1, null);

        internal class ProgressBarInfo
        {
            private string id = string.Empty;
            private string text = string.Empty;
            private int count = 0;
            private int? count_max = null;
            internal ProgressBarInfo(string id, string text, int? count_max = null)
            {
                this.id = id;
                this.text = text;
                this.count_max = count_max;
            }
            internal string Id() => id;
            internal string Text() => text;
            internal string PrintText()
            {
                var t = $"{text}: {count}";
                if (count_max is not null) t += $" / {count_max}";
                t += ".";
                return t;
            }
            internal int Count() => count;
            internal int ModifyCount(int mod) => count += mod;
        }

        private class ProgressBar
        {
            private int progress = 0;
            private int? max_progress = null;
            private string text = string.Empty;
            private ProgressBarInfo[] secondaryInfo = new ProgressBarInfo[] { };
            private bool finished = false;

            internal ProgressBar(string text, int? max_progress, params ProgressBarInfo[] secondaryInfo)
            {
                this.max_progress = max_progress;
                this.text = text;
                this.secondaryInfo = secondaryInfo;
            }

            internal void Update(int new_progress, string? id)
            {
                if (id is null)
                {
                    progress += new_progress;
                    if (max_progress is not null && progress > max_progress)
                    {
                        progress = max_progress ?? progress;
                        finished = true;
                    }
                    return;
                }
                // update if the id is not null
                UpdateSecondary(id, new_progress);
            }

            private void UpdateSecondary(string id, int val)
            {
                var info = GetInfo(id);
                if (info is null)
                {
                    Print("UpdateSecondary({id}) in ProgressBar called, but id not found. Skipping.", MessageSeverity.Warning);
                    return;
                }
                info.ModifyCount(val);
            }
            private ProgressBarInfo? GetInfo(string id)
                => secondaryInfo.FirstOrDefault(info => info.Id() == id);

            internal string PrintText()
            {
                var t = $"{text}: {progress}";
                if (max_progress is not null) t += $" / {max_progress}";
                if (t.Last() != '.') t += "."; // ensure that the text ends with a period
                foreach (var info in secondaryInfo)
                {
                    if (info.Count() > 0)
                    {
                        t += " " + info.PrintText();
                        if (t.Last() != '.') t += "."; // ensure that the text ends with a period
                    }
                }
                return t;
            }
            internal Message Message()
                => new Message(PrintText(), MessageSource.ProgressBar, IsFinished() ? MessageSeverity.Success : MessageSeverity.Info);
            internal bool IsFinished()
                => finished;
            internal void Finish()
                => finished = true;
        }
    }
}
