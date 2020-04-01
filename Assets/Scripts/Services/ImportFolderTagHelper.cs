using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StlVault.Config;

namespace StlVault.Services
{
    internal static class ImportFolderTagHelper
    {  
        private static readonly char[] Separators = {'_', '-', ' ', '.', '(', ')', '+'};
        private static readonly char[] Digits = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};

        public static IReadOnlyList<string> GenerateTags(ImportFolderConfig folderConfig, string resourcePath)
        {
            var rootPath = folderConfig.FullPath;
            var fileName = Path.GetFileNameWithoutExtension(resourcePath);

            return BuildDumbTags(GetTagGenerationString(folderConfig.AutoTagMode))
                .Append("folder: " + rootPath.ToLowerInvariant())
                .ToList();

            string GetTagGenerationString(AutoTagMode mode)
            {
                switch (mode)
                {
                    case AutoTagMode.ExplodeResourcePath: return resourcePath;
                    case AutoTagMode.ExplodeFileName: return fileName;
                    default: return string.Empty;
                }
            }
        }
        
        private static IEnumerable<string> BuildDumbTags(string file)
        {
            return file?.Split(Path.DirectorySeparatorChar)
                .SelectMany(name => name.Trim().Split(Separators, StringSplitOptions.RemoveEmptyEntries))
                .Select(tag => tag.Trim().ToLowerInvariant().Trim(Digits))
                .Where(tag => tag.Length > 2 && !IsOnBlackList(tag));
        }
        
        private static bool IsOnBlackList(string tag) => tag == "repaired" || tag == "stl";
    }
}