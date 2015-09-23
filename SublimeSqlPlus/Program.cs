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

    public class CmdWrapper {
        private Process _process;
        private IList<string> _commands { get; set; }

        public string Program { get; set; }
        private bool HasStarted { get; set; }

        public CmdWrapper(string program) {
            this.Program = program;
            this.Restart();
        }

        public void Restart() {
            _process = new Process();
            _process.StartInfo.FileName = Program; //Program must be on the machine path
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.RedirectStandardOutput = true;

            _commands = new List<string>();
        }

        public void AddCommand(string command) {
            _commands.Add(command);
        }

        public string ExecuteCommands() {
            if (!this.HasStarted) {
                _process.Start();
                this.HasStarted = true;
            }

            using (StreamWriter sw = _process.StandardInput) {
                if (sw.BaseStream.CanWrite) {
                    foreach (var command in _commands) {
                        sw.WriteLine(command);
                    }
                }
            }

            _process.WaitForExit();

            return _process.StandardOutput.ReadToEnd();
        }
    }

    public class Program {
        static void Main(string[] args) {
            CmdWrapper cmd = new CmdWrapper("sqlplus");

            var input = new CommandArguments(args);
            var output = string.Empty;
            var fileName = CreateTempFile(input.FilePath);

            cmd.AddCommand(string.Format("{0}/{1}@{2}", input.DatabaseName, input.DatabasePassword, input.DatabaseServer));
            cmd.AddCommand(string.Format("@\"{0}\"", fileName));

            //Store output results
            output = cmd.ExecuteCommands();

            File.Delete(fileName);

            //Check for errors and run errors query
            if (output.ToLower().Contains("compilation errors")) {
                cmd.Restart();

                //Connect to Database
                cmd.AddCommand(string.Format("{0}/{1}@{2}", input.DatabaseName, input.DatabasePassword, input.DatabaseServer));

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
