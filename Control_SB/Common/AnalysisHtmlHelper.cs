using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Control_SB
{
    /// <summary>
    /// 解析html
    /// </summary>
    public class AnalysisHtmlHelper
    {
        #region 足球
        /// <summary>
        /// 沙巴足球早盘列表，独赢 & 让球 & 大小 & 单/双
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<LeagueMatch> SB_Football_ZP_DRDD_List(string html)
        {
            html = html.Replace("&nbsp;", "");
            html = "<root>" + html + "</root>";
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            var doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            TextReader tr = new StringReader(html);
            XmlReader reader = XmlReader.Create(tr, settings);
            //doc.LoadXml(html);
            doc.Load(reader);
            var rowList = doc.DocumentElement.ChildNodes;
            if (rowList != null)
            {
                foreach (XmlNode rowNode in rowList)
                {
                    if (rowNode.Attributes["class"].Value.Contains("match-odds-title"))
                    {
                        continue;
                    }
                    LeagueMatch lm = new LeagueMatch();
                    lm.Name = rowNode.SelectSingleNode("h2/span").InnerText;
                    if (string.IsNullOrEmpty(lm.Name) || FilterLeagueMatch(1, lm.Name))
                    {
                        continue;
                    }
                    var bisaidiv = rowNode.ChildNodes;
                    foreach (XmlNode bisai in bisaidiv)
                    {
                        if (bisai.Name.ToUpper() == "H2")
                        {
                            continue;
                        }
                        Match m = new Match();
                        bool init = true;
                        foreach (XmlNode bisairow in bisai.ChildNodes)
                        {
                            if (init)
                            {
                                XmlNodeList timenode = bisairow.ChildNodes[0].SelectNodes("ul/li/span");
                                m.time = timenode[0].InnerText + " " + timenode[1].InnerText;
                                if (timenode.Count >= 3)
                                {
                                    m.GQ = timenode[2].InnerText;
                                }
                                XmlNodeList teamnode = bisairow.ChildNodes[1].SelectNodes("ul/li/span");
                                m.HomeTeam = teamnode[0].InnerText;
                                m.VisitingTeam = teamnode[1].InnerText;
                                m.statustext = "";
                            }
                            init = false;
                            //全场赔率================================================
                            Odds o1 = new Odds() { type = 1 };
                            //独赢
                            XmlNodeList duying1 = bisairow.ChildNodes[2].ChildNodes[0].SelectNodes("li/b");
                            if (duying1.Count == 3)
                            {
                                o1.Odds_ZY = duying1[0].InnerText;
                                o1.Odds_KY = duying1[1].InnerText;
                                o1.Odds_HJ = duying1[2].InnerText;
                                //o1.IsDisable_DY = "0";
                                //if (duying1[0].Attributes["class"].Value.Contains("odds-disable") || duying1[1].Attributes["class"].Value.Contains("odds-disable") || duying1[2].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DY = "1";
                                //}
                            }
                            //让球
                            XmlNodeList rangqiu1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/b");
                            if (rangqiu1.Count == 2)
                            {
                                o1.Odds_RQZY = rangqiu1[0].InnerText;
                                o1.Odds_RQKY = rangqiu1[1].InnerText;
                                //o1.IsDisable_RQ = "0";
                                //if (rangqiu1[0].Attributes["class"].Value.Contains("odds-disable") || rangqiu1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_RQ = "1";
                                //}
                            }
                            XmlNodeList rangqiutext1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/em");
                            if (rangqiutext1.Count == 2)
                            {
                                o1.Text_ZRKQ = rangqiutext1[0].InnerText;
                                o1.Text_KRZQ = rangqiutext1[1].InnerText;
                            }
                            //大小
                            XmlNodeList daxiao1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/b");
                            if (daxiao1.Count == 2)
                            {
                                o1.Odds_DQ = daxiao1[0].InnerText;
                                o1.Odds_XQ = daxiao1[1].InnerText;
                                //o1.IsDisable_DX = "0";
                                //if (daxiao1[0].Attributes["class"].Value.Contains("odds-disable") || daxiao1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DX = "1";
                                //}
                            }
                            XmlNodeList daxiaotext1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/em");
                            if (daxiaotext1.Count == 2)
                            {
                                o1.Text_DQ = daxiaotext1[0].InnerText;
                                o1.Text_XQ = daxiaotext1[1].InnerText;
                            }
                            //单双
                            XmlNodeList dansuang1 = bisairow.ChildNodes[2].ChildNodes[3].SelectNodes("li/b");
                            if (dansuang1.Count == 2)
                            {
                                o1.Odds_D = dansuang1[0].InnerText;
                                o1.Odds_S = dansuang1[1].InnerText;
                                //o1.IsDisable_DS = "0";
                                //if (dansuang1[0].Attributes["class"].Value.Contains("odds-disable") || dansuang1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DS = "1";
                                //}
                            }

                            m.FullCourtList.Add(o1);

                            //半场================================================
                            Odds o2 = new Odds() { type = 2 };
                            //独赢
                            XmlNodeList duying2 = bisairow.ChildNodes[3].ChildNodes[0].SelectNodes("li/b");
                            if (duying2.Count == 3)
                            {
                                o2.Odds_ZY = duying2[0].InnerText;
                                o2.Odds_KY = duying2[1].InnerText;
                                o2.Odds_HJ = duying2[2].InnerText;
                                //o2.IsDisable_DY = "0";
                                //if (duying2[0].Attributes["class"].Value.Contains("odds-disable") || duying2[1].Attributes["class"].Value.Contains("odds-disable") || duying2[2].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o2.IsDisable_DY = "1";
                                //}
                            }
                            //让球
                            XmlNodeList rangqiu2 = bisairow.ChildNodes[3].ChildNodes[1].SelectNodes("li/b");
                            if (rangqiu2.Count == 2)
                            {
                                o2.Odds_RQZY = rangqiu2[0].InnerText;
                                o2.Odds_RQKY = rangqiu2[1].InnerText;
                                //o2.IsDisable_RQ = "0";
                                //if (rangqiu2[0].Attributes["class"].Value.Contains("odds-disable") || rangqiu2[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o2.IsDisable_RQ = "1";
                                //}
                            }
                            XmlNodeList rangqiutext2 = bisairow.ChildNodes[3].ChildNodes[1].SelectNodes("li/em");
                            if (rangqiutext2.Count == 2)
                            {
                                o2.Text_ZRKQ = rangqiutext2[0].InnerText;
                                o2.Text_KRZQ = rangqiutext2[1].InnerText;
                            }
                            //大小
                            XmlNodeList daxiao2 = bisairow.ChildNodes[3].ChildNodes[2].SelectNodes("li/b");
                            if (daxiao2.Count == 2)
                            {
                                o2.Odds_DQ = daxiao2[0].InnerText;
                                o2.Odds_XQ = daxiao2[1].InnerText;
                                //o2.IsDisable_DX = "0";
                                //if (daxiao2[0].Attributes["class"].Value.Contains("odds-disable") || daxiao2[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o2.IsDisable_DX = "1";
                                //}
                            }
                            XmlNodeList daxiaotext2 = bisairow.ChildNodes[3].ChildNodes[2].SelectNodes("li/em");
                            if (daxiaotext2.Count == 2)
                            {
                                o2.Text_DQ = daxiaotext2[0].InnerText;
                                o2.Text_XQ = daxiaotext2[1].InnerText;
                            }

                            m.HalfCourtList.Add(o2);
                        }
                        lm.MatchList.Add(m);
                    }
                    lmList.Add(lm);
                }
            }
            return lmList;
        }
        /// <summary>
        /// 沙巴足球早盘列表，半场 / 全场
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<LeagueMatch> SB_Football_ZP_BCQC_List(string html)
        {
            html = html.Replace("&nbsp;", "");
            html = "<root>" + html + "</root>";
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            var doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            TextReader tr = new StringReader(html);
            XmlReader reader = XmlReader.Create(tr, settings);
            //doc.LoadXml(html);
            doc.Load(reader);
            var rowList = doc.DocumentElement.ChildNodes;
            if (rowList != null)
            {
                foreach (XmlNode rowNode in rowList)
                {
                    if (rowNode.Attributes["class"].Value.Contains("match-odds-title"))
                    {
                        continue;
                    }
                    LeagueMatch lm = new LeagueMatch();
                    lm.Name = rowNode.SelectSingleNode("h2/span").InnerText;
                    if (string.IsNullOrEmpty(lm.Name) || FilterLeagueMatch(1, lm.Name))
                    {
                        continue;
                    }
                    var bisaidiv = rowNode.ChildNodes;
                    foreach (XmlNode bisai in bisaidiv)
                    {
                        if (bisai.Name.ToUpper() == "H2")
                        {
                            continue;
                        }
                        Match m = new Match();
                        foreach (XmlNode bisairow in bisai.ChildNodes)
                        {
                            XmlNodeList timenode = bisairow.ChildNodes[0].SelectNodes("ul/li/span");
                            m.time = timenode[0].InnerText + " " + timenode[1].InnerText;
                            if (timenode.Count >= 3)
                            {
                                m.GQ = timenode[2].InnerText;
                            }
                            XmlNodeList teamnode = bisairow.ChildNodes[1].SelectNodes("ul/li/span");
                            m.HomeTeam = teamnode[0].InnerText;
                            m.VisitingTeam = teamnode[1].InnerText;
                            m.statustext = "";

                            //全场赔率================================================
                            OddsBCQC bcqc = new OddsBCQC();

                            XmlNodeList bettype = bisairow.ChildNodes[2].ChildNodes[0].SelectNodes("li/b");
                            if (bettype.Count == 9)
                            {
                                bcqc.Odds_HH = bettype[0].InnerText;
                                bcqc.Odds_HD = bettype[1].InnerText;
                                bcqc.Odds_HV = bettype[2].InnerText;
                                bcqc.Odds_DH = bettype[3].InnerText;
                                bcqc.Odds_DD = bettype[4].InnerText;
                                bcqc.Odds_DV = bettype[5].InnerText;
                                bcqc.Odds_VH = bettype[6].InnerText;
                                bcqc.Odds_VD = bettype[7].InnerText;
                                bcqc.Odds_VV = bettype[8].InnerText;
                                //if (bettype[0].Attributes["class"].Value.Contains("odds-disable") ||
                                //    bettype[1].Attributes["class"].Value.Contains("odds-disable") ||
                                //    bettype[2].Attributes["class"].Value.Contains("odds-disable") ||
                                //    bettype[3].Attributes["class"].Value.Contains("odds-disable") ||
                                //    bettype[4].Attributes["class"].Value.Contains("odds-disable") ||
                                //    bettype[5].Attributes["class"].Value.Contains("odds-disable") ||
                                //    bettype[6].Attributes["class"].Value.Contains("odds-disable") ||
                                //    bettype[7].Attributes["class"].Value.Contains("odds-disable") ||
                                //    bettype[8].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    bcqc.IsDisable = "1";
                                //}
                                //else
                                //{
                                //    bcqc.IsDisable = "0";
                                //}
                            }

                            m.DoubleResult = bcqc;
                        }
                        lm.MatchList.Add(m);
                    }
                    lmList.Add(lm);
                }
            }
            return lmList;
        }
        /// <summary>
        /// 沙巴足球早盘列表，波胆
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<LeagueMatch> SB_Football_ZP_BD_List(string html)
        {
            html = html.Replace("&nbsp;", "");
            html = "<root>" + html + "</root>";
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            var doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            TextReader tr = new StringReader(html);
            XmlReader reader = XmlReader.Create(tr, settings);
            //doc.LoadXml(html);
            doc.Load(reader);
            var rowList = doc.DocumentElement.ChildNodes;
            if (rowList != null)
            {
                foreach (XmlNode rowNode in rowList)
                {
                    if (rowNode.Attributes["class"].Value.Contains("match-odds-title"))
                    {
                        continue;
                    }
                    LeagueMatch lm = new LeagueMatch();
                    lm.Name = rowNode.SelectSingleNode("h2/span").InnerText;
                    if (string.IsNullOrEmpty(lm.Name) || FilterLeagueMatch(1, lm.Name))
                    {
                        continue;
                    }
                    var bisaidiv = rowNode.ChildNodes;
                    foreach (XmlNode bisai in bisaidiv)
                    {
                        if (bisai.Name.ToUpper() == "H2")
                        {
                            continue;
                        }
                        Match m = new Match();
                        foreach (XmlNode bisairow in bisai.ChildNodes)
                        {
                            XmlNodeList timenode = bisairow.ChildNodes[0].SelectNodes("ul/li/span");
                            m.time = timenode[0].InnerText + " " + timenode[1].InnerText;
                            if (timenode.Count >= 3)
                            {
                                m.GQ = timenode[2].InnerText;
                            }
                            XmlNodeList teamnode = bisairow.ChildNodes[1].SelectNodes("ul/li/span");
                            m.HomeTeam = teamnode[0].InnerText;
                            m.VisitingTeam = teamnode[1].InnerText;
                            m.statustext = "";

                            //波胆================================================ 
                            XmlNodeList bettype = bisairow.ChildNodes[2].ChildNodes;//.SelectNodes("li/b");
                            if (bettype.Count == 16)
                            {
                                int[,] bfarr = new int[,]
                                {
                                    {1,0 },
                                    {2,0 },
                                    {2,1 },
                                    {3,0 },
                                    {3,1 },
                                    {3,2 },
                                    {4,0 },
                                    {4,1 },
                                    {4,2 },
                                    {4,3 },
                                    {0,0 },
                                    {1,1 },
                                    {2,2 },
                                    {3,3 },
                                    {4,4 },
                                    {-1,-1 },
                                };
                                int i = 0;
                                foreach (XmlNode ul in bettype)
                                {
                                    XmlNodeList b = ul.SelectNodes("li/b");
                                    if (i >= 10)
                                    {
                                        OddsBD bd = new OddsBD() { type = 1 };
                                        bd.Text_H = bfarr[i, 0];
                                        bd.Text_V = bfarr[i, 1];
                                        bd.Odds_BD = b[0].InnerText;
                                        //if (b[0].Attributes["class"].Value.Contains("odds-disable"))
                                        //{
                                        //    bd.IsDisable = "1";
                                        //}
                                        //else
                                        //{
                                        //    bd.IsDisable = "0";
                                        //}

                                        m.CorrectScoreList.Add(bd);
                                    }
                                    else
                                    {
                                        OddsBD bd1 = new OddsBD() { type = 1 };
                                        bd1.Text_H = bfarr[i, 0];
                                        bd1.Text_V = bfarr[i, 1];
                                        bd1.Odds_BD = b[0].InnerText;
                                        OddsBD bd2 = new OddsBD() { type = 1 };
                                        bd2.Text_H = bfarr[i, 1];
                                        bd2.Text_V = bfarr[i, 0];
                                        bd2.Odds_BD = b[1].InnerText;
                                        //if (b[0].Attributes["class"].Value.Contains("odds-disable"))
                                        //{
                                        //    bd1.IsDisable = "1";
                                        //}
                                        //else
                                        //{
                                        //    bd1.IsDisable = "0";
                                        //}
                                        //if (b[1].Attributes["class"].Value.Contains("odds-disable"))
                                        //{
                                        //    bd2.IsDisable = "1";
                                        //}
                                        //else
                                        //{
                                        //    bd2.IsDisable = "0";
                                        //}

                                        m.CorrectScoreList.Add(bd1);
                                        m.CorrectScoreList.Add(bd2);
                                    }
                                    i++;
                                }
                            }

                        }
                        lm.MatchList.Add(m);
                    }
                    lmList.Add(lm);
                }
            }
            return lmList;
        }
        /// <summary>
        /// 沙巴足球早盘列表，总入球
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<LeagueMatch> SB_Football_ZP_ZRQ_List(string html)
        {
            html = html.Replace("&nbsp;", "");
            html = "<root>" + html + "</root>";
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            var doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            TextReader tr = new StringReader(html);
            XmlReader reader = XmlReader.Create(tr, settings);
            //doc.LoadXml(html);
            doc.Load(reader);
            var rowList = doc.DocumentElement.ChildNodes;
            if (rowList != null)
            {
                foreach (XmlNode rowNode in rowList)
                {
                    if (rowNode.Attributes["class"].Value.Contains("match-odds-title"))
                    {
                        continue;
                    }
                    LeagueMatch lm = new LeagueMatch();
                    lm.Name = rowNode.SelectSingleNode("h2/span").InnerText;
                    if (string.IsNullOrEmpty(lm.Name) || FilterLeagueMatch(1, lm.Name))
                    {
                        continue;
                    }
                    var bisaidiv = rowNode.ChildNodes;
                    foreach (XmlNode bisai in bisaidiv)
                    {
                        if (bisai.Name.ToUpper() == "H2")
                        {
                            continue;
                        }
                        Match m = new Match();
                        foreach (XmlNode bisairow in bisai.ChildNodes)
                        {
                            XmlNodeList timenode = bisairow.ChildNodes[0].SelectNodes("ul/li/span");
                            m.time = timenode[0].InnerText + " " + timenode[1].InnerText;
                            if (timenode.Count >= 3)
                            {
                                m.GQ = timenode[2].InnerText;
                            }
                            XmlNodeList teamnode = bisairow.ChildNodes[1].SelectNodes("ul/li/span");
                            m.HomeTeam = teamnode[0].InnerText;
                            m.VisitingTeam = teamnode[1].InnerText;
                            m.statustext = "";
                            //总入球================================================                            

                            XmlNodeList bettype = bisairow.ChildNodes[2].ChildNodes[0].SelectNodes("li/b");
                            if (bettype.Count == 4)
                            {
                                string[] goals = new string[] { "0-1", "2-3", "4-6", "7+" };
                                for (int i = 0; i < bettype.Count; i++)
                                {
                                    OddsZRQ zrq = new OddsZRQ() { type = 1 };
                                    zrq.Text_Goals = goals[i];
                                    zrq.Odds_ZRQ = bettype[i].InnerText;
                                    //if (bettype[i].Attributes["class"].Value.Contains("odds-disable"))
                                    //{
                                    //    zrq.IsDisable = "1";
                                    //}
                                    //else
                                    //{
                                    //    zrq.IsDisable = "0";
                                    //}
                                    m.TotalGoalList.Add(zrq);
                                }
                            }

                        }
                        lm.MatchList.Add(m);
                    }
                    lmList.Add(lm);
                }
            }
            return lmList;
        }
        /// <summary>
        /// 沙巴足球早盘列表，冠军
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<LeagueMatch> SB_Football_ZP_GJ_List(string html)
        {
            html = html.Replace("&nbsp;", "");
            html = "<root>" + html + "</root>";
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            var doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            TextReader tr = new StringReader(html);
            XmlReader reader = XmlReader.Create(tr, settings);
            //doc.LoadXml(html);
            doc.Load(reader);
            var rowList = doc.DocumentElement.ChildNodes;
            if (rowList != null)
            {
                foreach (XmlNode rowNode in rowList)
                {
                    if (rowNode.Attributes["class"].Value.Contains("match-odds-title"))
                    {
                        continue;
                    }
                    LeagueMatch lm = new LeagueMatch();
                    lm.Name = rowNode.SelectSingleNode("h2/span").InnerText.Replace(" - 冠军", "");
                    if (string.IsNullOrEmpty(lm.Name) || FilterLeagueMatch(1, lm.Name))
                    {
                        continue;
                    }
                    var teamdiv = rowNode.ChildNodes;
                    foreach (XmlNode team in teamdiv)
                    {
                        if (team.Name.ToUpper() == "H2")
                        {
                            continue;
                        }
                        Team m = new Team();
                        m.Name = team.SelectSingleNode("div/div[1]/ul/li/span").InnerText;
                        if (!string.IsNullOrEmpty(m.Name))
                        {
                            m.OutrightOdds = team.SelectSingleNode("div/div[2]/ul/li/b").InnerText;
                            lm.TeamList.Add(m);
                        }
                    }
                    lmList.Add(lm);
                }
            }
            return lmList;
        }
        /// <summary>
        /// 沙巴足球滚球列表，独赢 & 让球 & 大小 & 单/双
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<LeagueMatch> SB_Football_GQ_DRDD_List(string html)
        {
            html = html.Replace("&nbsp;", "");
            html = "<root>" + html + "</root>";
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            var doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            TextReader tr = new StringReader(html);
            XmlReader reader = XmlReader.Create(tr, settings);
            //doc.LoadXml(html);
            doc.Load(reader);
            var rowList = doc.DocumentElement.ChildNodes;
            if (rowList != null)
            {
                foreach (XmlNode rowNode in rowList)
                {
                    if (rowNode.Attributes["class"].Value.Contains("match-odds-title"))
                    {
                        continue;
                    }
                    LeagueMatch lm = new LeagueMatch();
                    lm.Name = rowNode.SelectSingleNode("h2/span").InnerText;
                    if (string.IsNullOrEmpty(lm.Name) || FilterLeagueMatch(1, lm.Name))
                    {
                        continue;
                    }
                    var bisaidiv = rowNode.ChildNodes;
                    foreach (XmlNode bisai in bisaidiv)
                    {
                        if (bisai.Name.ToUpper() == "H2")
                        {
                            continue;
                        }
                        Match m = new Match();
                        m.statustext = "";
                        bool init = true;
                        foreach (XmlNode bisairow in bisai.ChildNodes)
                        {
                            if (init)
                            {
                                XmlNodeList scorenode = bisairow.ChildNodes[0].SelectNodes("ul/li/span");
                                if (scorenode.Count >= 4)
                                {
                                    string temp = scorenode[0].InnerText.Trim();
                                    if (temp.Contains("'"))
                                    {
                                        int ts = -1;
                                        string[] tsarr = temp.Split(' ');
                                        int.TryParse(tsarr[1].Trim('\''), out ts);
                                        if (tsarr[0].Contains("H"))
                                        {
                                            if (ts == -1)
                                            {
                                                ts = 1;
                                            }
                                        }
                                        m.timing = ts.ToString();
                                        m.halftype = tsarr[0];
                                    }
                                    else
                                    {
                                        if (temp == "半场")//延迟，滚球
                                        {
                                            //m.timing = "45";
                                            m.halftype = "1H";
                                        }
                                        m.timing = "-1";
                                        m.statustext = temp;
                                    }
                                    //m.timing = scorenode[0].InnerText;
                                    m.HomeTeamScore = scorenode[1].InnerText;
                                    m.VisitingTeamScore = scorenode[3].InnerText;
                                }
                                XmlNodeList teamnode = bisairow.ChildNodes[1].SelectNodes("ul/li/span");
                                m.HomeTeam = teamnode[0].InnerText;
                                m.VisitingTeam = teamnode[1].InnerText;
                            }
                            init = false;
                            //全场赔率================================================
                            Odds o1 = new Odds() { type = 1 };
                            //独赢
                            XmlNodeList duying1 = bisairow.ChildNodes[2].ChildNodes[0].SelectNodes("li/b");
                            if (duying1.Count == 3)
                            {
                                o1.Odds_ZY = duying1[0].InnerText;
                                o1.Odds_KY = duying1[1].InnerText;
                                o1.Odds_HJ = duying1[2].InnerText;
                                //o1.IsDisable_DY = "0";
                                //if (duying1[0].Attributes["class"].Value.Contains("odds-disable") || duying1[1].Attributes["class"].Value.Contains("odds-disable") || duying1[2].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DY = "1";
                                //}
                            }
                            //让球
                            XmlNodeList rangqiu1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/b");
                            if (rangqiu1.Count == 2)
                            {
                                o1.Odds_RQZY = rangqiu1[0].InnerText;
                                o1.Odds_RQKY = rangqiu1[1].InnerText;
                                //o1.IsDisable_RQ = "0";
                                //if (rangqiu1[0].Attributes["class"].Value.Contains("odds-disable") || rangqiu1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_RQ = "1";
                                //}
                            }
                            XmlNodeList rangqiutext1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/em");
                            if (rangqiutext1.Count == 2)
                            {
                                o1.Text_ZRKQ = rangqiutext1[0].InnerText;
                                o1.Text_KRZQ = rangqiutext1[1].InnerText;
                            }
                            //大小
                            XmlNodeList daxiao1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/b");
                            if (daxiao1.Count == 2)
                            {
                                o1.Odds_DQ = daxiao1[0].InnerText;
                                o1.Odds_XQ = daxiao1[1].InnerText;
                                //o1.IsDisable_DX = "0";
                                //if (daxiao1[0].Attributes["class"].Value.Contains("odds-disable") || daxiao1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DX = "1";
                                //}
                            }
                            XmlNodeList daxiaotext1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/em");
                            if (daxiaotext1.Count == 2)
                            {
                                o1.Text_DQ = daxiaotext1[0].InnerText;
                                o1.Text_XQ = daxiaotext1[1].InnerText;
                            }
                            //单双
                            XmlNodeList dansuang1 = bisairow.ChildNodes[2].ChildNodes[3].SelectNodes("li/b");
                            if (dansuang1.Count == 2)
                            {
                                o1.Odds_D = dansuang1[0].InnerText;
                                o1.Odds_S = dansuang1[1].InnerText;
                                //o1.IsDisable_DS = "0";
                                //if (dansuang1[0].Attributes["class"].Value.Contains("odds-disable") || dansuang1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DS = "1";
                                //}
                            }

                            m.FullCourtList.Add(o1);

                            //半场================================================
                            Odds o2 = new Odds() { type = 2 };
                            //独赢
                            XmlNodeList duying2 = bisairow.ChildNodes[3].ChildNodes[0].SelectNodes("li/b");
                            if (duying2.Count == 3)
                            {
                                o2.Odds_ZY = duying2[0].InnerText;
                                o2.Odds_KY = duying2[1].InnerText;
                                o2.Odds_HJ = duying2[2].InnerText;
                                //o2.IsDisable_DY = "0";
                                //if (duying2[0].Attributes["class"].Value.Contains("odds-disable") || duying2[1].Attributes["class"].Value.Contains("odds-disable") || duying2[2].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o2.IsDisable_DY = "1";
                                //}
                            }
                            //让球
                            XmlNodeList rangqiu2 = bisairow.ChildNodes[3].ChildNodes[1].SelectNodes("li/b");
                            if (rangqiu2.Count == 2)
                            {
                                o2.Odds_RQZY = rangqiu2[0].InnerText;
                                o2.Odds_RQKY = rangqiu2[1].InnerText;
                                //o2.IsDisable_RQ = "0";
                                //if (rangqiu2[0].Attributes["class"].Value.Contains("odds-disable") || rangqiu2[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o2.IsDisable_RQ = "1";
                                //}
                            }
                            XmlNodeList rangqiutext2 = bisairow.ChildNodes[3].ChildNodes[1].SelectNodes("li/em");
                            if (rangqiutext2.Count == 2)
                            {
                                o2.Text_ZRKQ = rangqiutext2[0].InnerText;
                                o2.Text_KRZQ = rangqiutext2[1].InnerText;
                            }
                            //大小
                            XmlNodeList daxiao2 = bisairow.ChildNodes[3].ChildNodes[2].SelectNodes("li/b");
                            if (daxiao2.Count == 2)
                            {
                                o2.Odds_DQ = daxiao2[0].InnerText;
                                o2.Odds_XQ = daxiao2[1].InnerText;
                                //o2.IsDisable_DX = "0";
                                //if (daxiao2[0].Attributes["class"].Value.Contains("odds-disable") || daxiao2[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o2.IsDisable_DX = "1";
                                //}
                            }
                            XmlNodeList daxiaotext2 = bisairow.ChildNodes[3].ChildNodes[2].SelectNodes("li/em");
                            if (daxiaotext2.Count == 2)
                            {
                                o2.Text_DQ = daxiaotext2[0].InnerText;
                                o2.Text_XQ = daxiaotext2[1].InnerText;
                            }

                            m.HalfCourtList.Add(o2);
                        }
                        lm.MatchList.Add(m);
                    }
                    lmList.Add(lm);
                }
            }
            return lmList;
        }
        /// <summary>
        /// 沙巴足球滚球列表，波胆
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<LeagueMatch> SB_Football_GQ_BD_List(string html)
        {
            html = html.Replace("&nbsp;", "");
            html = "<root>" + html + "</root>";
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            var doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            TextReader tr = new StringReader(html);
            XmlReader reader = XmlReader.Create(tr, settings);
            //doc.LoadXml(html);
            doc.Load(reader);
            var rowList = doc.DocumentElement.ChildNodes;
            if (rowList != null)
            {
                foreach (XmlNode rowNode in rowList)
                {
                    if (rowNode.Attributes["class"].Value.Contains("match-odds-title"))
                    {
                        continue;
                    }
                    LeagueMatch lm = new LeagueMatch();
                    lm.Name = rowNode.SelectSingleNode("h2/span").InnerText;
                    if (string.IsNullOrEmpty(lm.Name) || FilterLeagueMatch(1, lm.Name))
                    {
                        continue;
                    }
                    var bisaidiv = rowNode.ChildNodes;
                    foreach (XmlNode bisai in bisaidiv)
                    {
                        if (bisai.Name.ToUpper() == "H2")
                        {
                            continue;
                        }
                        Match m = new Match();
                        m.statustext = "";
                        foreach (XmlNode bisairow in bisai.ChildNodes)
                        {
                            XmlNodeList scorenode = bisairow.ChildNodes[0].SelectNodes("ul/li/span");
                            if (scorenode.Count >= 4)
                            {
                                string temp = scorenode[0].InnerText.Trim();
                                if (temp.Contains("'"))
                                {
                                    int ts = -1;
                                    string[] tsarr = temp.Split(' ');
                                    int.TryParse(tsarr[1].Trim('\''), out ts);
                                    if (tsarr[0].Contains("H"))
                                    {
                                        if (ts == -1)
                                        {
                                            ts = 1;
                                        }
                                    }
                                    m.timing = ts.ToString();
                                    m.halftype = tsarr[0];
                                }
                                else
                                {
                                    if (temp == "半场")//延迟，滚球
                                    {
                                        //m.timing = "45";
                                        m.halftype = "1H";
                                    }
                                    else
                                    {
                                        m.timing = "-1";
                                    }
                                    m.statustext = temp;
                                }
                                //m.timing = scorenode[0].InnerText;
                                m.HomeTeamScore = scorenode[1].InnerText;
                                m.VisitingTeamScore = scorenode[3].InnerText;
                            }
                            XmlNodeList teamnode = bisairow.ChildNodes[1].SelectNodes("ul/li/span");
                            m.HomeTeam = teamnode[0].InnerText;
                            m.VisitingTeam = teamnode[1].InnerText;

                            //波胆================================================ 
                            XmlNodeList bettype = bisairow.ChildNodes[2].ChildNodes;//.SelectNodes("li/b");
                            if (bettype.Count == 16)
                            {
                                int[,] bfarr = new int[,]
                                {
                                    {1,0 },
                                    {2,0 },
                                    {2,1 },
                                    {3,0 },
                                    {3,1 },
                                    {3,2 },
                                    {4,0 },
                                    {4,1 },
                                    {4,2 },
                                    {4,3 },
                                    {0,0 },
                                    {1,1 },
                                    {2,2 },
                                    {3,3 },
                                    {4,4 },
                                    {-1,-1 },
                                };
                                int i = 0;
                                foreach (XmlNode ul in bettype)
                                {
                                    XmlNodeList b = ul.SelectNodes("li/b");
                                    if (i >= 10)
                                    {
                                        OddsBD bd = new OddsBD() { type = 1 };
                                        bd.Text_H = bfarr[i, 0];
                                        bd.Text_V = bfarr[i, 1];
                                        bd.Odds_BD = b[0].InnerText;
                                        //if (b[0].Attributes["class"].Value.Contains("odds-disable"))
                                        //{
                                        //    bd.IsDisable = "1";
                                        //}
                                        //else
                                        //{
                                        //    bd.IsDisable = "0";
                                        //}

                                        m.CorrectScoreList.Add(bd);
                                    }
                                    else
                                    {
                                        OddsBD bd1 = new OddsBD() { type = 1 };
                                        bd1.Text_H = bfarr[i, 0];
                                        bd1.Text_V = bfarr[i, 1];
                                        bd1.Odds_BD = b[0].InnerText;
                                        OddsBD bd2 = new OddsBD() { type = 1 };
                                        bd2.Text_H = bfarr[i, 1];
                                        bd2.Text_V = bfarr[i, 0];
                                        bd2.Odds_BD = b[1].InnerText;
                                        //if (b[0].Attributes["class"].Value.Contains("odds-disable"))
                                        //{
                                        //    bd1.IsDisable = "1";
                                        //}
                                        //else
                                        //{
                                        //    bd1.IsDisable = "0";
                                        //}
                                        //if (b[1].Attributes["class"].Value.Contains("odds-disable"))
                                        //{
                                        //    bd2.IsDisable = "1";
                                        //}
                                        //else
                                        //{
                                        //    bd2.IsDisable = "0";
                                        //}

                                        m.CorrectScoreList.Add(bd1);
                                        m.CorrectScoreList.Add(bd2);
                                    }
                                    i++;
                                }
                            }

                        }
                        lm.MatchList.Add(m);
                    }
                    lmList.Add(lm);
                }
            }
            return lmList;
        }
        /// <summary>
        /// 沙巴足球滚球列表，总入球
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<LeagueMatch> SB_Football_GQ_ZRQ_List(string html)
        {
            html = html.Replace("&nbsp;", "");
            html = "<root>" + html + "</root>";
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            var doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            TextReader tr = new StringReader(html);
            XmlReader reader = XmlReader.Create(tr, settings);
            //doc.LoadXml(html);
            doc.Load(reader);
            var rowList = doc.DocumentElement.ChildNodes;
            if (rowList != null)
            {
                foreach (XmlNode rowNode in rowList)
                {
                    if (rowNode.Attributes["class"].Value.Contains("match-odds-title"))
                    {
                        continue;
                    }
                    LeagueMatch lm = new LeagueMatch();
                    lm.Name = rowNode.SelectSingleNode("h2/span").InnerText;
                    if (string.IsNullOrEmpty(lm.Name) || FilterLeagueMatch(1, lm.Name))
                    {
                        continue;
                    }
                    var bisaidiv = rowNode.ChildNodes;
                    foreach (XmlNode bisai in bisaidiv)
                    {
                        if (bisai.Name.ToUpper() == "H2")
                        {
                            continue;
                        }
                        Match m = new Match();
                        m.statustext = "";
                        foreach (XmlNode bisairow in bisai.ChildNodes)
                        {
                            XmlNodeList scorenode = bisairow.ChildNodes[0].SelectNodes("ul/li/span");
                            if (scorenode.Count >= 4)
                            {
                                string temp = scorenode[0].InnerText.Trim();
                                if (temp.Contains("'"))
                                {
                                    int ts = -1;
                                    string[] tsarr = temp.Split(' ');
                                    int.TryParse(tsarr[1].Trim('\''), out ts);
                                    if (tsarr[0].Contains("H"))
                                    {
                                        if (ts == -1)
                                        {
                                            ts = 1;
                                        }
                                    }
                                    m.timing = ts.ToString();
                                    m.halftype = tsarr[0];
                                }
                                else
                                {
                                    if (temp == "半场")//延迟，滚球
                                    {
                                        //m.timing = "45";
                                        m.halftype = "1H";
                                    }
                                    else
                                    {
                                        m.timing = "-1";
                                    }
                                    m.statustext = temp;
                                }
                                //m.timing = scorenode[0].InnerText;
                                m.HomeTeamScore = scorenode[1].InnerText;
                                m.VisitingTeamScore = scorenode[3].InnerText;
                            }
                            XmlNodeList teamnode = bisairow.ChildNodes[1].SelectNodes("ul/li/span");
                            m.HomeTeam = teamnode[0].InnerText;
                            m.VisitingTeam = teamnode[1].InnerText;

                            //总入球================================================                            

                            XmlNodeList bettype = bisairow.ChildNodes[2].ChildNodes[0].SelectNodes("li/b");
                            if (bettype.Count == 4)
                            {
                                string[] goals = new string[] { "0-1", "2-3", "4-6", "7+" };
                                for (int i = 0; i < bettype.Count; i++)
                                {
                                    OddsZRQ zrq = new OddsZRQ() { type = 1 };
                                    zrq.Text_Goals = goals[i];
                                    zrq.Odds_ZRQ = bettype[i].InnerText;
                                    //if (bettype[i].Attributes["class"].Value.Contains("odds-disable"))
                                    //{
                                    //    zrq.IsDisable = "1";
                                    //}
                                    //else
                                    //{
                                    //    zrq.IsDisable = "0";
                                    //}
                                    m.TotalGoalList.Add(zrq);
                                }
                            }
                        }
                        lm.MatchList.Add(m);
                    }
                    lmList.Add(lm);
                }
            }
            return lmList;
        }
        #endregion

        #region 篮球
        /// <summary>
        /// 沙巴篮球早盘列表，胜负盘&让球&大小&单/双
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<LeagueMatch> SB_Basketball_ZP_DRDD_List(string html)
        {
            html = html.Replace("&nbsp;", "");
            html = "<root>" + html + "</root>";
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            var doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            TextReader tr = new StringReader(html);
            XmlReader reader = XmlReader.Create(tr, settings);
            //doc.LoadXml(html);
            doc.Load(reader);
            var rowList = doc.DocumentElement.ChildNodes;
            if (rowList != null)
            {
                foreach (XmlNode rowNode in rowList)
                {
                    if (rowNode.Attributes["class"].Value.Contains("match-odds-title"))
                    {
                        continue;
                    }
                    LeagueMatch lm = new LeagueMatch();
                    lm.Name = rowNode.SelectSingleNode("h2/span").InnerText;
                    if (string.IsNullOrEmpty(lm.Name) || FilterLeagueMatch(2, lm.Name))
                    {
                        continue;
                    }
                    var bisaidiv = rowNode.ChildNodes;
                    foreach (XmlNode bisai in bisaidiv)
                    {
                        if (bisai.Name.ToUpper() == "H2")
                        {
                            continue;
                        }
                        Match m = new Match();
                        bool init = true;
                        foreach (XmlNode bisairow in bisai.ChildNodes)
                        {
                            if (init)
                            {
                                XmlNodeList timenode = bisairow.ChildNodes[0].SelectNodes("ul/li/span");
                                m.time = timenode[0].InnerText + " " + timenode[1].InnerText;
                                if (timenode.Count >= 3)
                                {
                                    m.GQ = timenode[2].InnerText;
                                }
                                XmlNodeList teamnode = bisairow.ChildNodes[1].SelectNodes("ul/li/span");
                                m.HomeTeam = teamnode[0].InnerText;
                                m.VisitingTeam = teamnode[1].InnerText;
                                m.statustext = "";
                            }
                            if (init || bisairow.ChildNodes[0].ChildNodes.Count == 0)
                            {
                                init = false;
                                //全场赔率================================================
                                Odds o1 = new Odds() { type = 1 };
                                //独赢
                                XmlNodeList duying1 = bisairow.ChildNodes[2].ChildNodes[0].SelectNodes("li/b");
                                if (duying1.Count == 2)
                                {
                                    o1.Odds_ZY = duying1[0].InnerText;
                                    o1.Odds_KY = duying1[1].InnerText;
                                    //o1.IsDisable_DY = "0";
                                    //if (duying1[0].Attributes["class"].Value.Contains("odds-disable") || duying1[1].Attributes["class"].Value.Contains("odds-disable") || duying1[2].Attributes["class"].Value.Contains("odds-disable"))
                                    //{
                                    //    o1.IsDisable_DY = "1";
                                    //}
                                }
                                //让球
                                XmlNodeList rangqiu1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/b");
                                if (rangqiu1.Count == 2)
                                {
                                    o1.Odds_RQZY = rangqiu1[0].InnerText;
                                    o1.Odds_RQKY = rangqiu1[1].InnerText;
                                    //o1.IsDisable_RQ = "0";
                                    //if (rangqiu1[0].Attributes["class"].Value.Contains("odds-disable") || rangqiu1[1].Attributes["class"].Value.Contains("odds-disable"))
                                    //{
                                    //    o1.IsDisable_RQ = "1";
                                    //}
                                }
                                XmlNodeList rangqiutext1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/em");
                                if (rangqiutext1.Count == 2)
                                {
                                    o1.Text_ZRKQ = rangqiutext1[0].InnerText;
                                    o1.Text_KRZQ = rangqiutext1[1].InnerText;
                                }
                                //大小
                                XmlNodeList daxiao1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/b");
                                if (daxiao1.Count == 2)
                                {
                                    o1.Odds_DQ = daxiao1[0].InnerText;
                                    o1.Odds_XQ = daxiao1[1].InnerText;
                                    //o1.IsDisable_DX = "0";
                                    //if (daxiao1[0].Attributes["class"].Value.Contains("odds-disable") || daxiao1[1].Attributes["class"].Value.Contains("odds-disable"))
                                    //{
                                    //    o1.IsDisable_DX = "1";
                                    //}
                                }
                                XmlNodeList daxiaotext1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/em");
                                if (daxiaotext1.Count == 2)
                                {
                                    o1.Text_DQ = daxiaotext1[0].InnerText;
                                    o1.Text_XQ = daxiaotext1[1].InnerText;
                                }
                                //单双
                                XmlNodeList dansuang1 = bisairow.ChildNodes[2].ChildNodes[3].SelectNodes("li/b");
                                if (dansuang1.Count == 2)
                                {
                                    o1.Odds_D = dansuang1[0].InnerText;
                                    o1.Odds_S = dansuang1[1].InnerText;
                                    //o1.IsDisable_DS = "0";
                                    //if (dansuang1[0].Attributes["class"].Value.Contains("odds-disable") || dansuang1[1].Attributes["class"].Value.Contains("odds-disable"))
                                    //{
                                    //    o1.IsDisable_DS = "1";
                                    //}
                                }
                                //主队得分大小
                                if (bisairow.ChildNodes[3].HasChildNodes)
                                {
                                    XmlNodeList zdfdx = bisairow.ChildNodes[3].ChildNodes[0].SelectNodes("li/b");
                                    if (zdfdx.Count == 2)
                                    {
                                        o1.Odds_DQZ = zdfdx[0].InnerText;
                                        o1.Odds_XQZ = zdfdx[1].InnerText;
                                    }
                                    XmlNodeList zdfdxtext = bisairow.ChildNodes[3].ChildNodes[0].SelectNodes("li/em");
                                    if (zdfdxtext.Count == 2)
                                    {
                                        o1.Text_DQZ = zdfdxtext[0].InnerText;
                                        o1.Text_XQZ = zdfdxtext[1].InnerText;
                                    }
                                    //客队得分大小
                                    XmlNodeList kdfdx = bisairow.ChildNodes[3].ChildNodes[1].SelectNodes("li/b");
                                    if (kdfdx.Count == 2)
                                    {
                                        o1.Odds_DQK = kdfdx[0].InnerText;
                                        o1.Odds_XQK = kdfdx[1].InnerText;
                                    }
                                    XmlNodeList kdfdxtext = bisairow.ChildNodes[3].ChildNodes[1].SelectNodes("li/em");
                                    if (kdfdxtext.Count == 2)
                                    {
                                        o1.Text_DQK = kdfdxtext[0].InnerText;
                                        o1.Text_XQK = kdfdxtext[1].InnerText;
                                    }
                                }                                

                                m.FullCourtList.Add(o1);
                            }
                            else
                            {
                                int sectiontype = 0;
                                string section = bisairow.ChildNodes[0].SelectSingleNode("ul/li/div/span").InnerText;
                                switch (section)
                                {
                                    case "上半场":
                                        sectiontype = 2;
                                        break;
                                    case "下半场":
                                        sectiontype = 3;
                                        break;
                                    case "第一节":
                                        sectiontype = 4;
                                        break;
                                    case "第二节":
                                        sectiontype = 5;
                                        break;
                                    case "第三节":
                                        sectiontype = 6;
                                        break;
                                    case "第四节":
                                        sectiontype = 7;
                                        break;
                                }
                                if (sectiontype == 0)
                                {
                                    continue;
                                }
                                //半场================================================
                                Odds o2 = new Odds() { type = sectiontype };
                                //独赢
                                XmlNodeList duying2 = bisairow.ChildNodes[2].ChildNodes[0].SelectNodes("li/b");
                                if (duying2.Count == 2)
                                {
                                    o2.Odds_ZY = duying2[0].InnerText;
                                    o2.Odds_KY = duying2[1].InnerText;
                                    //o2.IsDisable_DY = "0";
                                    //if (duying2[0].Attributes["class"].Value.Contains("odds-disable") || duying2[1].Attributes["class"].Value.Contains("odds-disable") || duying2[2].Attributes["class"].Value.Contains("odds-disable"))
                                    //{
                                    //    o2.IsDisable_DY = "1";
                                    //}
                                }
                                //让球
                                XmlNodeList rangqiu2 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/b");
                                if (rangqiu2.Count == 2)
                                {
                                    o2.Odds_RQZY = rangqiu2[0].InnerText;
                                    o2.Odds_RQKY = rangqiu2[1].InnerText;
                                    //o2.IsDisable_RQ = "0";
                                    //if (rangqiu2[0].Attributes["class"].Value.Contains("odds-disable") || rangqiu2[1].Attributes["class"].Value.Contains("odds-disable"))
                                    //{
                                    //    o2.IsDisable_RQ = "1";
                                    //}
                                }
                                XmlNodeList rangqiutext2 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/em");
                                if (rangqiutext2.Count == 2)
                                {
                                    o2.Text_ZRKQ = rangqiutext2[0].InnerText;
                                    o2.Text_KRZQ = rangqiutext2[1].InnerText;
                                }
                                //大小
                                XmlNodeList daxiao2 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/b");
                                if (daxiao2.Count == 2)
                                {
                                    o2.Odds_DQ = daxiao2[0].InnerText;
                                    o2.Odds_XQ = daxiao2[1].InnerText;
                                    //o2.IsDisable_DX = "0";
                                    //if (daxiao2[0].Attributes["class"].Value.Contains("odds-disable") || daxiao2[1].Attributes["class"].Value.Contains("odds-disable"))
                                    //{
                                    //    o2.IsDisable_DX = "1";
                                    //}
                                }
                                XmlNodeList daxiaotext2 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/em");
                                if (daxiaotext2.Count == 2)
                                {
                                    o2.Text_DQ = daxiaotext2[0].InnerText;
                                    o2.Text_XQ = daxiaotext2[1].InnerText;
                                }
                                //主队得分大小
                                if (bisairow.ChildNodes[3].HasChildNodes)
                                {
                                    XmlNodeList zdfdx = bisairow.ChildNodes[3].ChildNodes[0].SelectNodes("li/b");
                                    if (zdfdx.Count == 2)
                                    {
                                        o2.Odds_DQZ = zdfdx[0].InnerText;
                                        o2.Odds_XQZ = zdfdx[1].InnerText;
                                    }
                                    XmlNodeList zdfdxtext = bisairow.ChildNodes[3].ChildNodes[0].SelectNodes("li/em");
                                    if (zdfdxtext.Count == 2)
                                    {
                                        o2.Text_DQZ = zdfdxtext[0].InnerText;
                                        o2.Text_XQZ = zdfdxtext[1].InnerText;
                                    }
                                    //客队得分大小
                                    XmlNodeList kdfdx = bisairow.ChildNodes[3].ChildNodes[1].SelectNodes("li/b");
                                    if (kdfdx.Count == 2)
                                    {
                                        o2.Odds_DQK = kdfdx[0].InnerText;
                                        o2.Odds_XQK = kdfdx[1].InnerText;
                                    }
                                    XmlNodeList kdfdxtext = bisairow.ChildNodes[3].ChildNodes[1].SelectNodes("li/em");
                                    if (kdfdxtext.Count == 2)
                                    {
                                        o2.Text_DQK = kdfdxtext[0].InnerText;
                                        o2.Text_XQK = kdfdxtext[1].InnerText;
                                    }
                                }                                
                                m.HalfCourtList.Add(o2);
                            }
                        }
                        lm.MatchList.Add(m);
                    }
                    lmList.Add(lm);
                }
            }
            return lmList;
        }

        /// <summary>
        /// 沙巴篮球滚球列表，胜负盘&让球&大小&单/双             1Q 2Q 3Q 4Q 比赛时间倒计 12至0   NCAA联赛只分上下半场 20至0
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<LeagueMatch> SB_Basketball_GQ_DRDD_List(string html)
        {
            html = html.Replace("&nbsp;", "");
            html = "<root>" + html + "</root>";
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            var doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            TextReader tr = new StringReader(html);
            XmlReader reader = XmlReader.Create(tr, settings);
            //doc.LoadXml(html);
            doc.Load(reader);
            var rowList = doc.DocumentElement.ChildNodes;
            if (rowList != null)
            {
                foreach (XmlNode rowNode in rowList)
                {
                    if (rowNode.Attributes["class"].Value.Contains("match-odds-title"))
                    {
                        continue;
                    }
                    LeagueMatch lm = new LeagueMatch();
                    lm.Name = rowNode.SelectSingleNode("h2/span").InnerText;
                    if (string.IsNullOrEmpty(lm.Name) || FilterLeagueMatch(2, lm.Name))
                    {
                        continue;
                    }
                    var bisaidiv = rowNode.ChildNodes;
                    foreach (XmlNode bisai in bisaidiv)
                    {
                        if (bisai.Name.ToUpper() == "H2")
                        {
                            continue;
                        }
                        Match m = new Match();
                        m.statustext = "";
                        bool init = true;
                        foreach (XmlNode bisairow in bisai.ChildNodes)
                        {
                            if (init)
                            {
                                XmlNodeList scorenode = bisairow.ChildNodes[0].SelectNodes("ul/li/span");
                                if (scorenode.Count >= 4)
                                {
                                    string temp = scorenode[0].InnerText.Trim();
                                    if (temp.Contains("'"))
                                    {
                                        int ts = -1;
                                        string[] tsarr = temp.Split(' ');
                                        int.TryParse(tsarr[1].Trim('\''), out ts);
                                        if (lm.Name.Contains("NCAA"))
                                        {
                                            if (tsarr[0].Contains("H"))
                                            {
                                                if (ts == -1)
                                                {
                                                    ts = 1;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (tsarr[0].Contains("Q"))
                                            {
                                                if (ts == -1)
                                                {
                                                    ts = 1;
                                                }
                                            }
                                        }
                                        m.timing = ts.ToString();
                                        m.halftype = tsarr[0];
                                    }
                                    else
                                    {
                                        if (lm.Name.Contains("NCAA"))
                                        {
                                            if (temp == "半场")//延迟，滚球
                                            {
                                                //m.timing = "45";
                                                m.halftype = "1H";
                                            }
                                        }
                                        m.timing = "-1";
                                        m.statustext = temp;
                                    }
                                    //m.timing = scorenode[0].InnerText;
                                    m.HomeTeamScore = scorenode[1].InnerText;
                                    m.VisitingTeamScore = scorenode[3].InnerText;
                                }
                                XmlNodeList teamnode = bisairow.ChildNodes[1].SelectNodes("ul/li/span");
                                m.HomeTeam = teamnode[0].InnerText;
                                m.VisitingTeam = teamnode[1].InnerText;
                            }
                            if (init || bisairow.ChildNodes[0].ChildNodes.Count == 0)
                            {
                                init = false;
                                //全场赔率================================================
                                Odds o1 = new Odds() { type = 1 };
                                //独赢
                                XmlNodeList duying1 = bisairow.ChildNodes[2].ChildNodes[0].SelectNodes("li/b");
                                if (duying1.Count == 2)
                                {
                                    o1.Odds_ZY = duying1[0].InnerText;
                                    o1.Odds_KY = duying1[1].InnerText;
                                    //o1.IsDisable_DY = "0";
                                    //if (duying1[0].Attributes["class"].Value.Contains("odds-disable") || duying1[1].Attributes["class"].Value.Contains("odds-disable") || duying1[2].Attributes["class"].Value.Contains("odds-disable"))
                                    //{
                                    //    o1.IsDisable_DY = "1";
                                    //}
                                }
                                //让球
                                XmlNodeList rangqiu1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/b");
                                if (rangqiu1.Count == 2)
                                {
                                    o1.Odds_RQZY = rangqiu1[0].InnerText;
                                    o1.Odds_RQKY = rangqiu1[1].InnerText;
                                    //o1.IsDisable_RQ = "0";
                                    //if (rangqiu1[0].Attributes["class"].Value.Contains("odds-disable") || rangqiu1[1].Attributes["class"].Value.Contains("odds-disable"))
                                    //{
                                    //    o1.IsDisable_RQ = "1";
                                    //}
                                }
                                XmlNodeList rangqiutext1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/em");
                                if (rangqiutext1.Count == 2)
                                {
                                    o1.Text_ZRKQ = rangqiutext1[0].InnerText;
                                    o1.Text_KRZQ = rangqiutext1[1].InnerText;
                                }
                                //大小
                                XmlNodeList daxiao1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/b");
                                if (daxiao1.Count == 2)
                                {
                                    o1.Odds_DQ = daxiao1[0].InnerText;
                                    o1.Odds_XQ = daxiao1[1].InnerText;
                                    //o1.IsDisable_DX = "0";
                                    //if (daxiao1[0].Attributes["class"].Value.Contains("odds-disable") || daxiao1[1].Attributes["class"].Value.Contains("odds-disable"))
                                    //{
                                    //    o1.IsDisable_DX = "1";
                                    //}
                                }
                                XmlNodeList daxiaotext1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/em");
                                if (daxiaotext1.Count == 2)
                                {
                                    o1.Text_DQ = daxiaotext1[0].InnerText;
                                    o1.Text_XQ = daxiaotext1[1].InnerText;
                                }
                                //单双
                                XmlNodeList dansuang1 = bisairow.ChildNodes[2].ChildNodes[3].SelectNodes("li/b");
                                if (dansuang1.Count == 2)
                                {
                                    o1.Odds_D = dansuang1[0].InnerText;
                                    o1.Odds_S = dansuang1[1].InnerText;
                                    //o1.IsDisable_DS = "0";
                                    //if (dansuang1[0].Attributes["class"].Value.Contains("odds-disable") || dansuang1[1].Attributes["class"].Value.Contains("odds-disable"))
                                    //{
                                    //    o1.IsDisable_DS = "1";
                                    //}
                                }
                                //主队得分大小
                                if (bisairow.ChildNodes[3].HasChildNodes)
                                {
                                    XmlNodeList zdfdx = bisairow.ChildNodes[3].ChildNodes[0].SelectNodes("li/b");
                                    if (zdfdx.Count == 2)
                                    {
                                        o1.Odds_DQZ = zdfdx[0].InnerText;
                                        o1.Odds_XQZ = zdfdx[1].InnerText;
                                    }
                                    XmlNodeList zdfdxtext = bisairow.ChildNodes[3].ChildNodes[0].SelectNodes("li/em");
                                    if (zdfdxtext.Count == 2)
                                    {
                                        o1.Text_DQZ = zdfdxtext[0].InnerText;
                                        o1.Text_XQZ = zdfdxtext[1].InnerText;
                                    }
                                    //客队得分大小
                                    XmlNodeList kdfdx = bisairow.ChildNodes[3].ChildNodes[1].SelectNodes("li/b");
                                    if (kdfdx.Count == 2)
                                    {
                                        o1.Odds_DQK = kdfdx[0].InnerText;
                                        o1.Odds_XQK = kdfdx[1].InnerText;
                                    }
                                    XmlNodeList kdfdxtext = bisairow.ChildNodes[3].ChildNodes[1].SelectNodes("li/em");
                                    if (kdfdxtext.Count == 2)
                                    {
                                        o1.Text_DQK = kdfdxtext[0].InnerText;
                                        o1.Text_XQK = kdfdxtext[1].InnerText;
                                    }
                                }                                

                                m.FullCourtList.Add(o1);
                            }
                            else
                            {
                                int sectiontype = 0;
                                string section = bisairow.ChildNodes[0].SelectSingleNode("ul/li/div/span").InnerText;
                                switch (section)
                                {
                                    case "上半场":
                                        sectiontype = 2;
                                        break;
                                    case "下半场":
                                        sectiontype = 3;
                                        break;
                                    case "第一节":
                                        sectiontype = 4;
                                        break;
                                    case "第二节":
                                        sectiontype = 5;
                                        break;
                                    case "第三节":
                                        sectiontype = 6;
                                        break;
                                    case "第四节":
                                        sectiontype = 7;
                                        break;
                                }
                                if (sectiontype == 0)
                                {
                                    continue;
                                }
                                //半场================================================
                                Odds o2 = new Odds() { type = sectiontype };
                                //独赢
                                XmlNodeList duying2 = bisairow.ChildNodes[2].ChildNodes[0].SelectNodes("li/b");
                                if (duying2.Count == 2)
                                {
                                    o2.Odds_ZY = duying2[0].InnerText;
                                    o2.Odds_KY = duying2[1].InnerText;
                                    //o2.IsDisable_DY = "0";
                                    //if (duying2[0].Attributes["class"].Value.Contains("odds-disable") || duying2[1].Attributes["class"].Value.Contains("odds-disable") || duying2[2].Attributes["class"].Value.Contains("odds-disable"))
                                    //{
                                    //    o2.IsDisable_DY = "1";
                                    //}
                                }
                                //让球
                                XmlNodeList rangqiu2 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/b");
                                if (rangqiu2.Count == 2)
                                {
                                    o2.Odds_RQZY = rangqiu2[0].InnerText;
                                    o2.Odds_RQKY = rangqiu2[1].InnerText;
                                    //o2.IsDisable_RQ = "0";
                                    //if (rangqiu2[0].Attributes["class"].Value.Contains("odds-disable") || rangqiu2[1].Attributes["class"].Value.Contains("odds-disable"))
                                    //{
                                    //    o2.IsDisable_RQ = "1";
                                    //}
                                }
                                XmlNodeList rangqiutext2 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/em");
                                if (rangqiutext2.Count == 2)
                                {
                                    o2.Text_ZRKQ = rangqiutext2[0].InnerText;
                                    o2.Text_KRZQ = rangqiutext2[1].InnerText;
                                }
                                //大小
                                XmlNodeList daxiao2 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/b");
                                if (daxiao2.Count == 2)
                                {
                                    o2.Odds_DQ = daxiao2[0].InnerText;
                                    o2.Odds_XQ = daxiao2[1].InnerText;
                                    //o2.IsDisable_DX = "0";
                                    //if (daxiao2[0].Attributes["class"].Value.Contains("odds-disable") || daxiao2[1].Attributes["class"].Value.Contains("odds-disable"))
                                    //{
                                    //    o2.IsDisable_DX = "1";
                                    //}
                                }
                                XmlNodeList daxiaotext2 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/em");
                                if (daxiaotext2.Count == 2)
                                {
                                    o2.Text_DQ = daxiaotext2[0].InnerText;
                                    o2.Text_XQ = daxiaotext2[1].InnerText;
                                }
                                //主队得分大小
                                if (bisairow.ChildNodes[3].HasChildNodes)
                                {
                                    XmlNodeList zdfdx = bisairow.ChildNodes[3].ChildNodes[0].SelectNodes("li/b");
                                    if (zdfdx.Count == 2)
                                    {
                                        o2.Odds_DQZ = zdfdx[0].InnerText;
                                        o2.Odds_XQZ = zdfdx[1].InnerText;
                                    }
                                    XmlNodeList zdfdxtext = bisairow.ChildNodes[3].ChildNodes[0].SelectNodes("li/em");
                                    if (zdfdxtext.Count == 2)
                                    {
                                        o2.Text_DQZ = zdfdxtext[0].InnerText;
                                        o2.Text_XQZ = zdfdxtext[1].InnerText;
                                    }
                                    //客队得分大小
                                    XmlNodeList kdfdx = bisairow.ChildNodes[3].ChildNodes[1].SelectNodes("li/b");
                                    if (kdfdx.Count == 2)
                                    {
                                        o2.Odds_DQK = kdfdx[0].InnerText;
                                        o2.Odds_XQK = kdfdx[1].InnerText;
                                    }
                                    XmlNodeList kdfdxtext = bisairow.ChildNodes[3].ChildNodes[1].SelectNodes("li/em");
                                    if (kdfdxtext.Count == 2)
                                    {
                                        o2.Text_DQK = kdfdxtext[0].InnerText;
                                        o2.Text_XQK = kdfdxtext[1].InnerText;
                                    }
                                }
                                
                                m.HalfCourtList.Add(o2);
                            }
                        }
                        lm.MatchList.Add(m);
                    }
                    lmList.Add(lm);
                }
            }
            return lmList;
        }
        #endregion

        #region 美式足球
        /// <summary>
        /// 沙巴美式足球早盘列表，胜负盘&让球&大小&单/双     1Q 2Q 3Q 4Q 比赛时间倒计 15至0
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<LeagueMatch> SB_AmericanFootball_ZP_DRDD_List(string html)
        {
            html = html.Replace("&nbsp;", "");
            html = "<root>" + html + "</root>";
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            var doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            TextReader tr = new StringReader(html);
            XmlReader reader = XmlReader.Create(tr, settings);
            //doc.LoadXml(html);
            doc.Load(reader);
            var rowList = doc.DocumentElement.ChildNodes;
            if (rowList != null)
            {
                foreach (XmlNode rowNode in rowList)
                {
                    if (rowNode.Attributes["class"].Value.Contains("match-odds-title"))
                    {
                        continue;
                    }
                    LeagueMatch lm = new LeagueMatch();
                    lm.Name = rowNode.SelectSingleNode("h2/span").InnerText;
                    if (string.IsNullOrEmpty(lm.Name) || FilterLeagueMatch(8, lm.Name))
                    {
                        continue;
                    }
                    var bisaidiv = rowNode.ChildNodes;
                    foreach (XmlNode bisai in bisaidiv)
                    {
                        if (bisai.Name.ToUpper() == "H2")
                        {
                            continue;
                        }
                        Match m = new Match();
                        bool init = true;
                        foreach (XmlNode bisairow in bisai.ChildNodes)
                        {
                            if (init)
                            {
                                XmlNodeList timenode = bisairow.ChildNodes[0].SelectNodes("ul/li/span");
                                m.time = timenode[0].InnerText + " " + timenode[1].InnerText;
                                if (timenode.Count >= 3)
                                {
                                    m.GQ = timenode[2].InnerText;
                                }
                                XmlNodeList teamnode = bisairow.ChildNodes[1].SelectNodes("ul/li/span");
                                m.HomeTeam = teamnode[0].InnerText;
                                m.VisitingTeam = teamnode[1].InnerText;
                                m.statustext = "";
                            }
                            init = false;
                            //全场赔率================================================
                            Odds o1 = new Odds() { type = 1 };
                            //让球
                            XmlNodeList rangqiu1 = bisairow.ChildNodes[2].ChildNodes[0].SelectNodes("li/b");
                            if (rangqiu1.Count == 2)
                            {
                                o1.Odds_RQZY = rangqiu1[0].InnerText;
                                o1.Odds_RQKY = rangqiu1[1].InnerText;
                                //o1.IsDisable_RQ = "0";
                                //if (rangqiu1[0].Attributes["class"].Value.Contains("odds-disable") || rangqiu1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_RQ = "1";
                                //}
                            }
                            XmlNodeList rangqiutext1 = bisairow.ChildNodes[2].ChildNodes[0].SelectNodes("li/em");
                            if (rangqiutext1.Count == 2)
                            {
                                o1.Text_ZRKQ = rangqiutext1[0].InnerText;
                                o1.Text_KRZQ = rangqiutext1[1].InnerText;
                            }
                            //大小
                            XmlNodeList daxiao1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/b");
                            if (daxiao1.Count == 2)
                            {
                                o1.Odds_DQ = daxiao1[0].InnerText;
                                o1.Odds_XQ = daxiao1[1].InnerText;
                                //o1.IsDisable_DX = "0";
                                //if (daxiao1[0].Attributes["class"].Value.Contains("odds-disable") || daxiao1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DX = "1";
                                //}
                            }
                            XmlNodeList daxiaotext1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/em");
                            if (daxiaotext1.Count == 2)
                            {
                                o1.Text_DQ = daxiaotext1[0].InnerText;
                                o1.Text_XQ = daxiaotext1[1].InnerText;
                            }
                            //单双
                            XmlNodeList dansuang1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/b");
                            if (dansuang1.Count == 2)
                            {
                                o1.Odds_D = dansuang1[0].InnerText;
                                o1.Odds_S = dansuang1[1].InnerText;
                                //o1.IsDisable_DS = "0";
                                //if (dansuang1[0].Attributes["class"].Value.Contains("odds-disable") || dansuang1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DS = "1";
                                //}
                            }

                            m.FullCourtList.Add(o1);

                            //半场================================================
                            Odds o2 = new Odds() { type = 2 };
                            //让球
                            XmlNodeList rangqiu2 = bisairow.ChildNodes[3].ChildNodes[0].SelectNodes("li/b");
                            if (rangqiu2.Count == 2)
                            {
                                o2.Odds_RQZY = rangqiu2[0].InnerText;
                                o2.Odds_RQKY = rangqiu2[1].InnerText;
                                //o2.IsDisable_RQ = "0";
                                //if (rangqiu2[0].Attributes["class"].Value.Contains("odds-disable") || rangqiu2[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o2.IsDisable_RQ = "1";
                                //}
                            }
                            XmlNodeList rangqiutext2 = bisairow.ChildNodes[3].ChildNodes[0].SelectNodes("li/em");
                            if (rangqiutext2.Count == 2)
                            {
                                o2.Text_ZRKQ = rangqiutext2[0].InnerText;
                                o2.Text_KRZQ = rangqiutext2[1].InnerText;
                            }
                            //大小
                            XmlNodeList daxiao2 = bisairow.ChildNodes[3].ChildNodes[1].SelectNodes("li/b");
                            if (daxiao2.Count == 2)
                            {
                                o2.Odds_DQ = daxiao2[0].InnerText;
                                o2.Odds_XQ = daxiao2[1].InnerText;
                                //o2.IsDisable_DX = "0";
                                //if (daxiao2[0].Attributes["class"].Value.Contains("odds-disable") || daxiao2[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o2.IsDisable_DX = "1";
                                //}
                            }
                            XmlNodeList daxiaotext2 = bisairow.ChildNodes[3].ChildNodes[1].SelectNodes("li/em");
                            if (daxiaotext2.Count == 2)
                            {
                                o2.Text_DQ = daxiaotext2[0].InnerText;
                                o2.Text_XQ = daxiaotext2[1].InnerText;
                            }

                            m.HalfCourtList.Add(o2);
                        }
                        lm.MatchList.Add(m);
                    }
                    lmList.Add(lm);
                }
            }
            return lmList;
        }
        /// <summary>
        /// 沙巴美式足球滚球列表，胜负盘&让球&大小&单/双     1Q 2Q 3Q 4Q 比赛时间倒计 15至0
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<LeagueMatch> SB_AmericanFootball_GQ_DRDD_List(string html)
        {
            html = html.Replace("&nbsp;", "");
            html = "<root>" + html + "</root>";
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            var doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            TextReader tr = new StringReader(html);
            XmlReader reader = XmlReader.Create(tr, settings);
            //doc.LoadXml(html);
            doc.Load(reader);
            var rowList = doc.DocumentElement.ChildNodes;
            if (rowList != null)
            {
                foreach (XmlNode rowNode in rowList)
                {
                    if (rowNode.Attributes["class"].Value.Contains("match-odds-title"))
                    {
                        continue;
                    }
                    LeagueMatch lm = new LeagueMatch();
                    lm.Name = rowNode.SelectSingleNode("h2/span").InnerText;
                    if (string.IsNullOrEmpty(lm.Name) || FilterLeagueMatch(8, lm.Name))
                    {
                        continue;
                    }
                    var bisaidiv = rowNode.ChildNodes;
                    foreach (XmlNode bisai in bisaidiv)
                    {
                        if (bisai.Name.ToUpper() == "H2")
                        {
                            continue;
                        }
                        Match m = new Match();
                        m.statustext = "";
                        bool init = true;
                        foreach (XmlNode bisairow in bisai.ChildNodes)
                        {
                            if (init)
                            {
                                XmlNodeList scorenode = bisairow.ChildNodes[0].SelectNodes("ul/li/span");
                                if (scorenode.Count >= 4)
                                {
                                    string temp = scorenode[0].InnerText.Trim();
                                    if (temp.Contains("'"))
                                    {
                                        int ts = -1;
                                        string[] tsarr = temp.Split(' ');
                                        int.TryParse(tsarr[1].Trim('\''), out ts);
                                        if (tsarr[0].Contains("Q"))
                                        {
                                            if (ts == -1)
                                            {
                                                ts = 1;
                                            }
                                        }
                                        m.timing = ts.ToString();
                                        m.halftype = tsarr[0];
                                    }
                                    else
                                    {
                                        m.timing = "-1";
                                        m.statustext = temp;
                                    }
                                    //m.timing = scorenode[0].InnerText;
                                    m.HomeTeamScore = scorenode[1].InnerText;
                                    m.VisitingTeamScore = scorenode[3].InnerText;
                                }
                                XmlNodeList teamnode = bisairow.ChildNodes[1].SelectNodes("ul/li/span");
                                m.HomeTeam = teamnode[0].InnerText;
                                m.VisitingTeam = teamnode[1].InnerText;
                            }
                            init = false;
                            //全场赔率================================================
                            Odds o1 = new Odds() { type = 1 };
                            //让球
                            XmlNodeList rangqiu1 = bisairow.ChildNodes[2].ChildNodes[0].SelectNodes("li/b");
                            if (rangqiu1.Count == 2)
                            {
                                o1.Odds_RQZY = rangqiu1[0].InnerText;
                                o1.Odds_RQKY = rangqiu1[1].InnerText;
                                //o1.IsDisable_RQ = "0";
                                //if (rangqiu1[0].Attributes["class"].Value.Contains("odds-disable") || rangqiu1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_RQ = "1";
                                //}
                            }
                            XmlNodeList rangqiutext1 = bisairow.ChildNodes[2].ChildNodes[0].SelectNodes("li/em");
                            if (rangqiutext1.Count == 2)
                            {
                                o1.Text_ZRKQ = rangqiutext1[0].InnerText;
                                o1.Text_KRZQ = rangqiutext1[1].InnerText;
                            }
                            //大小
                            XmlNodeList daxiao1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/b");
                            if (daxiao1.Count == 2)
                            {
                                o1.Odds_DQ = daxiao1[0].InnerText;
                                o1.Odds_XQ = daxiao1[1].InnerText;
                                //o1.IsDisable_DX = "0";
                                //if (daxiao1[0].Attributes["class"].Value.Contains("odds-disable") || daxiao1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DX = "1";
                                //}
                            }
                            XmlNodeList daxiaotext1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/em");
                            if (daxiaotext1.Count == 2)
                            {
                                o1.Text_DQ = daxiaotext1[0].InnerText;
                                o1.Text_XQ = daxiaotext1[1].InnerText;
                            }
                            //单双
                            XmlNodeList dansuang1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/b");
                            if (dansuang1.Count == 2)
                            {
                                o1.Odds_D = dansuang1[0].InnerText;
                                o1.Odds_S = dansuang1[1].InnerText;
                                //o1.IsDisable_DS = "0";
                                //if (dansuang1[0].Attributes["class"].Value.Contains("odds-disable") || dansuang1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DS = "1";
                                //}
                            }

                            m.FullCourtList.Add(o1);

                            //半场================================================
                            Odds o2 = new Odds() { type = 2 };
                            //让球
                            XmlNodeList rangqiu2 = bisairow.ChildNodes[3].ChildNodes[0].SelectNodes("li/b");
                            if (rangqiu2.Count == 2)
                            {
                                o2.Odds_RQZY = rangqiu2[0].InnerText;
                                o2.Odds_RQKY = rangqiu2[1].InnerText;
                                //o2.IsDisable_RQ = "0";
                                //if (rangqiu2[0].Attributes["class"].Value.Contains("odds-disable") || rangqiu2[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o2.IsDisable_RQ = "1";
                                //}
                            }
                            XmlNodeList rangqiutext2 = bisairow.ChildNodes[3].ChildNodes[0].SelectNodes("li/em");
                            if (rangqiutext2.Count == 2)
                            {
                                o2.Text_ZRKQ = rangqiutext2[0].InnerText;
                                o2.Text_KRZQ = rangqiutext2[1].InnerText;
                            }
                            //大小
                            XmlNodeList daxiao2 = bisairow.ChildNodes[3].ChildNodes[1].SelectNodes("li/b");
                            if (daxiao2.Count == 2)
                            {
                                o2.Odds_DQ = daxiao2[0].InnerText;
                                o2.Odds_XQ = daxiao2[1].InnerText;
                                //o2.IsDisable_DX = "0";
                                //if (daxiao2[0].Attributes["class"].Value.Contains("odds-disable") || daxiao2[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o2.IsDisable_DX = "1";
                                //}
                            }
                            XmlNodeList daxiaotext2 = bisairow.ChildNodes[3].ChildNodes[1].SelectNodes("li/em");
                            if (daxiaotext2.Count == 2)
                            {
                                o2.Text_DQ = daxiaotext2[0].InnerText;
                                o2.Text_XQ = daxiaotext2[1].InnerText;
                            }

                            m.HalfCourtList.Add(o2);
                        }
                        lm.MatchList.Add(m);
                    }
                    lmList.Add(lm);
                }
            }
            return lmList;
        } 
        #endregion        

        #region 网球
        /// <summary>
        /// 沙巴网球早盘列表，胜负盘 & 让盘 & 大小 & 单/双
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<LeagueMatch> SB_Tennis_ZP_DRDD_List(string html)
        {
            html = html.Replace("&nbsp;", "");
            html = "<root>" + html + "</root>";
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            var doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            TextReader tr = new StringReader(html);
            XmlReader reader = XmlReader.Create(tr, settings);
            //doc.LoadXml(html);
            doc.Load(reader);
            var rowList = doc.DocumentElement.ChildNodes;
            if (rowList != null)
            {
                foreach (XmlNode rowNode in rowList)
                {
                    if (rowNode.Attributes["class"].Value.Contains("match-odds-title"))
                    {
                        continue;
                    }
                    LeagueMatch lm = new LeagueMatch();
                    lm.Name = rowNode.SelectSingleNode("h2/span").InnerText;
                    if (string.IsNullOrEmpty(lm.Name) || FilterLeagueMatch(3, lm.Name))
                    {
                        continue;
                    }
                    var bisaidiv = rowNode.ChildNodes;
                    foreach (XmlNode bisai in bisaidiv)
                    {
                        if (bisai.Name.ToUpper() == "H2")
                        {
                            continue;
                        }
                        Match m = new Match();
                        bool init = true;
                        foreach (XmlNode bisairow in bisai.ChildNodes)
                        {
                            if (init)
                            {
                                XmlNodeList timenode = bisairow.ChildNodes[0].SelectNodes("ul/li/span");
                                m.time = timenode[0].InnerText + " " + timenode[1].InnerText;
                                if (timenode.Count >= 3)
                                {
                                    m.GQ = timenode[2].InnerText;
                                }
                                XmlNodeList teamnode = bisairow.ChildNodes[1].SelectNodes("ul/li/span");
                                m.HomeTeam = teamnode[0].InnerText;
                                m.VisitingTeam = teamnode[1].InnerText;
                                m.statustext = "";
                            }
                            init = false;
                            //全场赔率================================================
                            Odds o1 = new Odds() { type = 1 };
                            //独赢
                            XmlNodeList duying1 = bisairow.ChildNodes[2].ChildNodes[0].SelectNodes("li/b");
                            if (duying1.Count == 2)
                            {
                                o1.Odds_ZY = duying1[0].InnerText;
                                o1.Odds_KY = duying1[1].InnerText;
                                //o1.IsDisable_DY = "0";
                                //if (duying1[0].Attributes["class"].Value.Contains("odds-disable") || duying1[1].Attributes["class"].Value.Contains("odds-disable") || duying1[2].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DY = "1";
                                //}
                            }
                            //让盘
                            XmlNodeList rangqiu1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/b");
                            if (rangqiu1.Count == 2)
                            {
                                o1.Odds_RQZY = rangqiu1[0].InnerText;
                                o1.Odds_RQKY = rangqiu1[1].InnerText;
                                //o1.IsDisable_RQ = "0";
                                //if (rangqiu1[0].Attributes["class"].Value.Contains("odds-disable") || rangqiu1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_RQ = "1";
                                //}
                            }
                            XmlNodeList rangqiutext1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/em");
                            if (rangqiutext1.Count == 2)
                            {
                                o1.Text_ZRKQ = rangqiutext1[0].InnerText;
                                o1.Text_KRZQ = rangqiutext1[1].InnerText;
                            }
                            //大小
                            XmlNodeList daxiao1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/b");
                            if (daxiao1.Count == 2)
                            {
                                o1.Odds_DQ = daxiao1[0].InnerText;
                                o1.Odds_XQ = daxiao1[1].InnerText;
                                //o1.IsDisable_DX = "0";
                                //if (daxiao1[0].Attributes["class"].Value.Contains("odds-disable") || daxiao1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DX = "1";
                                //}
                            }
                            XmlNodeList daxiaotext1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/em");
                            if (daxiaotext1.Count == 2)
                            {
                                o1.Text_DQ = daxiaotext1[0].InnerText;
                                o1.Text_XQ = daxiaotext1[1].InnerText;
                            }
                            //单双
                            XmlNodeList dansuang1 = bisairow.ChildNodes[2].ChildNodes[3].SelectNodes("li/b");
                            if (dansuang1.Count == 2)
                            {
                                o1.Odds_D = dansuang1[0].InnerText;
                                o1.Odds_S = dansuang1[1].InnerText;
                                //o1.IsDisable_DS = "0";
                                //if (dansuang1[0].Attributes["class"].Value.Contains("odds-disable") || dansuang1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DS = "1";
                                //}
                            }
                            //让局
                            XmlNodeList rangju = bisairow.ChildNodes[3].ChildNodes[0].SelectNodes("li/b");
                            if (rangju.Count == 2)
                            {
                                o1.Odds_RJZY = rangju[0].InnerText;
                                o1.Odds_RJKY = rangju[1].InnerText;
                            }
                            XmlNodeList rangjutext = bisairow.ChildNodes[3].ChildNodes[0].SelectNodes("li/em");
                            if (rangjutext.Count == 2)
                            {
                                o1.Text_ZRKJ = rangjutext[0].InnerText;
                                o1.Text_KRZJ = rangjutext[1].InnerText;
                            }

                            m.FullCourtList.Add(o1);                            
                        }
                        lm.MatchList.Add(m);
                    }
                    lmList.Add(lm);
                }
            }
            return lmList;
        }
        /// <summary>
        /// 沙巴网球滚球列表，胜负盘 & 让盘 & 大小 & 单/双
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<LeagueMatch> SB_Tennis_GQ_DRDD_List(string html)
        {
            html = html.Replace("&nbsp;", "");
            html = "<root>" + html + "</root>";
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            var doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            TextReader tr = new StringReader(html);
            XmlReader reader = XmlReader.Create(tr, settings);
            //doc.LoadXml(html);
            doc.Load(reader);
            var rowList = doc.DocumentElement.ChildNodes;
            if (rowList != null)
            {
                foreach (XmlNode rowNode in rowList)
                {
                    if (rowNode.Attributes["class"].Value.Contains("match-odds-title"))
                    {
                        continue;
                    }
                    LeagueMatch lm = new LeagueMatch();
                    lm.Name = rowNode.SelectSingleNode("h2/span").InnerText;
                    if (string.IsNullOrEmpty(lm.Name) || FilterLeagueMatch(3, lm.Name))
                    {
                        continue;
                    }
                    var bisaidiv = rowNode.ChildNodes;
                    foreach (XmlNode bisai in bisaidiv)
                    {
                        if (bisai.Name.ToUpper() == "H2")
                        {
                            continue;
                        }
                        Match m = new Match();
                        m.statustext = "";
                        bool init = true;
                        foreach (XmlNode bisairow in bisai.ChildNodes)
                        {
                            if (init)
                            {
                                XmlNodeList scorenode = bisairow.ChildNodes[0].SelectNodes("ul/li");
                                if (scorenode.Count == 3)
                                {
                                    m.HomeTeamSet = scorenode[1].ChildNodes[1].InnerText;
                                    m.HomeTeamInning = scorenode[1].ChildNodes[2].InnerText;
                                    m.HomeTeamScore = scorenode[1].ChildNodes[3].InnerText;

                                    m.VisitingTeamSet = scorenode[2].ChildNodes[1].InnerText;
                                    m.VisitingTeamInning = scorenode[2].ChildNodes[2].InnerText;
                                    m.VisitingTeamScore = scorenode[2].ChildNodes[3].InnerText;
                                }
                                XmlNodeList teamnode = bisairow.ChildNodes[1].SelectNodes("ul/li/span");
                                m.HomeTeam = teamnode[0].InnerText;
                                m.VisitingTeam = teamnode[1].InnerText;
                            }
                            init = false;
                            //全场赔率================================================
                            Odds o1 = new Odds() { type = 1 };
                            //独赢
                            XmlNodeList duying1 = bisairow.ChildNodes[2].ChildNodes[0].SelectNodes("li/b");
                            if (duying1.Count == 2)
                            {
                                o1.Odds_ZY = duying1[0].InnerText;
                                o1.Odds_KY = duying1[1].InnerText;
                                //o1.IsDisable_DY = "0";
                                //if (duying1[0].Attributes["class"].Value.Contains("odds-disable") || duying1[1].Attributes["class"].Value.Contains("odds-disable") || duying1[2].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DY = "1";
                                //}
                            }
                            //让球
                            XmlNodeList rangqiu1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/b");
                            if (rangqiu1.Count == 2)
                            {
                                o1.Odds_RQZY = rangqiu1[0].InnerText;
                                o1.Odds_RQKY = rangqiu1[1].InnerText;
                                //o1.IsDisable_RQ = "0";
                                //if (rangqiu1[0].Attributes["class"].Value.Contains("odds-disable") || rangqiu1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_RQ = "1";
                                //}
                            }
                            XmlNodeList rangqiutext1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/em");
                            if (rangqiutext1.Count == 2)
                            {
                                o1.Text_ZRKQ = rangqiutext1[0].InnerText;
                                o1.Text_KRZQ = rangqiutext1[1].InnerText;
                            }
                            //大小
                            XmlNodeList daxiao1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/b");
                            if (daxiao1.Count == 2)
                            {
                                o1.Odds_DQ = daxiao1[0].InnerText;
                                o1.Odds_XQ = daxiao1[1].InnerText;
                                //o1.IsDisable_DX = "0";
                                //if (daxiao1[0].Attributes["class"].Value.Contains("odds-disable") || daxiao1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DX = "1";
                                //}
                            }
                            XmlNodeList daxiaotext1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/em");
                            if (daxiaotext1.Count == 2)
                            {
                                o1.Text_DQ = daxiaotext1[0].InnerText;
                                o1.Text_XQ = daxiaotext1[1].InnerText;
                            }
                            //单双
                            XmlNodeList dansuang1 = bisairow.ChildNodes[2].ChildNodes[3].SelectNodes("li/b");
                            if (dansuang1.Count == 2)
                            {
                                o1.Odds_D = dansuang1[0].InnerText;
                                o1.Odds_S = dansuang1[1].InnerText;
                                //o1.IsDisable_DS = "0";
                                //if (dansuang1[0].Attributes["class"].Value.Contains("odds-disable") || dansuang1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DS = "1";
                                //}
                            }
                            //让局
                            XmlNodeList rangju = bisairow.ChildNodes[3].ChildNodes[0].SelectNodes("li/b");
                            if (rangju.Count == 2)
                            {
                                o1.Odds_RJZY = rangju[0].InnerText;
                                o1.Odds_RJKY = rangju[1].InnerText;
                            }
                            XmlNodeList rangjutext = bisairow.ChildNodes[3].ChildNodes[0].SelectNodes("li/em");
                            if (rangjutext.Count == 2)
                            {
                                o1.Text_ZRKJ = rangjutext[0].InnerText;
                                o1.Text_KRZJ = rangjutext[1].InnerText;
                            }

                            m.FullCourtList.Add(o1);                            
                        }
                        lm.MatchList.Add(m);
                    }
                    lmList.Add(lm);
                }
            }
            return lmList;
        }
        #endregion

        #region 排球
        /// <summary>
        /// 沙巴排球早盘列表，胜负盘 & 让盘 & 大小 & 单/双
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<LeagueMatch> SB_Volleyball_ZP_DRDD_List(string html)
        {
            html = html.Replace("&nbsp;", "");
            html = "<root>" + html + "</root>";
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            var doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            TextReader tr = new StringReader(html);
            XmlReader reader = XmlReader.Create(tr, settings);
            //doc.LoadXml(html);
            doc.Load(reader);
            var rowList = doc.DocumentElement.ChildNodes;
            if (rowList != null)
            {
                foreach (XmlNode rowNode in rowList)
                {
                    if (rowNode.Attributes["class"].Value.Contains("match-odds-title"))
                    {
                        continue;
                    }
                    LeagueMatch lm = new LeagueMatch();
                    lm.Name = rowNode.SelectSingleNode("h2/span").InnerText;
                    if (string.IsNullOrEmpty(lm.Name) || FilterLeagueMatch(4, lm.Name))
                    {
                        continue;
                    }
                    var bisaidiv = rowNode.ChildNodes;
                    foreach (XmlNode bisai in bisaidiv)
                    {
                        if (bisai.Name.ToUpper() == "H2")
                        {
                            continue;
                        }
                        Match m = new Match();
                        bool init = true;
                        foreach (XmlNode bisairow in bisai.ChildNodes)
                        {
                            if (init)
                            {
                                XmlNodeList timenode = bisairow.ChildNodes[0].SelectNodes("ul/li/span");
                                m.time = timenode[0].InnerText + " " + timenode[1].InnerText;
                                if (timenode.Count >= 3)
                                {
                                    m.GQ = timenode[2].InnerText;
                                }
                                XmlNodeList teamnode = bisairow.ChildNodes[1].SelectNodes("ul/li/span");
                                m.HomeTeam = teamnode[0].InnerText;
                                m.VisitingTeam = teamnode[1].InnerText;
                                m.statustext = "";
                            }
                            init = false;
                            //全场赔率================================================
                            Odds o1 = new Odds() { type = 1 };
                            //独赢
                            XmlNodeList duying1 = bisairow.ChildNodes[2].ChildNodes[0].SelectNodes("li/b");
                            if (duying1.Count == 2)
                            {
                                o1.Odds_ZY = duying1[0].InnerText;
                                o1.Odds_KY = duying1[1].InnerText;
                                //o1.IsDisable_DY = "0";
                                //if (duying1[0].Attributes["class"].Value.Contains("odds-disable") || duying1[1].Attributes["class"].Value.Contains("odds-disable") || duying1[2].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DY = "1";
                                //}
                            }
                            //让分
                            XmlNodeList rangqiu1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/b");
                            if (rangqiu1.Count == 2)
                            {
                                o1.Odds_RQZY = rangqiu1[0].InnerText;
                                o1.Odds_RQKY = rangqiu1[1].InnerText;
                                //o1.IsDisable_RQ = "0";
                                //if (rangqiu1[0].Attributes["class"].Value.Contains("odds-disable") || rangqiu1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_RQ = "1";
                                //}
                            }
                            XmlNodeList rangqiutext1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/em");
                            if (rangqiutext1.Count == 2)
                            {
                                o1.Text_ZRKQ = rangqiutext1[0].InnerText;
                                o1.Text_KRZQ = rangqiutext1[1].InnerText;
                            }
                            //大小
                            XmlNodeList daxiao1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/b");
                            if (daxiao1.Count == 2)
                            {
                                o1.Odds_DQ = daxiao1[0].InnerText;
                                o1.Odds_XQ = daxiao1[1].InnerText;
                                //o1.IsDisable_DX = "0";
                                //if (daxiao1[0].Attributes["class"].Value.Contains("odds-disable") || daxiao1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DX = "1";
                                //}
                            }
                            XmlNodeList daxiaotext1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/em");
                            if (daxiaotext1.Count == 2)
                            {
                                o1.Text_DQ = daxiaotext1[0].InnerText;
                                o1.Text_XQ = daxiaotext1[1].InnerText;
                            }
                            //单双
                            XmlNodeList dansuang1 = bisairow.ChildNodes[2].ChildNodes[3].SelectNodes("li/b");
                            if (dansuang1.Count == 2)
                            {
                                o1.Odds_D = dansuang1[0].InnerText;
                                o1.Odds_S = dansuang1[1].InnerText;
                                //o1.IsDisable_DS = "0";
                                //if (dansuang1[0].Attributes["class"].Value.Contains("odds-disable") || dansuang1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DS = "1";
                                //}
                            }
                            m.FullCourtList.Add(o1);
                        }
                        lm.MatchList.Add(m);
                    }
                    lmList.Add(lm);
                }
            }
            return lmList;
        }
        /// <summary>
        /// 沙巴排球滚球列表，胜负盘 & 让盘 & 大小 & 单/双
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<LeagueMatch> SB_Volleyball_GQ_DRDD_List(string html)
        {
            html = html.Replace("&nbsp;", "");
            html = "<root>" + html + "</root>";
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            var doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            TextReader tr = new StringReader(html);
            XmlReader reader = XmlReader.Create(tr, settings);
            //doc.LoadXml(html);
            doc.Load(reader);
            var rowList = doc.DocumentElement.ChildNodes;
            if (rowList != null)
            {
                foreach (XmlNode rowNode in rowList)
                {
                    if (rowNode.Attributes["class"].Value.Contains("match-odds-title"))
                    {
                        continue;
                    }
                    LeagueMatch lm = new LeagueMatch();
                    lm.Name = rowNode.SelectSingleNode("h2/span").InnerText;
                    if (string.IsNullOrEmpty(lm.Name) || FilterLeagueMatch(4, lm.Name))
                    {
                        continue;
                    }
                    var bisaidiv = rowNode.ChildNodes;
                    foreach (XmlNode bisai in bisaidiv)
                    {
                        if (bisai.Name.ToUpper() == "H2")
                        {
                            continue;
                        }
                        Match m = new Match();
                        m.statustext = "";
                        bool init = true;
                        foreach (XmlNode bisairow in bisai.ChildNodes)
                        {
                            if (init)
                            {
                                XmlNodeList scorenode = bisairow.ChildNodes[0].SelectNodes("ul/li");
                                if (scorenode.Count == 3)
                                {
                                    m.HomeTeamSet = scorenode[1].ChildNodes[1].InnerText;
                                    m.HomeTeamInning = scorenode[1].ChildNodes[2].InnerText;
                                    m.HomeTeamScore = scorenode[1].ChildNodes[3].InnerText;

                                    m.VisitingTeamSet = scorenode[2].ChildNodes[1].InnerText;
                                    m.VisitingTeamInning = scorenode[2].ChildNodes[2].InnerText;
                                    m.VisitingTeamScore = scorenode[2].ChildNodes[3].InnerText;
                                }
                                XmlNodeList teamnode = bisairow.ChildNodes[1].SelectNodes("ul/li/span");
                                m.HomeTeam = teamnode[0].InnerText;
                                m.VisitingTeam = teamnode[1].InnerText;
                            }
                            init = false;
                            //全场赔率================================================
                            Odds o1 = new Odds() { type = 1 };
                            //独赢
                            XmlNodeList duying1 = bisairow.ChildNodes[2].ChildNodes[0].SelectNodes("li/b");
                            if (duying1.Count == 2)
                            {
                                o1.Odds_ZY = duying1[0].InnerText;
                                o1.Odds_KY = duying1[1].InnerText;
                                //o1.IsDisable_DY = "0";
                                //if (duying1[0].Attributes["class"].Value.Contains("odds-disable") || duying1[1].Attributes["class"].Value.Contains("odds-disable") || duying1[2].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DY = "1";
                                //}
                            }
                            //让球
                            XmlNodeList rangqiu1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/b");
                            if (rangqiu1.Count == 2)
                            {
                                o1.Odds_RQZY = rangqiu1[0].InnerText;
                                o1.Odds_RQKY = rangqiu1[1].InnerText;
                                //o1.IsDisable_RQ = "0";
                                //if (rangqiu1[0].Attributes["class"].Value.Contains("odds-disable") || rangqiu1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_RQ = "1";
                                //}
                            }
                            XmlNodeList rangqiutext1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/em");
                            if (rangqiutext1.Count == 2)
                            {
                                o1.Text_ZRKQ = rangqiutext1[0].InnerText;
                                o1.Text_KRZQ = rangqiutext1[1].InnerText;
                            }
                            //大小
                            XmlNodeList daxiao1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/b");
                            if (daxiao1.Count == 2)
                            {
                                o1.Odds_DQ = daxiao1[0].InnerText;
                                o1.Odds_XQ = daxiao1[1].InnerText;
                                //o1.IsDisable_DX = "0";
                                //if (daxiao1[0].Attributes["class"].Value.Contains("odds-disable") || daxiao1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DX = "1";
                                //}
                            }
                            XmlNodeList daxiaotext1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/em");
                            if (daxiaotext1.Count == 2)
                            {
                                o1.Text_DQ = daxiaotext1[0].InnerText;
                                o1.Text_XQ = daxiaotext1[1].InnerText;
                            }
                            //单双
                            XmlNodeList dansuang1 = bisairow.ChildNodes[2].ChildNodes[3].SelectNodes("li/b");
                            if (dansuang1.Count == 2)
                            {
                                o1.Odds_D = dansuang1[0].InnerText;
                                o1.Odds_S = dansuang1[1].InnerText;
                                //o1.IsDisable_DS = "0";
                                //if (dansuang1[0].Attributes["class"].Value.Contains("odds-disable") || dansuang1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DS = "1";
                                //}
                            }
                            m.FullCourtList.Add(o1);
                        }
                        lm.MatchList.Add(m);
                    }
                    lmList.Add(lm);
                }
            }
            return lmList;
        }
        #endregion

        #region 棒球
        // 
        #endregion

        #region 羽毛球
        /// <summary>
        /// 沙巴羽毛球早盘列表，胜负盘 & 让局 & 单/双 & 大小
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<LeagueMatch> SB_Badminton_ZP_DRDD_List(string html)
        {
            html = html.Replace("&nbsp;", "");
            html = "<root>" + html + "</root>";
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            var doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            TextReader tr = new StringReader(html);
            XmlReader reader = XmlReader.Create(tr, settings);
            //doc.LoadXml(html);
            doc.Load(reader);
            var rowList = doc.DocumentElement.ChildNodes;
            if (rowList != null)
            {
                foreach (XmlNode rowNode in rowList)
                {
                    if (rowNode.Attributes["class"].Value.Contains("match-odds-title"))
                    {
                        continue;
                    }
                    LeagueMatch lm = new LeagueMatch();
                    lm.Name = rowNode.SelectSingleNode("h2/span").InnerText;
                    if (string.IsNullOrEmpty(lm.Name) || FilterLeagueMatch(6, lm.Name))
                    {
                        continue;
                    }
                    var bisaidiv = rowNode.ChildNodes;
                    foreach (XmlNode bisai in bisaidiv)
                    {
                        if (bisai.Name.ToUpper() == "H2")
                        {
                            continue;
                        }
                        Match m = new Match();
                        bool init = true;
                        foreach (XmlNode bisairow in bisai.ChildNodes)
                        {
                            if (init)
                            {
                                XmlNodeList timenode = bisairow.ChildNodes[0].SelectNodes("ul/li/span");
                                m.time = timenode[0].InnerText + " " + timenode[1].InnerText;
                                if (timenode.Count >= 3)
                                {
                                    m.GQ = timenode[2].InnerText;
                                }
                                XmlNodeList teamnode = bisairow.ChildNodes[1].SelectNodes("ul/li/span");
                                m.HomeTeam = teamnode[0].InnerText;
                                m.VisitingTeam = teamnode[1].InnerText;
                                m.statustext = "";
                            }
                            init = false;
                            //全场赔率================================================
                            Odds o1 = new Odds() { type = 1 };
                            //独赢
                            XmlNodeList duying1 = bisairow.ChildNodes[2].ChildNodes[0].SelectNodes("li/b");
                            if (duying1.Count == 2)
                            {
                                o1.Odds_ZY = duying1[0].InnerText;
                                o1.Odds_KY = duying1[1].InnerText;
                                //o1.IsDisable_DY = "0";
                                //if (duying1[0].Attributes["class"].Value.Contains("odds-disable") || duying1[1].Attributes["class"].Value.Contains("odds-disable") || duying1[2].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DY = "1";
                                //}
                            }
                            //让球
                            XmlNodeList rangqiu1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/b");
                            if (rangqiu1.Count == 2)
                            {
                                o1.Odds_RQZY = rangqiu1[0].InnerText;
                                o1.Odds_RQKY = rangqiu1[1].InnerText;
                                //o1.IsDisable_RQ = "0";
                                //if (rangqiu1[0].Attributes["class"].Value.Contains("odds-disable") || rangqiu1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_RQ = "1";
                                //}
                            }
                            XmlNodeList rangqiutext1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/em");
                            if (rangqiutext1.Count == 2)
                            {
                                o1.Text_ZRKQ = rangqiutext1[0].InnerText;
                                o1.Text_KRZQ = rangqiutext1[1].InnerText;
                            }
                            //大小
                            XmlNodeList daxiao1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/b");
                            if (daxiao1.Count == 2)
                            {
                                o1.Odds_DQ = daxiao1[0].InnerText;
                                o1.Odds_XQ = daxiao1[1].InnerText;
                                //o1.IsDisable_DX = "0";
                                //if (daxiao1[0].Attributes["class"].Value.Contains("odds-disable") || daxiao1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DX = "1";
                                //}
                            }
                            XmlNodeList daxiaotext1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/em");
                            if (daxiaotext1.Count == 2)
                            {
                                o1.Text_DQ = daxiaotext1[0].InnerText;
                                o1.Text_XQ = daxiaotext1[1].InnerText;
                            }
                            //单双
                            XmlNodeList dansuang1 = bisairow.ChildNodes[2].ChildNodes[3].SelectNodes("li/b");
                            if (dansuang1.Count == 2)
                            {
                                o1.Odds_D = dansuang1[0].InnerText;
                                o1.Odds_S = dansuang1[1].InnerText;
                                //o1.IsDisable_DS = "0";
                                //if (dansuang1[0].Attributes["class"].Value.Contains("odds-disable") || dansuang1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DS = "1";
                                //}
                            }

                            m.FullCourtList.Add(o1);
                        }
                        lm.MatchList.Add(m);
                    }
                    lmList.Add(lm);
                }
            }
            return lmList;
        }
        /// <summary>
        /// 沙巴羽毛球滚球列表，胜负盘 & 让局 & 单/双 & 大小
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<LeagueMatch> SB_Badminton_GQ_DRDD_List(string html)
        {
            html = html.Replace("&nbsp;", "");
            html = "<root>" + html + "</root>";
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            var doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            TextReader tr = new StringReader(html);
            XmlReader reader = XmlReader.Create(tr, settings);
            //doc.LoadXml(html);
            doc.Load(reader);
            var rowList = doc.DocumentElement.ChildNodes;
            if (rowList != null)
            {
                foreach (XmlNode rowNode in rowList)
                {
                    if (rowNode.Attributes["class"].Value.Contains("match-odds-title"))
                    {
                        continue;
                    }
                    LeagueMatch lm = new LeagueMatch();
                    lm.Name = rowNode.SelectSingleNode("h2/span").InnerText;
                    if (string.IsNullOrEmpty(lm.Name) || FilterLeagueMatch(6, lm.Name))
                    {
                        continue;
                    }
                    var bisaidiv = rowNode.ChildNodes;
                    foreach (XmlNode bisai in bisaidiv)
                    {
                        if (bisai.Name.ToUpper() == "H2")
                        {
                            continue;
                        }
                        Match m = new Match();
                        m.statustext = "";
                        bool init = true;
                        foreach (XmlNode bisairow in bisai.ChildNodes)
                        {
                            if (init)
                            {
                                //沙巴暂时只显示滚球0-0
                                //XmlNodeList scorenode = bisairow.ChildNodes[0].SelectNodes("ul/li/span");
                                //if (scorenode.Count >= 4)
                                //{
                                //    string temp = scorenode[0].InnerText.Trim();
                                //    if (temp.Contains("'"))
                                //    {
                                //        int ts = -1;
                                //        string[] tsarr = temp.Split(' ');
                                //        int.TryParse(tsarr[1].Trim('\''), out ts);
                                //        if (tsarr[0].Contains("H"))
                                //        {
                                //            if (ts == -1)
                                //            {
                                //                ts = 1;
                                //            }
                                //        }
                                //        m.timing = ts.ToString();
                                //        m.halftype = tsarr[0];
                                //    }
                                //    else
                                //    {
                                //        m.timing = "-1";
                                //        m.statustext = temp;
                                //    }
                                //    //m.timing = scorenode[0].InnerText;
                                //    m.HomeTeamScore = scorenode[1].InnerText;
                                //    m.VisitingTeamScore = scorenode[3].InnerText;
                                //}
                                XmlNodeList teamnode = bisairow.ChildNodes[1].SelectNodes("ul/li/span");
                                m.HomeTeam = teamnode[0].InnerText;
                                m.VisitingTeam = teamnode[1].InnerText;
                            }
                            init = false;
                            //全场赔率================================================
                            Odds o1 = new Odds() { type = 1 };
                            //独赢
                            XmlNodeList duying1 = bisairow.ChildNodes[2].ChildNodes[0].SelectNodes("li/b");
                            if (duying1.Count == 2)
                            {
                                o1.Odds_ZY = duying1[0].InnerText;
                                o1.Odds_KY = duying1[1].InnerText;
                                //o1.IsDisable_DY = "0";
                                //if (duying1[0].Attributes["class"].Value.Contains("odds-disable") || duying1[1].Attributes["class"].Value.Contains("odds-disable") || duying1[2].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DY = "1";
                                //}
                            }
                            //让球
                            XmlNodeList rangqiu1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/b");
                            if (rangqiu1.Count == 2)
                            {
                                o1.Odds_RQZY = rangqiu1[0].InnerText;
                                o1.Odds_RQKY = rangqiu1[1].InnerText;
                                //o1.IsDisable_RQ = "0";
                                //if (rangqiu1[0].Attributes["class"].Value.Contains("odds-disable") || rangqiu1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_RQ = "1";
                                //}
                            }
                            XmlNodeList rangqiutext1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/em");
                            if (rangqiutext1.Count == 2)
                            {
                                o1.Text_ZRKQ = rangqiutext1[0].InnerText;
                                o1.Text_KRZQ = rangqiutext1[1].InnerText;
                            }
                            //大小
                            XmlNodeList daxiao1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/b");
                            if (daxiao1.Count == 2)
                            {
                                o1.Odds_DQ = daxiao1[0].InnerText;
                                o1.Odds_XQ = daxiao1[1].InnerText;
                                //o1.IsDisable_DX = "0";
                                //if (daxiao1[0].Attributes["class"].Value.Contains("odds-disable") || daxiao1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DX = "1";
                                //}
                            }
                            XmlNodeList daxiaotext1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/em");
                            if (daxiaotext1.Count == 2)
                            {
                                o1.Text_DQ = daxiaotext1[0].InnerText;
                                o1.Text_XQ = daxiaotext1[1].InnerText;
                            }
                            //单双
                            XmlNodeList dansuang1 = bisairow.ChildNodes[2].ChildNodes[3].SelectNodes("li/b");
                            if (dansuang1.Count == 2)
                            {
                                o1.Odds_D = dansuang1[0].InnerText;
                                o1.Odds_S = dansuang1[1].InnerText;
                                //o1.IsDisable_DS = "0";
                                //if (dansuang1[0].Attributes["class"].Value.Contains("odds-disable") || dansuang1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DS = "1";
                                //}
                            }

                            m.FullCourtList.Add(o1);                            
                        }
                        lm.MatchList.Add(m);
                    }
                    lmList.Add(lm);
                }
            }
            return lmList;
        }
        #endregion

        #region 乒乓球
        /// <summary>
        /// 沙巴乒乓球早盘列表，胜负盘 & 让局 & 单/双 & 大小
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<LeagueMatch> SB_Pingpong_ZP_DRDD_List(string html)
        {
            html = html.Replace("&nbsp;", "");
            html = "<root>" + html + "</root>";
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            var doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            TextReader tr = new StringReader(html);
            XmlReader reader = XmlReader.Create(tr, settings);
            //doc.LoadXml(html);
            doc.Load(reader);
            var rowList = doc.DocumentElement.ChildNodes;
            if (rowList != null)
            {
                foreach (XmlNode rowNode in rowList)
                {
                    if (rowNode.Attributes["class"].Value.Contains("match-odds-title"))
                    {
                        continue;
                    }
                    LeagueMatch lm = new LeagueMatch();
                    lm.Name = rowNode.SelectSingleNode("h2/span").InnerText;
                    if (string.IsNullOrEmpty(lm.Name) || FilterLeagueMatch(7, lm.Name))
                    {
                        continue;
                    }
                    var bisaidiv = rowNode.ChildNodes;
                    foreach (XmlNode bisai in bisaidiv)
                    {
                        if (bisai.Name.ToUpper() == "H2")
                        {
                            continue;
                        }
                        Match m = new Match();
                        bool init = true;
                        foreach (XmlNode bisairow in bisai.ChildNodes)
                        {
                            if (init)
                            {
                                XmlNodeList timenode = bisairow.ChildNodes[0].SelectNodes("ul/li/span");
                                m.time = timenode[0].InnerText + " " + timenode[1].InnerText;
                                if (timenode.Count >= 3)
                                {
                                    m.GQ = timenode[2].InnerText;
                                }
                                XmlNodeList teamnode = bisairow.ChildNodes[1].SelectNodes("ul/li/span");
                                m.HomeTeam = teamnode[0].InnerText;
                                m.VisitingTeam = teamnode[1].InnerText;
                                m.statustext = "";
                            }
                            init = false;
                            //全场赔率================================================
                            Odds o1 = new Odds() { type = 1 };
                            //独赢
                            XmlNodeList duying1 = bisairow.ChildNodes[2].ChildNodes[0].SelectNodes("li/b");
                            if (duying1.Count == 2)
                            {
                                o1.Odds_ZY = duying1[0].InnerText;
                                o1.Odds_KY = duying1[1].InnerText;
                                //o1.IsDisable_DY = "0";
                                //if (duying1[0].Attributes["class"].Value.Contains("odds-disable") || duying1[1].Attributes["class"].Value.Contains("odds-disable") || duying1[2].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DY = "1";
                                //}
                            }
                            //让球
                            XmlNodeList rangqiu1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/b");
                            if (rangqiu1.Count == 2)
                            {
                                o1.Odds_RQZY = rangqiu1[0].InnerText;
                                o1.Odds_RQKY = rangqiu1[1].InnerText;
                                //o1.IsDisable_RQ = "0";
                                //if (rangqiu1[0].Attributes["class"].Value.Contains("odds-disable") || rangqiu1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_RQ = "1";
                                //}
                            }
                            XmlNodeList rangqiutext1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/em");
                            if (rangqiutext1.Count == 2)
                            {
                                o1.Text_ZRKQ = rangqiutext1[0].InnerText;
                                o1.Text_KRZQ = rangqiutext1[1].InnerText;
                            }
                            //大小
                            XmlNodeList daxiao1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/b");
                            if (daxiao1.Count == 2)
                            {
                                o1.Odds_DQ = daxiao1[0].InnerText;
                                o1.Odds_XQ = daxiao1[1].InnerText;
                                //o1.IsDisable_DX = "0";
                                //if (daxiao1[0].Attributes["class"].Value.Contains("odds-disable") || daxiao1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DX = "1";
                                //}
                            }
                            XmlNodeList daxiaotext1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/em");
                            if (daxiaotext1.Count == 2)
                            {
                                o1.Text_DQ = daxiaotext1[0].InnerText;
                                o1.Text_XQ = daxiaotext1[1].InnerText;
                            }
                            //单双
                            XmlNodeList dansuang1 = bisairow.ChildNodes[2].ChildNodes[3].SelectNodes("li/b");
                            if (dansuang1.Count == 2)
                            {
                                o1.Odds_D = dansuang1[0].InnerText;
                                o1.Odds_S = dansuang1[1].InnerText;
                                //o1.IsDisable_DS = "0";
                                //if (dansuang1[0].Attributes["class"].Value.Contains("odds-disable") || dansuang1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DS = "1";
                                //}
                            }

                            m.FullCourtList.Add(o1);
                        }
                        lm.MatchList.Add(m);
                    }
                    lmList.Add(lm);
                }
            }
            return lmList;
        }
        /// <summary>
        /// 沙巴乒乓球滚球列表，胜负盘 & 让局 & 单/双 & 大小
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static List<LeagueMatch> SB_Pingpong_GQ_DRDD_List(string html)
        {
            html = html.Replace("&nbsp;", "");
            html = "<root>" + html + "</root>";
            List<LeagueMatch> lmList = new List<LeagueMatch>();
            var doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            TextReader tr = new StringReader(html);
            XmlReader reader = XmlReader.Create(tr, settings);
            //doc.LoadXml(html);
            doc.Load(reader);
            var rowList = doc.DocumentElement.ChildNodes;
            if (rowList != null)
            {
                foreach (XmlNode rowNode in rowList)
                {
                    if (rowNode.Attributes["class"].Value.Contains("match-odds-title"))
                    {
                        continue;
                    }
                    LeagueMatch lm = new LeagueMatch();
                    lm.Name = rowNode.SelectSingleNode("h2/span").InnerText;
                    if (string.IsNullOrEmpty(lm.Name) || FilterLeagueMatch(7, lm.Name))
                    {
                        continue;
                    }
                    var bisaidiv = rowNode.ChildNodes;
                    foreach (XmlNode bisai in bisaidiv)
                    {
                        if (bisai.Name.ToUpper() == "H2")
                        {
                            continue;
                        }
                        Match m = new Match();
                        m.statustext = "";
                        bool init = true;
                        foreach (XmlNode bisairow in bisai.ChildNodes)
                        {
                            if (init)
                            {
                                //沙巴暂时只显示滚球0-0
                                //XmlNodeList scorenode = bisairow.ChildNodes[0].SelectNodes("ul/li/span");
                                //if (scorenode.Count >= 4)
                                //{
                                //    string temp = scorenode[0].InnerText.Trim();
                                //    if (temp.Contains("'"))
                                //    {
                                //        int ts = -1;
                                //        string[] tsarr = temp.Split(' ');
                                //        int.TryParse(tsarr[1].Trim('\''), out ts);
                                //        if (tsarr[0].Contains("H"))
                                //        {
                                //            if (ts == -1)
                                //            {
                                //                ts = 1;
                                //            }
                                //        }
                                //        m.timing = ts.ToString();
                                //        m.halftype = tsarr[0];
                                //    }
                                //    else
                                //    {
                                //        m.timing = "-1";
                                //        m.statustext = temp;
                                //    }
                                //    //m.timing = scorenode[0].InnerText;
                                //    m.HomeTeamScore = scorenode[1].InnerText;
                                //    m.VisitingTeamScore = scorenode[3].InnerText;
                                //}
                                XmlNodeList teamnode = bisairow.ChildNodes[1].SelectNodes("ul/li/span");
                                m.HomeTeam = teamnode[0].InnerText;
                                m.VisitingTeam = teamnode[1].InnerText;
                            }
                            init = false;
                            //全场赔率================================================
                            Odds o1 = new Odds() { type = 1 };
                            //独赢
                            XmlNodeList duying1 = bisairow.ChildNodes[2].ChildNodes[0].SelectNodes("li/b");
                            if (duying1.Count == 2)
                            {
                                o1.Odds_ZY = duying1[0].InnerText;
                                o1.Odds_KY = duying1[1].InnerText;
                                //o1.IsDisable_DY = "0";
                                //if (duying1[0].Attributes["class"].Value.Contains("odds-disable") || duying1[1].Attributes["class"].Value.Contains("odds-disable") || duying1[2].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DY = "1";
                                //}
                            }
                            //让球
                            XmlNodeList rangqiu1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/b");
                            if (rangqiu1.Count == 2)
                            {
                                o1.Odds_RQZY = rangqiu1[0].InnerText;
                                o1.Odds_RQKY = rangqiu1[1].InnerText;
                                //o1.IsDisable_RQ = "0";
                                //if (rangqiu1[0].Attributes["class"].Value.Contains("odds-disable") || rangqiu1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_RQ = "1";
                                //}
                            }
                            XmlNodeList rangqiutext1 = bisairow.ChildNodes[2].ChildNodes[1].SelectNodes("li/em");
                            if (rangqiutext1.Count == 2)
                            {
                                o1.Text_ZRKQ = rangqiutext1[0].InnerText;
                                o1.Text_KRZQ = rangqiutext1[1].InnerText;
                            }
                            //大小
                            XmlNodeList daxiao1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/b");
                            if (daxiao1.Count == 2)
                            {
                                o1.Odds_DQ = daxiao1[0].InnerText;
                                o1.Odds_XQ = daxiao1[1].InnerText;
                                //o1.IsDisable_DX = "0";
                                //if (daxiao1[0].Attributes["class"].Value.Contains("odds-disable") || daxiao1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DX = "1";
                                //}
                            }
                            XmlNodeList daxiaotext1 = bisairow.ChildNodes[2].ChildNodes[2].SelectNodes("li/em");
                            if (daxiaotext1.Count == 2)
                            {
                                o1.Text_DQ = daxiaotext1[0].InnerText;
                                o1.Text_XQ = daxiaotext1[1].InnerText;
                            }
                            //单双
                            XmlNodeList dansuang1 = bisairow.ChildNodes[2].ChildNodes[3].SelectNodes("li/b");
                            if (dansuang1.Count == 2)
                            {
                                o1.Odds_D = dansuang1[0].InnerText;
                                o1.Odds_S = dansuang1[1].InnerText;
                                //o1.IsDisable_DS = "0";
                                //if (dansuang1[0].Attributes["class"].Value.Contains("odds-disable") || dansuang1[1].Attributes["class"].Value.Contains("odds-disable"))
                                //{
                                //    o1.IsDisable_DS = "1";
                                //}
                            }

                            m.FullCourtList.Add(o1);
                        }
                        lm.MatchList.Add(m);
                    }
                    lmList.Add(lm);
                }
            }
            return lmList;
        }
        #endregion

        #region 其他
        // 
        #endregion

        #region 屏蔽联赛
        public static Dictionary<int, string[]> filterKeys = new Dictionary<int, string[]>()
        {
            //足球
            {1,new string[] { "梦幻对垒", "角球", "特定15分钟", "测试", "主场/客场", "哪一队先开球", "半场结束前受伤延长补时", "哪一队可晉级", "哪一队可晋级", "总入球分钟" }},
            //篮球
            {2,new string[] { "三分球最多之球队", "得分最多", "梦幻对垒", "第一节", "第二节", "第三节", "第四节" }},
            //网球
            {3,new string[] { "获胜者", "最多发球得分", "最多个双误", "赛局获胜者" }},
            //排球
            {4,new string[] { "第一局", "第二局", "第三局", "第四局", "第五局", "总分获胜队" }},
            //棒球
            {5,new string[] {  }},
            //羽毛球
            {6,new string[] { "第一局", "第二局", "第三局" }},
            //乒乓球
            {7,new string[] { "第一局", "第二局", "第三局", "第四局", "第五局", "第六局", "第七局" }},
            //美式足球
            {8,new string[] { "第一节", "第二节", "第三节", "第四节", "队伍源自地" }},
        };
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <returns>true包含</returns>
        public static bool FilterLeagueMatch(int key,string name)
        {
            bool flag = false;
            foreach (string item in filterKeys[key])
            {
                if (name.Contains(item))
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }
        #endregion

    }
}
