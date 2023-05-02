using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tuplaus_teht.Service.DTO
{
    public class TuplausGameEvent
    {
        public DateTime EventTime { get; set; }
        public int PlayerID { get; set; }
        public int Stake { get; set; }
        public string Choice { get; set; }
        public int Card { get; set; }
        public int Prize { get; set; }
    }
}
