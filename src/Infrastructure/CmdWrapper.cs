using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SublimeSqlWrapper.Infrastructure
{
    public class CmdWrapper
    {
        private Process _process;
        private IList<string> _commands { get; set; }

        public string Program { get; set; }
        private bool HasStarted { get; set; }

        public CmdWrapper(string program)
        {
            this.Program = program;
            this.Restart();
        }

        public void Restart()
        {
            _process = new Process();
            _process.StartInfo.FileName = Program; //Program must be on the machine path
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.RedirectStandardOutput = true;

            this.HasStarted = false;

            _commands = new List<string>();
        }

        public void AddCommand(string command)
        {
            _commands.Add(command);
        }

        public string ExecuteCommands()
        {
            if (!this.HasStarted)
            {
                _process.Start();
                this.HasStarted = true;
            }

            using (StreamWriter sw = _process.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    foreach (var command in _commands)
                    {
                        sw.WriteLine(command);
                    }
                }
            }

            _process.WaitForExit();

            return _process.StandardOutput.ReadToEnd();
        }
    }
}
