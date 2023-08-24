namespace foldersync
{
    class Program
    {
        static int Main(string[] args)
        {
            /*
             * source - first folder path
             * dest - second folder path
             * interval - syncing interval
             * log - log file path
             */

            string _folder1, _folder2, _log;
            int _interval;

            foreach(var item in args)
            {
                Console.WriteLine(item);
            }


            //check arguments number
            if (args.Length == 0)
            {
                return -1;
            }
            if (args.Length >= 1 && args.Length < 4)
            {
                return -1;
            }
            //get log path
            try
            {
                Console.WriteLine(args[3]);
                _log = args[3];
                Console.WriteLine(_log);
                if (!File.Exists(_log))
                    throw new Exception("Path " + _log + " does not exist!");
                Console.WriteLine("test");
            }
            catch
            {
                return -1;
            }
            Console.WriteLine("test");
            LogFileClass log = new LogFileClass(_log);

            //get folder paths
            try
            {
                _folder1 = args[0];
                if (!Directory.Exists(_folder1))
                    throw new Exception("Path " + _folder1 + " does not exist!");
                _folder2 = args[1];
                if (!Directory.Exists(_folder2))
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
                _interval = Convert.ToInt32(args[2]);
            }
            catch (Exception ex)
            {
                log.Log(ex.Message);
                log.Close();
                return -1;
            }

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

                //check missing files/folders in dest
                List<string> missingDestFolders = new List<string>(), missingDestFiles = new List<string>(), updateDestFiles = new List<string>();

                missingDestFolders = sourceFolders.Except(destFolders).ToList();
                missingDestFiles = sourceFiles.Except(destFiles).ToList();
                updateDestFiles = sourceFiles.Intersect(destFiles).ToList();

                //check additional files/folders in dest
                List<string> deleteDestFolders = new List<string>(), deleteDestFiles = new List<string>();

                deleteDestFolders = destFolders.Except(sourceFolders).ToList();
                deleteDestFiles = destFiles.Except(sourceFiles).ToList();


                List<string> folderPaths = new List<string>(), filePaths = new List<string>();
                //create missing folders first
                if (missingDestFolders.Count > 0)
                {
                    //concat folder name to path
                    folderPaths = missingDestFolders.Select(x => _folder2 + x).ToList();
                    foreach (var path in folderPaths)
                    {
                        log.Log("Creating folder: " + path);
                        Directory.CreateDirectory(path);
                    }
                }

                //create missing files first
                if (missingDestFiles.Count > 0)
                {
                    //concat file name to path
                    filePaths = missingDestFiles.Select(x => _folder2 + x).ToList();
                    foreach (var path in filePaths)
                    {
                        log.Log("Creating file: " + path);
                        File.Copy(path.Replace(_folder2, _folder1), path);
                    }
                }

                //update existing files
                if (updateDestFiles.Count > 0)
                {
                    //concat file name to path
                    filePaths = updateDestFiles.Select(x => _folder2 + x).ToList();
                    foreach (var path in filePaths)
                    {
                        log.Log("Updating file: " + path);
                        File.Copy(path.Replace(_folder2, _folder1), path, true);
                    }
                }

                //delete files
                if (deleteDestFiles.Count > 0)
                {
                    //concat file name to path
                    filePaths = deleteDestFiles.Select(x => _folder2 + x).ToList();
                    foreach (var path in filePaths)
                    {
                        log.Log("Deleting file: " + path);
                        File.Delete(path);
                    }
                }

                //delete folders
                if (deleteDestFolders.Count > 0)
                {
                    //concat file name to path and sort to avoid deleting parent folders
                    folderPaths = deleteDestFolders.Select(x => _folder2 + x).ToList().OrderByDescending(x => x).ToList();
                    foreach (var path in folderPaths)
                    {
                        log.Log("Deleting folder: " + path);
                        Directory.Delete(path);
                    }
                }
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
                Console.WriteLine(text);
                writer?.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + text);
            }

            public void Flush()
            {
                writer?.Flush();
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