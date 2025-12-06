using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopNotify.Resources;

namespace TopNotify.Daemon
{
    [Serializable]
    public class DaemonError
    {
        public string ID;
        public string Text;

        public DaemonError(string id, string text)
        {
            this.ID = id;
            this.Text = text;
        }
    }
}
