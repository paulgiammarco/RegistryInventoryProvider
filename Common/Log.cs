using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RegistryInventoryProvider
{

    // Simple text logging class
    public static class Log
    {
        public static string LogLocation { get; set; }
        public static bool LogEnabled = false;

        [DebuggerStepThrough]
        public static void Write(string message)
        {
            if (!LogEnabled || string.IsNullOrEmpty(LogLocation)) return;

            message = $"{DateTimeOffset.Now.ToString("MM/dd/yyyy h:mm:ss tt")}: {message}";

            ValidateDirectory(LogLocation);

            try
            {
                using (StreamWriter LogFile = new StreamWriter(LogLocation, true))
                {
                    LogFile.Write(message + "\r\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [DebuggerStepThrough]
        public static void ValidateDirectory(string path)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }
                catch { }
            }
        }

    }
}
