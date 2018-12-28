using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.API
{
    public class PlaysModel
    {
        //public string RaceId { get; set; }

        public string TabCode { get; set; }

        public string TabName { get; set; }

        //public Dictionary<string,string[]> Plays { get; set; }

        public List<BetCodeModel> Plays { get; set; }

    }
}
