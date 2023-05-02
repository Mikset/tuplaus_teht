using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml.Serialization;


namespace tuplaus_teht.Common.DTO
{
    public class TransactionData
    {
        public int PlayerID { get; set; }
        public int Amount { get; set; }
    }
}
