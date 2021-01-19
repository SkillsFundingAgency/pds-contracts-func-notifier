using System;
using System.Collections.Generic;
using System.Text;

namespace Pds.Contracts.Notifications.Services.Models
{
    public class Contract
    {
        public string ContractNumber { get; set; }

        public int ContractVersion { get; set; }

        public ContractStatus Status { get; set; }
    }
}
