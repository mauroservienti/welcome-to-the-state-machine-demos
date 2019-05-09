using System;
using System.Collections.Generic;
using System.Text;

namespace Finance.Service.Messages
{
    class AuthorizeCard
    {
        public Guid OrderId { get; set; }
    }
}
