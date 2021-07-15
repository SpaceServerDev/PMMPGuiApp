using System;
using System.IO;
using System.Text;

namespace PMMPGuiApp.Data {
    internal class ProcessData {

        private string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\yurisi\PMMPGuiApp";

        private int processID;

        public ProcessData() {
            Directory.CreateDirectory(path);
            if (!File.Exists(path + @"\PROCESS")) File.Create(path + @"\PROCESS");
            using (StreamReader reader = new StreamReader(path + @"\PROCESS")) {
                int output = -1;
                int.TryParse(reader.ReadLine(), out output);
                processID = output;
            }
        }

        public void setProcess(string PID) {
            using (StreamWriter writer = new StreamWriter(path + @"\PROCESS", false, Encoding.GetEncoding("utf-8"))) {
                writer.WriteLine(PID);
                processID = int.Parse(PID);
            }
        }

        public int getProcess() {
            return processID;
        }
    }
}
