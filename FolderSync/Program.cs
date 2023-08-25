using System.Collections;
using System.Security.Cryptography;

namespace FolderSync
{
    public class Program
    {
        private static string? _folder1, _folder2, _log;
        private static int _interval = 0;
        private static LogFileClass? log;
        private static bool didWork = false;

        /// <summary>
        /// Calculates and compares the hash values of 2 files using MD5.
        /// </summary>
        /// <param name="source">The source file.</param>
        /// <param name="dest">The destination file.</param>
        /// <returns>True if the files differ, False if they are the same.</returns>
        internal static bool ChecksumsDiffer(string source, string dest)
        {
            using (var md5 = MD5.Create())
            {
                try
                {
                    if (!File.Exists(source))
                        throw new Exception("The source file can not be opened.");
                }
                catch (Exception ex)
                {
                    log.Log(ex.Message);
                    return false;
                }
                var sourceFile = File.OpenRead(source);
                var sourceHash = md5.ComputeHash(sourceFile);
                sourceFile.Close();

                try
                {
                    if (!File.Exists(dest))
                        throw new Exception("The destination file can not be opened.");
                }
                catch (Exception ex)
                {
                    log.Log(ex.Message);
                    return false;
                }
                var destFile = File.OpenRead(dest);
                var destHash = md5.ComputeHash(destFile);
                destFile.Close();

                return !StructuralComparisons.StructuralEqualityComparer.Equals(sourceHash, destHash);
            }
        }

