using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels
{
    public class SessionDTO
    {
        public SessionDTO()
        {
            session = new session();
        }

        public string merchant { get; set; }
        public string result { get; set; }

        public session session { get; set; }

    }
    public class session
    {
        public string id { get; set; }

    }
}
