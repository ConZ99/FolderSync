using Microsoft.VisualStudio.TestTools.UnitTesting;
using FolderSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSync.Tests
{
    [TestClass()]
    public class ProgramTests
    {
        [TestMethod()]
        public void ChecksumsDiffer_SameFiles_ReturnsFalse()
        {
            string sourcePath = "source.txt";
            string destPath = "dest.txt";

            File.WriteAllText(sourcePath, "test content");
            File.WriteAllText(destPath, "test content");

            bool result = Program.ChecksumsDiffer(sourcePath, destPath);

            Assert.IsFalse(result);

            File.Delete(sourcePath);
            File.Delete(destPath);
        }

        [TestMethod()]
        public void ArgumentsParsing_ValidArguments_ReturnsOne()
        {
            var tempPath = Directory.GetCurrentDirectory();
            string[] args = { "-s", tempPath, "-d", tempPath, "-i", "1000", "-l", tempPath };

            int result = Program.ArgumentsParsing(args);

            Assert.AreEqual(1, result);
        }

        [TestMethod()]
        public void ArgumentsParsing_InvalidArguments_ReturnsMinusOne()
        {
            string[] args = { "-s", "sourcePath", "-d", "destPath", "-i" };

            int result = Program.ArgumentsParsing(args);

            Assert.AreEqual(-1, result);
        }

        [TestMethod()]
        public void LogFileClass_Log_ValidLogMessage_WritesToLogFile()
        {
            string logFilePath = "testLog.txt";
            var logFile = new LogFileClass(logFilePath);

            logFile.Log("Test log message");

            logFile.Flush();
            logFile.Close();

            Assert.IsTrue(File.ReadAllText(logFilePath).Contains("Test log message"));

            File.Delete(logFilePath);
        }
    }
}