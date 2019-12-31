using System;
using System.Collections;
using System.Collections.Generic;
using ClusterVR.InternalSDK.Core;

public class RankingScreenPresenter
{
    List<IRankingScreenView> rankingScreenViews;

    public RankingScreenPresenter(List<IRankingScreenView> rankingScreenViews)
    {
        this.rankingScreenViews = rankingScreenViews;
    }

    public void SetRanking(int playerCount)
    {
        var rankingData = GenerateRankingData(playerCount);
        foreach (var rankingScreenView in rankingScreenViews)
        {
            rankingScreenView.UpdateCells(rankingData.rankings, rankingData.selfRanking);
        }
    }

    RankingData GenerateRankingData(int playerCount)
    {
        var rankingData = new RankingData();
        rankingData.rankings = new List<Ranking>();
        for (int i = 0; i < playerCount; i++)
        {
            var user = new User( "displayName" + i,  "userName" + i, _ => {});
            var ranking = new Ranking(i,user);
            if (i == 0)
            {
                rankingData.selfRanking = ranking;
            }
            rankingData.rankings.Add(ranking);
        }

        return rankingData;
    }
    struct RankingData
    {
        public List<Ranking> rankings;
        public Ranking selfRanking;
    }

}
