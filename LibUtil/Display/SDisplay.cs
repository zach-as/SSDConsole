using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace LibUtil.UtilDisplay
{
    internal enum MessageSource
    {
        ProgressBar,
        Interrupt,
        Misc
    }

    internal enum Verbosity
    {
        Standard,
        Debug
    }

    public static partial class SDisplay
    {
        
        private const int TIME_BETWEEN_UPDATES = 10; // in milliseconds, aka 10 updates per second

        private static Message? previousMessage; // the most recent message printed

        private static List<Message> messages = new List<Message>(); // messages here are printed AFTER interrupts and progress bar
        private static List<Message> interruptMessages = new List<Message>(); // messages here are printed immediately, before progress bar and other messages

        private static ProgressBar? currentProgressBar; // the current progress bar, if any

        public static async Task BeginDisplay()
        {
            Print(new Message("Initiating display sequence.", MessageSource.Misc));
            await Task.Run(() =>
            {
                do
                {
                    // Handle all of the print logic
                    WriteToConsole();

                    // Avoid overloading the system by sleeping between updates
                    System.Threading.Thread.Sleep(TIME_BETWEEN_UPDATES);

                } while (true);
            });
        }

        // this will print a message to the console with high priority
        public static void Interrupt(string message, MessageSeverity severity)
            => interruptMessages.Add(new Message(message, MessageSource.Interrupt, severity));
        public static void Interrupt(string message)
            => Interrupt(message, MessageSeverity.Info);

        // for public use, this will print a message to the console with low priority
        public static void Print(string message, MessageSeverity severity = MessageSeverity.Info)
            => messages.Add(new Message(message, MessageSource.Misc, severity));
        public static void Success(string message)
            => Print(message, MessageSeverity.Success);
        public static void Warning(string message)
            => Print(message, MessageSeverity.Warning);
        public static void Error(string message)
            => Print(message, MessageSeverity.Error);

        private static void WriteToConsole()
        {
            // First print any interrupt messages if present
            if (interruptMessages.Count > 0)
            {
                Print(interruptMessages[0]);
                interruptMessages.RemoveAt(0);
                return;
            }

            // If there is a progress bar active, print it
            if (currentProgressBar is not null)
            {
                // Print the current progress
                Print(currentProgressBar.Message());

                if (currentProgressBar.IsFinished())
                {
                    currentProgressBar = null; // Reset the progress bar after it is finished
                }

                return;
            }

            // If there is no active progress bar and no interrupts, print any pending messages
            if (messages.Count > 0)
            {
                Print(messages[0]);
                messages.RemoveAt(0);
                return;
            }
        }

        private static void Print(Message message)
        {
            // Clear the previous line if it's a printed progress bar
            if (previousMessage is not null
                && previousMessage.Source() == MessageSource.ProgressBar) ClearLine();
            
            Console.ForegroundColor = message.Color();
            Console.Write(message.Text());

            // Move to the next line if this message is not from a progress bar
            if (message.Source() != MessageSource.ProgressBar) Console.WriteLine();

            // Update the previously used message to be the new message
            previousMessage = message;
        }

        #region debug
        private static Verbosity verbosity = Verbosity.Standard;
        internal static void SetVerbose(bool verbose) // if verbose is set to true, debug statements will print
        {
            if (verbose) verbosity = Verbosity.Debug;
            else verbosity = Verbosity.Standard;
        }
        internal static bool IsVerbose() // returns true if verbosity is set to debug
            => verbosity == Verbosity.Debug ? true : false;
        internal static void Debug(string message) // these statements will only print if SetVerbose(true) is called beforehand
        {
            if (IsVerbose())
                messages.Add(new Message(message, MessageSource.Misc, MessageSeverity.Debug));
        }
        #endregion debug
        private static void ClearLine()
        {
            Console.CursorLeft = 0;
            Console.Write(new string(' ', Console.WindowWidth - 1)); // Clear the line by writing spaces
            Console.CursorLeft = 0;
        }

    }
}
