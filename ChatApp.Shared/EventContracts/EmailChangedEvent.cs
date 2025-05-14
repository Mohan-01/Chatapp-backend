using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.EventContracts
{
    public class EmailChangedEvent
    {
        public string Username { get; set; }
        public string NewEmail { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
