using System;
using System.Linq;
using DAL;
using System.Collections.Generic;
using Model;
using Model.API;

namespace BLL
{
    public class A_MatchBll : Bll<A_Match>
    {
        public A_Match GetByHVTime(string HID, string VID, DateTime time)
        {
            return new A_MatchRepository(new Context()).GetByHVTime(HID, VID, time);
        }

        public A_Match GetByHVDate(string HID, string VID, DateTime date)
        {
            return new A_MatchRepository(new Context()).GetByHVDate(HID, VID, date);
        }
        public List<A_Match> FindByDate(string sp, string sportsType, DateTime date)
        {
            return new A_MatchRepository(new Context()).FindByDate(sp, sportsType, date);
        }



        public List<BallModel> GetBallList(string TabCode)
        {
            return new A_MatchRepository(new Context()).GetBallList(TabCode);
        }



        /// <summary>
        /// ��ȡ�����б�
        /// </summary>
        /// <param name="TabCode">���򣬽��գ�����</param>
        /// <param name="BallCode">�����</param>
        /// <param name="LeagueId">����ID</param>
        /// <param name="PageIndex">ҳ������1��ʼ��</param>
        /// <param name="PageSize">ÿҳ����</param>
        /// <returns></returns>
        public List<RaceModel> GetRaceList(string TabCode, string BallCode, string LeagueId, ref int TotalPage, int PageIndex = 1, int PageSize = 10)
        {
            return new A_MatchRepository(new Context()).GetRaceList(TabCode, BallCode, LeagueId, ref TotalPage,PageIndex, PageSize);
        }


        public RaceModel GetRaceDetail(string RaceId)
        {
            return new A_MatchRepository(new Context()).GetRaceDetail(RaceId);
        }

        public List<OddsModel> GetByMatchId(string RaceId)
        {
            return new A_MatchRepository(new Context()).GetByMatchId(RaceId);
        }

        public List<O_Odds> GetOdds(string RaceId, string BetCode, int? MainSort, string OddsSort, string BetExplain)
        {
            return new A_MatchRepository(new Context()).GetOdds(RaceId, BetCode, MainSort, OddsSort, BetExplain);
        }

        public List<LeagueModel> GetLeagueList(string TabCode, string BallCode)
        {
            return new A_MatchRepository(new Context()).GetLeagueList(TabCode, BallCode);
        }


    }
}


