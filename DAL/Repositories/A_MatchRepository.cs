
using System.Linq;
using Model;
using System;
using System.Collections.Generic;
using Model.API;

namespace DAL
{
    public class A_MatchRepository : Repository<A_Match>
    {

        #region 变量
        Context db;
        #endregion

        #region 构造函数
        /// <summary>
        ///  创建实例
        /// </summary>
        /// <param name="db"></param>
        public A_MatchRepository(Context db)
            : base(db)
        {
            this.db = db;
        }
        #endregion
        /// <summary>
        /// 根据两队ID和比赛时间获取比赛
        /// </summary>
        /// <returns></returns>
        public A_Match GetByHVTime(string HID, string VID, DateTime time)
        {
            return db.A_Match.Where(x => x.HomeTeamID == HID && x.VisitingTeamID == VID && x.SP_GameStartTime == time).FirstOrDefault();
        }
        /// <summary>
        /// 根据两队ID和比赛日期获取比赛
        /// </summary>
        /// <returns></returns>
        public A_Match GetByHVDate(string HID, string VID, DateTime date)
        {
            DateTime date2 = date.AddDays(1);
            return db.A_Match.Where(x => x.HomeTeamID == HID && x.VisitingTeamID == VID && x.SP_GameStartTime >= date && x.SP_GameStartTime < date2).FirstOrDefault();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<A_Match> FindByDate(string sp, string sportsType, DateTime date)
        {
            var result = db.A_Match.Where(x => x.SourcePlatform == sp && x.SP_GameStartTime >= date);
            if (!string.IsNullOrEmpty(sportsType))
            {
                result = result.Where(x => x.SportsType == sportsType);
            }
            return result.ToList();
        }



        public List<BallModel> GetBallList(string TabCode)
        {
            DateTime jinri = GetJinRiEndTime();
            var linq = db.A_Match.AsQueryable();
            if (TabCode == SportTabEnum.GunQiu.ToString())
            {
                //LastMenuType（3滚球，1早盘，2今日）
                linq = linq.Where(x => x.LastMenuType == "3" && x.IsLock != "1" && x.IsEnd != "1").OrderBy(x => x.SP_GameStartTime);
            }
            else if (TabCode == SportTabEnum.JinRi.ToString())
            {
                //今日,
                linq = linq.Where(x => x.LastMenuType == "2" && x.IsStart != "1" && x.SP_GameStartTime < jinri && x.SP_GameStartTime > DateTime.Now).OrderBy(x => x.SP_GameStartTime);
            }
            else if (TabCode == SportTabEnum.ZaoPan.ToString())
            {
                //早盘
                linq = linq.Where(x => x.LastMenuType == "1" && x.IsStart != "1" && x.SP_GameStartTime > DateTime.Now).OrderBy(x => x.SP_GameStartTime);
            }

            var pl = from py in db.O_Odds
                     group py by new { MatchID = py.MatchID } into gpl
                     select new { MatchID = gpl.Key.MatchID, Cnt = gpl.Count() };

            var lq = from a in linq join b in pl on a.ID equals b.MatchID select a;

            return lq.GroupBy(x => x.SportsType).Select(x => new BallModel { BallCode = x.Key, BallName = "", RaceNum = x.Count() }).ToList();

        }


        public List<LeagueModel> GetLeagueList(string TabCode, string BallCode)
        {
            DateTime jinri = GetJinRiEndTime();
            var linq = db.A_Match.Where(x => x.SportsType == BallCode).AsQueryable();

            if (TabCode == SportTabEnum.GunQiu.ToString())
            {
                //LastMenuType（3滚球，1早盘，2今日）
                linq = linq.Where(x => x.LastMenuType == "3" && x.IsLock != "1" && x.IsEnd != "1").OrderBy(x => x.SP_GameStartTime);
            }
            else if (TabCode == SportTabEnum.JinRi.ToString())
            {
                //今日,
                linq = linq.Where(x => x.LastMenuType == "2" && x.IsStart != "1" && x.SP_GameStartTime < jinri && x.SP_GameStartTime > DateTime.Now).OrderBy(x => x.SP_GameStartTime);
            }
            else if (TabCode == SportTabEnum.ZaoPan.ToString())
            {
                //早盘
                linq = linq.Where(x => x.LastMenuType == "1" && x.IsStart != "1" && x.SP_GameStartTime > DateTime.Now).OrderBy(x => x.SP_GameStartTime);
            }

            var pl = from py in db.O_Odds
                     group py by new { MatchID = py.MatchID } into gpl
                     select new { MatchID = gpl.Key.MatchID, Cnt = gpl.Count() };

            var lq = from a in linq join b in pl on a.ID equals b.MatchID select a;

            var query = from m in lq
                        join
                        l in db.A_LeagueMatch
                        on m.LeagueMatchID equals l.ID
                        select new LeagueModel
                        {
                            LeagueId = l.ID,
                            LeagueName = l.Name
                        };

            return query.Distinct().OrderBy(x => x.LeagueName).ToList<LeagueModel>();

        }


        /// <summary>
        /// 获取赛事列表
        /// </summary>
        /// <param name="TabCode">滚球，今日，早盘</param>
        /// <param name="BallCode">球类别</param>
        /// <param name="LeagueId">联赛ID</param>
        /// <returns></returns>
        public List<RaceModel> GetRaceList(string TabCode, string BallCode, string LeagueId, ref int TotalPage, int PageIndex, int PageSize)
        {
            DateTime jinri = GetJinRiEndTime();

            var maxOdd = from o in db.O_Odds
                         group o by new { MatchID = o.MatchID, BetCode = o.BetCode, OddsSort = o.OddsSort, MainSort = o.MainSort }
            into g
                         select new { MatchID = g.Key.MatchID, BetCode = g.Key.BetCode, OddsSort = g.Key.OddsSort, MainSort = g.Key.MainSort, ModifyTime = g.Max(x => x.ModifyTime) };

            var odd = from o in db.O_Odds
                      join mo in maxOdd
                      on new { MatchID = o.MatchID, BetCode = o.BetCode, OddsSort = o.OddsSort, MainSort = o.MainSort, ModifyTime = o.ModifyTime } equals
                      new { MatchID = mo.MatchID, BetCode = mo.BetCode, OddsSort = mo.OddsSort, MainSort = mo.MainSort, ModifyTime = mo.ModifyTime }
                      select o;

            var play = from p in maxOdd
                       group p by new { MatchID = p.MatchID, BetCode = p.BetCode } into gp
                       select new { MatchID = gp.Key.MatchID, BetCode = gp.Key.BetCode };

            var pl = from py in play
                     group py by new { MatchID = py.MatchID } into gpl
                     select new { MatchID = gpl.Key.MatchID, Cnt = gpl.Count() };

            var linq = from m in db.A_Match.Where(x => x.SportsType == BallCode)
                       join lm in db.A_LeagueMatch
                       on m.LeagueMatchID equals lm.ID
                       join t1 in db.A_Team
                       on m.HomeTeamID equals t1.ID
                       join t2 in db.A_Team
                       on m.VisitingTeamID equals t2.ID

                       join pn in pl on m.ID equals pn.MatchID

                       join o1 in odd.Where(lf1 => lf1.BetCode == "1x2" && lf1.OddsSort == "1" && lf1.MainSort == 1 && lf1.SportsType == BallCode) on m.ID equals o1.MatchID
                       into tmp1
                       from lf1 in tmp1.DefaultIfEmpty()

                       join o2 in odd.Where(lf2 => lf2.BetCode == "1x2" && lf2.OddsSort == "2" && lf2.MainSort == 1 && lf2.SportsType == BallCode) on m.ID equals o2.MatchID
                       into tmp2
                       from lf2 in tmp2.DefaultIfEmpty()

                       join o3 in odd.Where(lf3 => lf3.BetCode == "1x2" && lf3.OddsSort == "x" && lf3.MainSort == 1 && lf3.SportsType == BallCode) on m.ID equals o3.MatchID
                       into tmp3
                       from lf3 in tmp3.DefaultIfEmpty()

                       join o4 in odd.Where(lf4 => lf4.BetCode == "ah" && lf4.OddsSort == "1" && lf4.MainSort == 1 && lf4.SportsType == BallCode) on m.ID equals o4.MatchID
                       into tmp4
                       from lf4 in tmp4.DefaultIfEmpty()

                       join o5 in odd.Where(lf5 => lf5.BetCode == "ah" && lf5.OddsSort == "2" && lf5.MainSort == 1 && lf5.SportsType == BallCode) on m.ID equals o5.MatchID
                       into tmp5
                       from lf5 in tmp5.DefaultIfEmpty()

                       join o6 in odd.Where(lf6 => lf6.BetCode == "ou" && lf6.OddsSort == "o" && lf6.MainSort == 1 && lf6.SportsType == BallCode) on m.ID equals o6.MatchID
                       into tmp6
                       from lf6 in tmp6.DefaultIfEmpty()

                       join o7 in odd.Where(lf7 => lf7.BetCode == "ou" && lf7.OddsSort == "u" && lf7.MainSort == 1 && lf7.SportsType == BallCode) on m.ID equals o7.MatchID
                       into tmp7
                       from lf7 in tmp7.DefaultIfEmpty()

                       select
                       new RaceModel
                       {
                           LeagueId = lm.ID,
                           LeagueName = lm.Name,
                           RaceId = m.ID,
                           MatchType = m.MatchType,
                           Team1Name = t1.Name,
                           Team2Name = t2.Name,
                           Status = m.IsStart,
                           StatusText = m.StatusText,
                           Score1 = m.HomeTeamScore,
                           Score2 = m.VisitingTeamScore,
                           SP_GameStartTime = m.SP_GameStartTime,
                           Timing = m.Timing,
                           GameEndTime = m.GameEndTime,
                           ExistLive = m.ExistLive,
                           IsStart = m.IsStart,
                           IsLock = m.IsLock,
                           PlaysNum = pn.Cnt,
                           LastMenuType = m.LastMenuType,
                           IsEnd = m.IsEnd,
                           Plays = new PlayModel()
                           {
                               Win1Odds = lf1.Odds,
                               Win1Text = lf1.BetExplain,
                               Win1Id = lf1.ID,
                               Win1MainSort = lf1.MainSort,
                               Win1OddsSort = lf1.OddsSort,
                               Win1BetCode = lf1.BetCode,
                               Win2Odds = lf2.Odds,
                               Win2Text = lf2.BetExplain,
                               Win2Id = lf2.ID,
                               Win2MainSort = lf2.MainSort,
                               Win2OddsSort = lf2.OddsSort,
                               Win2BetCode = lf2.BetCode,
                               TieOdds = lf3.Odds,
                               TieText = lf3.BetExplain,
                               TieId = lf3.ID,
                               TieBetCode = lf3.BetCode,
                               TieMainSort = lf3.MainSort,
                               TieOddsSort = lf3.OddsSort,
                               Let1Text = lf4.BetExplain,
                               Let1Odds = lf4.Odds,
                               Let1Id = lf4.ID,
                               Let1MainSort = lf4.MainSort,
                               Let1OddsSort = lf4.OddsSort,
                               Let1BetCode = lf4.BetCode,
                               Let2Text = lf5.BetExplain,
                               Let2Odds = lf5.Odds,
                               Let2Id = lf5.ID,
                               Let2MainSort = lf5.MainSort,
                               Let2OddsSort = lf5.OddsSort,
                               Let2BetCode = lf5.BetCode,
                               Bs1Text = lf6.BetExplain,
                               Bs1Odds = lf6.Odds,
                               Bs1Id = lf6.ID,
                               Bs1MainSort = lf6.MainSort,
                               Bs1OddsSort = lf6.OddsSort,
                               Bs1BetCode = lf6.BetCode,
                               Bs2Text = lf7.BetExplain,
                               Bs2Odds = lf7.Odds,
                               Bs2Id = lf7.ID,
                               Bs2MainSort = lf7.MainSort,
                               Bs2OddsSort = lf7.OddsSort,
                               Bs2BetCode = lf7.BetCode
                           }

                       };


            if (TabCode == SportTabEnum.GunQiu.ToString())
            {
                //LastMenuType（3滚球，1早盘，2今日）
                linq = linq.Where(x => x.LastMenuType == "3" && x.IsLock != "1" && x.IsEnd != "1").OrderBy(x => x.SP_GameStartTime);
            }
            else if (TabCode == SportTabEnum.JinRi.ToString())
            {
                //今日,
                linq = linq.Where(x => x.LastMenuType == "2" && x.IsStart != "1" && x.SP_GameStartTime < jinri && x.SP_GameStartTime > DateTime.Now).OrderBy(x => x.SP_GameStartTime);
            }
            else if (TabCode == SportTabEnum.ZaoPan.ToString())
            {
                linq = linq.Where(x => x.LastMenuType == "1" && x.IsStart != "1" && x.SP_GameStartTime > DateTime.Now).OrderBy(x => x.SP_GameStartTime);
            }

            if (!string.IsNullOrEmpty(LeagueId))
            {
                linq = linq.Where(x => x.LeagueId == LeagueId).OrderBy(x => x.SP_GameStartTime);
            }

            var list = linq.ToList();

            TotalPage = (int)Math.Ceiling(list.Count() * 1.0 / PageSize);

            return list.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();

        }


        public RaceModel GetRaceDetail(string RaceId)
        {

            //var maxOdd = from o in db.O_Odds
            //             group o by new { MatchID = o.MatchID, BetCode = o.BetCode, OddsSort = o.OddsSort, MainSort = o.MainSort }
            //into g
            //             select new { MatchID = g.Key.MatchID, BetCode = g.Key.BetCode, OddsSort = g.Key.OddsSort, MainSort = g.Key.MainSort, ModifyTime = g.Max(x => x.ModifyTime) };

            //var odd = from o in db.O_Odds
            //          join mo in maxOdd
            //            on new { MatchID = o.MatchID, BetCode = o.BetCode, OddsSort = o.OddsSort, MainSort = o.MainSort, ModifyTime = o.ModifyTime } equals
            //            new { MatchID = mo.MatchID, BetCode = mo.BetCode, OddsSort = mo.OddsSort, MainSort = mo.MainSort, ModifyTime = mo.ModifyTime }
            //          select o;

            var play = from p in db.O_Odds
                       group p by new { MatchID = p.MatchID, BetCode = p.BetCode } into gp
                       select new { MatchID = gp.Key.MatchID, BetCode = gp.Key.BetCode, Num = gp.Count() };

            var pl = from py in play
                     group py by new { MatchID = py.MatchID } into gpl
                     select new { MatchID = gpl.Key.MatchID, Cnt = gpl.Count() };

            var linq = from m in db.A_Match
                       join lm in db.A_LeagueMatch
                       on m.LeagueMatchID equals lm.ID
                       join t1 in db.A_Team
                       on m.HomeTeamID equals t1.ID
                       join t2 in db.A_Team
                       on m.VisitingTeamID equals t2.ID
                       join pn in pl on m.ID equals pn.MatchID

                       join o1 in db.O_Odds.Where(lf1 => lf1.BetCode == "1x2" && lf1.OddsSort == "1" && lf1.MainSort == 1) on m.ID equals o1.MatchID
                       into tmp1
                       from lf1 in tmp1.DefaultIfEmpty()

                       join o2 in db.O_Odds.Where(lf2 => lf2.BetCode == "1x2" && lf2.OddsSort == "2" && lf2.MainSort == 1) on m.ID equals o2.MatchID
                       into tmp2
                       from lf2 in tmp2.DefaultIfEmpty()

                       join o3 in db.O_Odds.Where(lf3 => lf3.BetCode == "1x2" && lf3.OddsSort == "x" && lf3.MainSort == 1) on m.ID equals o3.MatchID
                       into tmp3
                       from lf3 in tmp3.DefaultIfEmpty()

                       join o4 in db.O_Odds.Where(lf4 => lf4.BetCode == "ah" && lf4.OddsSort == "1" && lf4.MainSort == 1) on m.ID equals o4.MatchID
                       into tmp4
                       from lf4 in tmp4.DefaultIfEmpty()

                       join o5 in db.O_Odds.Where(lf5 => lf5.BetCode == "ah" && lf5.OddsSort == "2" && lf5.MainSort == 1) on m.ID equals o5.MatchID
                       into tmp5
                       from lf5 in tmp5.DefaultIfEmpty()

                       join o6 in db.O_Odds.Where(lf6 => lf6.BetCode == "ou" && lf6.OddsSort == "o" && lf6.MainSort == 1) on m.ID equals o6.MatchID
                       into tmp6
                       from lf6 in tmp6.DefaultIfEmpty()

                       join o7 in db.O_Odds.Where(lf7 => lf7.BetCode == "ou" && lf7.OddsSort == "u" && lf7.MainSort == 1) on m.ID equals o7.MatchID
                       into tmp7
                       from lf7 in tmp7.DefaultIfEmpty()

                       select
                       new RaceModel
                       {
                           LeagueId = lm.ID,
                           LeagueName = lm.Name,
                           RaceId = m.ID,
                           MatchType = m.MatchType,
                           Team1Name = t1.Name,
                           Team2Name = t2.Name,
                           Status = m.IsStart,
                           StatusText = m.StatusText,
                           Score1 = m.HomeTeamScore,
                           Score2 = m.VisitingTeamScore,
                           SP_GameStartTime = m.SP_GameStartTime,
                           Timing = m.Timing,
                           GameEndTime = m.GameEndTime,
                           ExistLive = m.ExistLive,
                           IsStart = m.IsStart,
                           IsLock = m.IsLock,
                           PlaysNum = pn.Cnt,
                           LastMenuType = m.LastMenuType,
                           IsEnd = m.IsEnd,
                           Plays = new PlayModel()
                           {
                               Win1Odds = lf1.Odds,
                               Win1Text = lf1.BetExplain,
                               Win1Id = lf1.ID,
                               Win1MainSort = lf1.MainSort,
                               Win1OddsSort = lf1.OddsSort,
                               Win1BetCode = lf1.BetCode,
                               Win2Odds = lf2.Odds,
                               Win2Text = lf2.BetExplain,
                               Win2Id = lf2.ID,
                               Win2MainSort = lf2.MainSort,
                               Win2OddsSort = lf2.OddsSort,
                               Win2BetCode = lf2.BetCode,
                               TieOdds = lf3.Odds,
                               TieText = lf3.BetExplain,
                               TieId = lf3.ID,
                               TieBetCode = lf3.BetCode,
                               TieMainSort = lf3.MainSort,
                               TieOddsSort = lf3.OddsSort,
                               Let1Text = lf4.BetExplain,
                               Let1Odds = lf4.Odds,
                               Let1Id = lf4.ID,
                               Let1MainSort = lf4.MainSort,
                               Let1OddsSort = lf4.OddsSort,
                               Let1BetCode = lf4.BetCode,
                               Let2Text = lf5.BetExplain,
                               Let2Odds = lf5.Odds,
                               Let2Id = lf5.ID,
                               Let2MainSort = lf5.MainSort,
                               Let2OddsSort = lf5.OddsSort,
                               Let2BetCode = lf5.BetCode,
                               Bs1Text = lf6.BetExplain,
                               Bs1Odds = lf6.Odds,
                               Bs1Id = lf6.ID,
                               Bs1MainSort = lf6.MainSort,
                               Bs1OddsSort = lf6.OddsSort,
                               Bs1BetCode = lf6.BetCode,
                               Bs2Text = lf7.BetExplain,
                               Bs2Odds = lf7.Odds,
                               Bs2Id = lf7.ID,
                               Bs2MainSort = lf7.MainSort,
                               Bs2OddsSort = lf7.OddsSort,
                               Bs2BetCode = lf7.BetCode
                           }

                       };

            if (!string.IsNullOrEmpty(RaceId))
            {
                linq = linq.Where(x => x.RaceId == RaceId);
            }

            return linq.FirstOrDefault();

        }

        public List<OddsModel> GetByMatchId(string RaceId)
        {
            var linq = from o in db.O_Odds.Where(x => x.MatchID == RaceId)
                       join b in db.S_BetCode on new { SportsType = o.SportsType, BetCode = o.BetCode } equals new { SportsType = b.SportsType, BetCode = b.Code }
                       join s in db.S_Section on new { SportsType = b.SportsType, SectionCode = b.SectionCode } equals new { SportsType = s.SportsType, SectionCode = s.Code }
                       select new OddsModel
                       {
                           RaceId = o.MatchID,
                           BetCode = o.BetCode,
                           BetExplain = o.BetExplain,
                           MainSort = o.MainSort,
                           OddId = o.ID,
                           OddsSort = o.OddsSort,
                           ReadSort = o.ReadSort,
                           Odds = o.Odds,
                           LastOdds = o.LastOdds,
                           ModifyTime = o.ModifyTime,
                           BetName = b.CodeName,
                           SectionCode = s.Code,
                           SectionName = s.Name,
                           SectionSort = s.Sort,
                           BetCodeSort = b.Sort
                       };

            return linq.ToList();
        }

        public List<O_Odds> GetOdds(string RaceId, string BetCode, int? MainSort, string OddsSort, string BetExplain)
        {
            var linq = db.O_Odds.Where(x => x.MatchID == RaceId && x.BetCode == BetCode && x.MainSort == MainSort && x.OddsSort == OddsSort && x.BetExplain == BetExplain);

            return linq.ToList();
        }


        private DateTime GetJinRiEndTime()
        {
            return DateTime.Now.AddHours(24);
        }


    }
}
