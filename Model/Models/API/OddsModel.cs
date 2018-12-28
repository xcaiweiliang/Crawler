using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.API
{
    public class OddsModel
    {
        public string RaceId { get; set; }

        public string BetCode { get; set; }

        public string BetName { get; set; }

        public string OddId { get; set; }

        public int? MainSort { get; set; }

        public string OddsSort { get; set; }

        public int? ReadSort { get; set; } 

        public string BetExplain { get; set; }

        public decimal Odds { get; set; }

        public decimal? LastOdds { get; set; }

        public DateTime ModifyTime { get; set; }

        public string SectionCode { get; set; }

        public string SectionName { get; set; }

        public int SectionSort { get; set; }

        public int BetCodeSort { get; set; }

    }
}
