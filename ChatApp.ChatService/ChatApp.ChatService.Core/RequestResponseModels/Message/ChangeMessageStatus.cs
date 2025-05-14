using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.ChatService.Core.RequestResponseModels.Message
{
    public class ChangeMessageStatus
    {
        required public string MessageId { get; set; } = null!;
        required public string From { get; set; } = null!;
        required public string To { get; set; } = null!;
        required public string MessageStatus { get; set; } = null!;
    }
}
