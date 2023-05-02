using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Xml.Serialization;


namespace tuplaus_teht.Common.DTO
{
    public class TuplausActionData
    {
        public int PlayerID { get; set; }
        public int Stake { get; set; }
        public string Choice { get; set; }
        public bool firstGame { get; set; }
    }
}
