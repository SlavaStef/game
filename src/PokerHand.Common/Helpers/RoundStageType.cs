namespace PokerHand.Common.Helpers
{
    public enum RoundStageType
    {
        None = 0,
        SetDealerAndBlinds = 1,
        DealPocketCards = 2,
        WageringPreFlopRound = 3,
        DealCommunityCards = 4,
        WageringSecondRound = 5,
        WageringThirdRound = 6,
        WageringFourthRound = 7,
        Showdown = 8
    }
}