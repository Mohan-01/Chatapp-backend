using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.EventContracts
{
    public class UserDeletedEvent
    {
        public string Username { get; set; } = string.Empty;
    }
}
