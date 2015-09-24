using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SublimeSqlWrapper.Infrastructure
{
    internal class CommandArguments
    {
        public string DatabaseName { get; set; }
        public string DatabasePassword { get; set; }
        public string DatabaseServer { get; set; }
        public string FilePath { get; set; }

        public CommandArguments() { }

        public CommandArguments(string[] args)
        {
            if (args == null) throw new ArgumentException("No arguments supplied");
            if (args.Length < 4) throw new ArgumentException("Not all required arguments supplied");

            this.DatabaseName = args[0];
            this.DatabasePassword = args[1];
            this.DatabaseServer = args[2];
            this.FilePath = args[3];
        }
    }
}
