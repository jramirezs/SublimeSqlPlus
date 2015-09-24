using SublimeSqlWrapper.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SublimeSqlWrapper {
    public class Program {
        static void Main(string[] args) {
            try {
                Run(new CommandArguments(args));
            } catch (Exception e) {
                //Capture errors and print to avoid Sublime Crashes
                Console.Write(e.Message);
            }
        }

        private static void Run(CommandArguments input) {
            var fileName = CreateTempFile(input.FilePath);

            if (!File.Exists(fileName)) {
                throw new ArgumentException("File not save or don't exists");
            }

            CmdWrapper cmd = new CmdWrapper("sqlplus");

            var output = string.Empty;
            var connectCommand = string.Format("{0}/{1}@{2}", input.DatabaseName, input.DatabasePassword, input.DatabaseServer);

            cmd.AddCommand(connectCommand);
            cmd.AddCommand(string.Format("@\"{0}\"", fileName));

            //Store output results
            output = cmd.ExecuteCommands();

            File.Delete(fileName);

            //Check for errors and run errors query
            if (output.ToLower().Contains("compilation errors")) {
                cmd.Restart();
                cmd.AddCommand(connectCommand);

                //Show Errors
                cmd.AddCommand(string.Format(File.ReadAllText("ErrorsQuery.sql"), input.DatabaseName, Path.GetFileNameWithoutExtension(input.FilePath)));

                //Store output errors
                output = cmd.ExecuteCommands();
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
