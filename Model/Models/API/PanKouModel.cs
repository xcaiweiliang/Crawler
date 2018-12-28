using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.API
{
    public class PanKouModel
    {
        public string RaceId { get; set; }

        public string Score1 { get; set; }

        public string Score2 { get; set; }

        public string MatchType { get; set; }

        public int? Timing { get; set; }

        public List<PlaysModel> Plays { get; set; }
    }
}
