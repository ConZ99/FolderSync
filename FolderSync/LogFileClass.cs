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

        public void Log(string text)
        {
            if (writer != null)
            {
                Console.WriteLine(text);
                writer.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + text);
            }
        }

        internal void Flush()
        {
            if (writer != null)
                writer.Flush();
        }

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
