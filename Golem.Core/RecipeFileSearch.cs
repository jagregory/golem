using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Golem.Core
{
    public class RecipeFileSearch
    {
        private string[] startDirs;
        public List<FileInfo> FoundAssemblyFiles = new List<FileInfo>();
        public ReadOnlyCollection<string> DistinctAssemblyFolders;
        
        public RecipeFileSearch()
        {
            startDirs = new[] { Environment.CurrentDirectory };    
        }

        public RecipeFileSearch(params string[] startDirs)
        {
            this.startDirs = startDirs;
        }

        public void BuildFileList()
        {
            FoundAssemblyFiles.Clear();
            
            foreach (var startDir in startDirs)
            {
                FileInfo[] dlls = FindFilesExcludingDuplicates(startDir);
                FoundAssemblyFiles.AddRange(dlls);
            }

            var tmp = FoundAssemblyFiles.GroupBy(s => s.Directory.FullName);
            DistinctAssemblyFolders = tmp.Select(s => s.Key).ToList().AsReadOnly();
            
        }

        private FileInfo[] FindFilesExcludingDuplicates(string startDir)
        {
            //if a file, then return
            var tmp = new FileInfo(startDir);
            if( tmp.Exists )
            {
                return new[]{tmp};
            }

            var found = new DirectoryInfo(startDir)
                .GetFiles("*.dll", SearchOption.AllDirectories);

            var valid = new List<FileInfo>();

            foreach (var fileInfo in found)
                if (!fileInfo.Directory.FullName.Contains("\\obj\\") && ! FoundAssemblyFiles.Contains(fileInfo))
                    valid.Add(fileInfo);

            return valid.ToArray();

        }

    }
}