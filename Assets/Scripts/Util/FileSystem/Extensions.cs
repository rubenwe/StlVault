using System;
using System.IO;
using System.Text.RegularExpressions;

namespace StlVault.Util.FileSystem
{
    /// <summary>
    /// Helper Extension Methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        ///   Returns a regex for the specified format expression.
        /// </summary>
        /// <param name="format"> The format expression. Format expression syntax: * = Any number of chars (.*?) ? = Any single character (.) # = A single digit (\d) % = A single character (\w) Other input characters will be escaped using Regex.Escape(...). </param>
        /// <returns> </returns>
        public static Regex ToRegex(this string format)
        {
            format = format.Replace("%", "%CHARACTER%").Replace("*", "%STAR%").Replace("#", "%DIGIT%").Replace("?", "%QUESTIONMARK%");

            format = Regex.Escape(format);

            format = format.Replace("%STAR%", @".*?").Replace("%DIGIT%", @"\d").Replace("%CHARACTER%", @"\w").Replace("%QUESTIONMARK%", @".");
            
            return new Regex($"^{format}$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        }

        /// <summary>
        /// Check if string ends on a special char.
        /// </summary>
        /// <param name="checkString">The string to check.</param>
        /// <param name="lastChar">The last char.</param>
        /// <returns>True if string ends on char, else false.</returns>
        public static bool EndsWith(this string checkString, char lastChar)
        {
            if(!string.IsNullOrEmpty(checkString))
            {
                return checkString[checkString.Length - 1] == lastChar;
            }

            return false;
        }

        /// <summary>
        /// Copies to temporary path.
        /// </summary>
        /// <param name="fileEntry">The file entry to copy over.</param>
        /// <returns>New temporary path for that file.</returns>
        public static string CopyToTempPath(this FileEntry fileEntry)
        {
            var tempDirectory = Path.GetTempPath();
            var tempFilePath = Path.Combine(tempDirectory, fileEntry.Name);

            using (var writer = new StreamWriter(tempFilePath, false))
            using (var reader = new StreamReader(fileEntry.GetFile()))
            {
                writer.Write(reader.ReadToEnd());
                writer.Flush();
            }

            return tempFilePath;
        }
    }
}
