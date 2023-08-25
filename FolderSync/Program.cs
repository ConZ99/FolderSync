using System.Collections;
using System.Security.Cryptography;

namespace foldersync
{
    class Program
    {
        private static string? _folder1, _folder2, _log;
        private static int _interval = 0;
        private static LogFileClass? log;

        private static bool ChecksumsDiffer(string source, string dest)
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

        private static int ArgumentsParsing(string[] args)
        {
            List<string> arguments = args.ToList();
            //get log path
            try
            {
                _log = arguments.ElementAt(arguments.FindIndex(x => x.Contains("-l")) + 1);
                if (_log == null || !File.Exists(_log))
                    throw new Exception("Path " + _log + " does not exist!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            log = new LogFileClass(_log);

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
                log.Log(ex.Message);
                log.Close();
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
                log.Log(ex.Message);
                log.Close();
                return -1;
            }

            return 1;
        }

        private static void CreateMissingFolders(List<string> source, List<string> dest)
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
                        log.Log("Creating folder: " + path);
                        Directory.CreateDirectory(path);
                    }
                    catch (Exception ex)
                    {
                        log.Log("Something went wrong when creating: " + path);
                    }
                }
            }
        }

        private static void CreateMissingFiles(List<string> source, List<string> dest)
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
                        log.Log("Creating file: " + path);
                        File.Copy(path.Replace(_folder2, _folder1), path);
                    }
                    catch (Exception ex)
                    {
                        log.Log("Something went wrong when creating: " + path);
                    }
                }
            }
        }

        private static void UpdateChangedFiles(List<string> source, List<string> dest)
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
                        log.Log("Updating file: " + path);
                        File.Copy(path.Replace(_folder2, _folder1), path, true);
                    }
                    catch (Exception ex)
                    {
                        log.Log("Something went wrong when updating: " + path);
                    }
                }
            }
        }

        private static void DeleteFiles(List<string> source, List<string> dest)
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
                        log.Log("Deleting file: " + path);
                        File.Delete(path);
                    }
                    catch (Exception ex)
                    {
                        log.Log("Something went wrong when deleting: " + path);
                    }
                }
            }
        }

        private static void DeleteFolders(List<string> source, List<string> dest)
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
                        log.Log("Deleting file: " + path);
                        Directory.Delete(path);
                    }
                    catch (Exception ex)
                    {
                        log.Log("Something went wrong when deleting: " + path);
                    }
                }
            }
        }

        static int Main(string[] args)
        {
            /* ARGUMENTS
             * source - first folder path
             * dest - second folder path
             * interval - syncing interval
             * log - log file path
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
                log.Flush();
                Thread.Sleep(_interval);
            }
        }

        class LogFileClass
        {
            FileStream? file;
            StreamWriter? writer;
            public LogFileClass(string filename)
            {
                if (filename != null)
                {
                    if (File.Exists(filename))
                        File.Delete(filename);
                    file = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    writer = new StreamWriter(file);
                }
            }

            public void Log(string text)
            {
                if(writer != null)
                {
                    Console.WriteLine(text);
                    writer.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + text);
                }
            }

            public void Flush()
            {
                if(writer != null)
                    writer.Flush();
            }

            public void Close()
            {
                if (file != null && writer != null)
                {
                    writer.Flush();
                    writer.Close();
                    file.Close();
                }
            }
        }
    }
}