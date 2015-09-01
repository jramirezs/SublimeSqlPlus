using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SublimeSqlWrapper {
    internal class CommandArguments {
        public string DatabaseName { get; set; }
        public string DatabasePassword { get; set; }
        public string DatabaseServer { get; set; }
        public string FilePath { get; set; }

        public CommandArguments() { }

        public CommandArguments(string[] args) {
            if (args == null) throw new ArgumentException("No arguments supplied");
            if (args.Length < 4) throw new ArgumentException("Not all required arguments supplied");

            this.DatabaseName = args[0];
            this.DatabasePassword = args[1];
            this.DatabaseServer = args[2];
            this.FilePath = args[3];
        }
    }

    public class Program {
        static void Main(string[] args) {
            Process process = new Process();
            process.StartInfo.FileName = "sqlplus"; //SqlPlus must be on the machine path
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;

            var input = new CommandArguments(args);
            var output = string.Empty;
            var fileName = CreateTempFile(input.FilePath);

            process.Start();
            using (StreamWriter sw = process.StandardInput) {
                if (sw.BaseStream.CanWrite) {
                    //Connect to Database
                    sw.WriteLine("{0}/{1}@{2}", input.DatabaseName, input.DatabasePassword, input.DatabaseServer);

                    //Execute File
                    sw.WriteLine("@\"{0}\"", fileName);
                }
            }

            //Store output results
            output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            File.Delete(fileName);

            //Check for errors and run errors query
            if (output.ToLower().Contains("compilation errors")) {
                process.Start();
                using (StreamWriter sw = process.StandardInput) {
                    if (sw.BaseStream.CanWrite) {
                        //Connect to Database
                        sw.WriteLine("{0}/{1}@{2}", input.DatabaseName, input.DatabasePassword, input.DatabaseServer);

                        //Show Errors
                        sw.Write(File.ReadAllText("ErrorsQuery.sql"), input.DatabaseName, Path.GetFileNameWithoutExtension(input.FilePath));
                    }
                }

                //Store output errors
                output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }

            Console.Write(output);
        }

        /// <summary>
        /// Creates a modifiable identical temporal file of the source file
        /// </summary>
        /// <param name="filePath"></param>
        public static string CreateTempFile(string filePath) {
            var tempFileName = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetRandomFileName(), "sql"));
            File.Copy(filePath, tempFileName);

            //Add SqlPlus finish token (/)
            var fileContent = File.AppendText(tempFileName);
            
            fileContent.WriteLine(); 
            fileContent.WriteLine("/");

            fileContent.Close();

            return tempFileName;
        }
    }
}