        /// <summary>
        /// Parses the received console arguments.
        /// </summary>
        /// <param name="args">The list of strings that define the console arguments.</param>
        /// <returns>Returns 1 if the arguments were parsed successfully, or -1 in case of exceptions.</returns>
        internal static int ArgumentsParsing(string[] args)
        {
            List<string> arguments = args.ToList();
            //get log path
            try
            {
                _log = arguments.ElementAt(arguments.FindIndex(x => x.Contains("-l")) + 1);
                if (_log == null)
                    throw new Exception("Path " + _log + " does not exist!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            //get folder paths
            try
            {
                _folder1 = arguments.ElementAt(arguments.FindIndex(x => x.Contains("-s")) + 1);
                if (_folder1 == null || !Directory.Exists(_folder1))
                    throw new Exception("Path " + _folder1 + " does not exist!");
                _folder2 = arguments.ElementAt(arguments.FindIndex(x => x.Contains("-d")) + 1);
                if (_folder2 == null || !Directory.Exists(_folder2))
                    throw new Exception("Path " + _folder2 + " does not exist!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            //get interval
            try
            {
                if (_interval != null)
                    _interval = Convert.ToInt32(arguments.ElementAt(arguments.FindIndex(x => x.Contains("-i")) + 1));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong with parsing the interval.");
                Console.WriteLine(ex.Message);
                return -1;
            }

            return 1;
        }

        /// <summary>
        /// Computes the list of folders that are present in the source directory but not in the destination.
        /// For each folder that is not present it creates the folder from the source path to the destination path.
        /// </summary>
        /// <param name="source">The source directory path.</param>
        /// <param name="dest">The destination directory path.</param>
        internal static void CreateMissingFolders(List<string> source, List<string> dest)
        {
            List<string> missingDestFolders = new List<string>(), paths = new List<string>();
            missingDestFolders = source.Except(dest).ToList();
            //create missing folders first
            if (missingDestFolders.Count > 0)
            {
                //concat folder name to path
                paths = missingDestFolders.Select(x => _folder2 + x).ToList();
                foreach (var path in paths)
                {
                    try
                    {
                        log.Log("Creating folder: " + path.Replace(_folder2, ""));
                        Directory.CreateDirectory(path);
                        didWork = true;
                    }
                    catch (Exception ex)
                    {
                        log.Log("Something went wrong when creating: " + path.Replace(_folder2, ""));
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Computes the list of files that are present in the source directory but not in the destination.
        /// For each file that is not present it copies the file and all it's contents from the source path to the destination path.
        /// Will not overwrite files.
        /// </summary>
        /// <param name="source">The source directory path.</param>
        /// <param name="dest">The destination directory path.</param>
        internal static void CreateMissingFiles(List<string> source, List<string> dest)
        {
            List<string> missingDestFiles = new List<string>(), paths = new List<string>();
            missingDestFiles = source.Except(dest).ToList();
            //create missing files first
            if (missingDestFiles.Count > 0)
            {
                //concat file name to path
                paths = missingDestFiles.Select(x => _folder2 + x).ToList();
                foreach (var path in paths)
                {
                    try
                    {
                        log.Log("Creating file: " + path.Replace(_folder2, ""));
                        File.Copy(path.Replace(_folder2, _folder1), path);
                        didWork = true;
                    }
                    catch (Exception ex)
                    {
                        log.Log("Something went wrong when creating: " + path.Replace(_folder2, ""));
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Computes the list of files that are present in the source directory and in the destination.
        /// For each file that is present it copies the file and all it's contents from the source path to the destination path.
        /// Will overwrite files.
        /// </summary>
        /// <param name="source">The source directory path.</param>
        /// <param name="dest">The destination directory path.</param>
        internal static void UpdateChangedFiles(List<string> source, List<string> dest)
        {
            List<string> changedDestFiles = new List<string>(), paths = new List<string>();
            changedDestFiles = source.Intersect(dest).Where(x => ChecksumsDiffer(_folder1 + x, _folder2 + x) == true).ToList();

            //update existing files
            if (changedDestFiles.Count > 0)
            {
                //concat file name to path
                paths = changedDestFiles.Select(x => _folder2 + x).ToList();
                foreach (var path in paths)
                {
                    try
                    {
                        log.Log("Updating file: " + path.Replace(_folder2, ""));
                        File.Copy(path.Replace(_folder2, _folder1), path, true);
                        didWork = true;
                    }
                    catch (Exception ex)
                    {
                        log.Log("Something went wrong when updating: " + path.Replace(_folder2, ""));
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Computes the list of files that are not present in the source directory but present in the destination.
        /// For each file that is not present in the source it deletes the file in the destination.
        /// </summary>
        /// <param name="source">The source directory path.</param>
        /// <param name="dest">The destination directory path.</param>
        internal static void DeleteFiles(List<string> source, List<string> dest)
        {
            List<string> deleteDestFiles = new List<string>(), paths = new List<string>();
            deleteDestFiles = dest.Except(source).ToList();

            //delete files
            if (deleteDestFiles.Count > 0)
            {
                //concat file name to path
                paths = deleteDestFiles.Select(x => _folder2 + x).ToList();
                foreach (var path in paths)
                {
                    try
                    {
                        log.Log("Deleting file: " + path.Replace(_folder2, ""));
                        File.Delete(path);
                        didWork = true;
                    }
                    catch (Exception ex)
                    {
                        log.Log("Something went wrong when deleting: " + path.Replace(_folder2, ""));
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Computes the list of folders that are not present in the source directory but present in the destination.
        /// For each folder that is present in the source it deletes the folder from the destination starting at children directories and working it's way up.
        /// </summary>
        /// <param name="source">The source directory path.</param>
        /// <param name="dest">The destination directory path.</param>
        internal static void DeleteFolders(List<string> source, List<string> dest)
        {
            List<string> deleteDestFolders = new List<string>(), paths = new List<string>();
            deleteDestFolders = dest.Except(source).ToList();

            //delete folders
            if (deleteDestFolders.Count > 0)
            {
                //concat file name to path
                paths = deleteDestFolders.Select(x => _folder2 + x).ToList().OrderByDescending(x => x).ToList();
                foreach (var path in paths)
                {
                    try
                    {
                        log.Log("Deleting folder: " + path.Replace(_folder2, ""));
                        Directory.Delete(path);
                        didWork = true;
                    }
                    catch (Exception ex)
                    {
                        log.Log("Something went wrong when deleting: " + path.Replace(_folder2, ""));
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Encapsulates the entire logic for the Folder Syncing program.
        /// Parses arguments, reads the entire hierarchy of the source and destination directories, does the necessary checks and actions to sync up the directories.
        /// </summary>
        /// <param name="args">The list of strings that define the console arguments.</param>
        /// <param name="-s source">The source directory path.</param>
        /// <param name="-d dest">The destination directory path.</param>
        /// <param name="-i interval">The sync interval.</param>
        /// <param name="-l log">The log file path.</param>
        static int Main(string[] args)
        {
            /* ARGUMENTS
             * -s source - first folder path
             * -d dest - second folder path
             * -i interval - syncing interval
             * -l log - log file path
             */

            //check arguments number
            if (args.Length == 0)
            {
                return -1;
            }
            if (args.Length >= 1 && args.Length < 4)
            {
                return -1;
            }

            //parsing arguments list
            if (ArgumentsParsing(args) == -1)
                return -1;

            log = new LogFileClass(_log);
            log.Log("Syncing directories...");

            while (true)
            {
                //read folder contents
                List<string> sourceFolders = new List<string>(), sourceFiles = new List<string>();
                List<string> destFolders = new List<string>(), destFiles = new List<string>();

                try
                {
                    sourceFolders = Directory.EnumerateDirectories(_folder1, "*", SearchOption.AllDirectories).Select(x => x.Replace(_folder1, "")).ToList();
                    sourceFiles = Directory.EnumerateFiles(_folder1, "*", SearchOption.AllDirectories).Select(x => x.Replace(_folder1, "")).ToList();

                    destFolders = Directory.EnumerateDirectories(_folder2, "*", SearchOption.AllDirectories).Select(x => x.Replace(_folder2, "")).ToList();
                    destFiles = Directory.EnumerateFiles(_folder2, "*", SearchOption.AllDirectories).Select(x => x.Replace(_folder2, "")).ToList();
                }
                catch (Exception ex)
                {
                    log.Log("Unable to read full content from given folders.");
                    log.Log(ex.Message);
                    log.Close();
                    return -1;
                }

                //doing work
                CreateMissingFolders(sourceFolders, destFolders);

                CreateMissingFiles(sourceFiles, destFiles);

                UpdateChangedFiles(sourceFiles, destFiles);

                DeleteFiles(sourceFiles, destFiles);

                DeleteFolders(sourceFolders, destFolders);

                //preparing for next iteration
                if (!didWork)
                    Console.WriteLine("Nothing to report in this iteration.");
                else didWork = false;
                Console.Write("\n");

                log.Flush();
                Thread.Sleep(_interval);
            }
        }
    }
}