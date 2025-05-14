using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.EventContracts
{
    public class UsernameChangedEvent
    {
        public string CurrentUsername { get; set; } = string.Empty;
        public string NewUsername { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
