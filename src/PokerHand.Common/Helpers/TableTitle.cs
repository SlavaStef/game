using System.Collections.Generic;

namespace PokerHand.Common.Helpers
{
    public enum TableTitle
    {
        Dash = 1,
        Lowball = 2,
        Private = 3,
        Tournament = 4,
        TropicalHouse = 5,
        WetDeskLounge = 6,
        IbizaDisco = 7,
        ShishaBar = 8,
        BeachClub = 9,
        RivieraHotel = 10,
        SevenNightsClub = 11,
        MirageCasino = 12,
        CityDreamsResort = 13,
        SunriseCafe = 14,
        BlueMoonYacht = 15,
        GoldMine = 16,
        DesertCaveHotel = 17,
        ImperialBunker = 18,
        MillenniumHotel = 19,
        TradesmanClub = 20,
        FortuneHippodrome = 21,
        HeritageBank = 22
    }

    public static class TableOptions
    {
        public static readonly Dictionary<string, int> TropicalHouseOptions = new Dictionary<string, int>()
        {
            {"Type", 1},
            {"MaxPlayers", 3},
            {"SmallBlind", 25},
            {"BigBlind", 50}
        };
    }
}