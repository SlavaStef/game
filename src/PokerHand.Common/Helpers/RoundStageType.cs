namespace PokerHand.Common.Helpers
{
    public enum RoundStageType
    {
        NotStarted = 0,
        PrepareTable = 1,
        WageringPreFlopRound = 2,
        DealCommunityCards = 3,
        WageringSecondRound = 4,
        WageringThirdRound = 5,
        WageringFourthRound = 6,
        Showdown = 7,
        Refresh = 8
    }
}