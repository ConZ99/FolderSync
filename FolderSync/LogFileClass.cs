using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSync
{
    internal class LogFileClass
    {
        FileStream? file;
        StreamWriter? writer;

        /// <summary>
        /// Creates and opens the log file.
        /// </summary>
        /// <param name="filename">Full path to the log file. Contains the file name as well.</param>
        internal LogFileClass(string filename)
        {
            if (filename != null)
            {
                if (File.Exists(filename))
                    File.Delete(filename);
                file = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                writer = new StreamWriter(file);
            }
        }

        /// <summary>
        /// Logs a text message to console and in the log file.
        /// </summary>
        /// <param name="text">The message that will be logged.</param>
        public void Log(string text)
        {
            if (writer != null)
            {
                Console.WriteLine(text);
                writer.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + text);
            }
        }

        /// <summary>
        /// Makes the flush functionality actionable.
        /// </summary>
        internal void Flush()
        {
            if (writer != null)
                writer.Flush();
        }

        /// <summary>
        /// Flushes data from the writer to the file and closes it.
        /// </summary>
        internal void Close()
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
