namespace PokerHand.Common.Helpers
{
    public enum RoundStageType
    {
        None = 0,
        SetDealerAndBlinds = 1,
        DealPocketCards = 2,
        PreFlopWagering = 3,
        DealCommunityCards = 4,
        Wagering = 5,
        Showdown = 6
    }
}