using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Model.API
{
    public class RaceModel
    {
        public string RaceId { get; set; }

        public string LeagueId { get; set; }

        public string LeagueName { get; set; }

        public string MatchType { get; set; }

        public string Team1Name { get; set; }

        public string Team2Name { get; set; }

        public string Score1 { get; set; }

        public string Score2 { get; set; }

        public string Status { get; set; }

        public string StatusText { get; set; }

        public DateTime? SP_GameStartTime { get; set; }

        public DateTime? GameEndTime { get; set; }

        public int? Timing { get; set; }

        public PlayModel Plays { get; set; }

        public int PlaysNum { get; set; }


        public string ExistLive { get; set; }

        public string IsStart { get; set; }

        public string IsLock { get; set; }

        public bool HasLive { get; set; }

        public bool HasCount { get; set; }


        public string LastMenuType { get; set; }

        public string IsEnd { get; set; }
    }
}