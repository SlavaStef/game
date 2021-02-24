﻿using System.Collections.Generic;

namespace PokerHand.Common.Helpers.Table
{
    public static class TableOptions
    {
        public static readonly Dictionary<string, Dictionary<string, int>> Tables =
            new Dictionary<string, Dictionary<string, int>>
            {
                {"Dash", new Dictionary<string, int>
                    {
                        {"TableType", 4},
                        {"Experience", 0},
                        {"SmallBlind", 50},
                        {"BigBlind", 100},
                        {"MinBuyIn", 1000},
                        {"MaxBuyIn", 10000},
                        {"MaxPlayers", 5}
                    }
                },
                {"Lowball", new Dictionary<string, int>
                    {
                        {"TableType", 1},
                        {"Experience", 0},
                        {"SmallBlind", 50},
                        {"BigBlind", 100},
                        {"MinBuyIn", 1000},
                        {"MaxBuyIn", 10000},
                        {"MaxPlayers", 8}
                    }
                },
                {"BeachClub", new Dictionary<string, int>
                    {
                        {"TableType", 2},
                        {"Experience", 25},
                        {"SmallBlind", 100},
                        {"BigBlind", 200},
                        {"MinBuyIn", 4000},
                        {"MaxBuyIn", 40000},
                        {"MaxPlayers", 5}
                    }
                },
                {"IbizaDisco", new Dictionary<string, int>
                    {
                        {"TableType", 3},
                        {"Experience", 25},
                        {"SmallBlind", 250},
                        {"BigBlind", 500},
                        {"MinBuyIn", 10000},
                        {"MaxBuyIn", 60000},
                        {"MaxPlayers", 8}
                    }
                },
                {"WetDeskLounge", new Dictionary<string, int>
                    {
                        {"TableType", 0},
                        {"Experience", 25},
                        {"SmallBlind", 500},
                        {"BigBlind", 1000},
                        {"MinBuyIn", 50000},
                        {"MaxBuyIn", 50000},
                        {"MaxPlayers", 8}
                    }
                },
                {"RivieraHotel", new Dictionary<string, int>
                    {
                        {"TableType", 5},
                        {"Experience", 25},
                        {"SmallBlind", 10},
                        {"BigBlind", 20},
                        {"MinBuyIn", 10_000},
                        {"MaxBuyIn", 10_000},
                        {"MaxPlayers", 5},
                        {"InitialStack", 3000},
                        {"FirstPlacePrize", 20_000},
                        {"SecondPlacePrize", 10_000}
                    }
                },
                {"ShishaBar", new Dictionary<string, int>
                    {
                        {"TableType", 0},
                        {"Experience", 25},
                        {"SmallBlind", 50},
                        {"BigBlind", 100},
                        {"MinBuyIn", 2000},
                        {"MaxBuyIn", 10000},
                        {"MaxPlayers", 8}
                    }
                },
                {"TropicalHouse", new Dictionary<string, int>
                    {
                        {"TableType", 0},
                        {"Experience", 25},
                        {"SmallBlind", 25},
                        {"BigBlind", 50},
                        {"MinBuyIn", 500},
                        {"MaxBuyIn", 5000},
                        {"MaxPlayers", 5}
                    }
                },
                {"SevenNightsClub", new Dictionary<string, int>
                    {
                        {"TableType", 2},
                        {"Experience", 30},
                        {"SmallBlind", 2500},
                        {"BigBlind", 5000},
                        {"MinBuyIn", 300000},
                        {"MaxBuyIn", 300000},
                        {"MaxPlayers", 5}
                    }
                },
                {"BlueMoonYacht", new Dictionary<string, int>
                    {
                        {"TableType", 3},
                        {"Experience", 30},
                        {"SmallBlind", 5000},
                        {"BigBlind", 10000},
                        {"MinBuyIn", 100000},
                        {"MaxBuyIn", 500000},
                        {"MaxPlayers", 8}
                    }
                },
                {"CityDreamsResort", new Dictionary<string, int>
                    {
                        {"TableType", 5},
                        {"Experience", 30},
                        {"SmallBlind", 20},
                        {"BigBlind", 40},
                        {"MinBuyIn", 50_000},
                        {"MaxBuyIn", 50_000},
                        {"MaxPlayers", 5},
                        {"InitialStack", 3000},
                        {"FirstPlacePrize", 170_000},
                        {"SecondPlacePrize", 50_000}
                    }
                },
                {"MirageCasino", new Dictionary<string, int>
                    {
                        {"TableType", 0},
                        {"Experience", 30},
                        {"SmallBlind", 20},
                        {"BigBlind", 40},
                        {"MinBuyIn", 50000},
                        {"MaxBuyIn", 50000},
                        {"MaxPlayers", 5}
                    }
                },
                {"SunriseCafe", new Dictionary<string, int>
                    {
                        {"TableType", 0},
                        {"Experience", 30},
                        {"SmallBlind", 1000},
                        {"BigBlind", 2000},
                        {"MinBuyIn", 20000},
                        {"MaxBuyIn", 100000},
                        {"MaxPlayers", 5}
                    }
                },
                {"DesertCaveHotel", new Dictionary<string, int>
                    {
                        {"TableType", 2},
                        {"Experience", 40},
                        {"SmallBlind", 12500},
                        {"BigBlind", 25000},
                        {"MinBuyIn", 500000},
                        {"MaxBuyIn", 2_000_000},
                        {"MaxPlayers", 5}
                    }
                },
                {"GoldMine", new Dictionary<string, int>
                    {
                        {"TableType", 0},
                        {"Experience", 40},
                        {"SmallBlind", 10000},
                        {"BigBlind", 20000},
                        {"MinBuyIn", 200000},
                        {"MaxBuyIn", 1_000_000},
                        {"MaxPlayers", 8}
                    }
                },
                {"ImperialBunker", new Dictionary<string, int>
                    {
                        {"TableType", 0},
                        {"Experience", 40},
                        {"SmallBlind", 15000},
                        {"BigBlind", 30000},
                        {"MinBuyIn", 2_000_000},
                        {"MaxBuyIn", 2_000_000},
                        {"MaxPlayers", 8}
                    }
                },
                {"TradesmanClub", new Dictionary<string, int>
                    {
                        {"TableType", 3},
                        {"Experience", 50},
                        {"SmallBlind", 100000},
                        {"BigBlind", 200000},
                        {"MinBuyIn", 25_000_000},
                        {"MaxBuyIn", 25_000_000},
                        {"MaxPlayers", 8}
                    }
                },
                {"HeritageBank", new Dictionary<string, int>
                    {
                        {"TableType", 5},
                        {"Experience", 50},
                        {"SmallBlind", 25},
                        {"BigBlind", 50},
                        {"MinBuyIn", 500_000},
                        {"MaxBuyIn", 500_000},
                        {"MaxPlayers", 5},
                        {"InitialStack", 5000},
                        {"FirstPlacePrize", 1_700_000},
                        {"SecondPlacePrize", 500_000}
                    }
                },
                {"MillenniumHotel", new Dictionary<string, int>
                    {
                        {"TableType", 0},
                        {"Experience", 50},
                        {"SmallBlind", 25000},
                        {"BigBlind", 50000},
                        {"MinBuyIn", 1_000_000},
                        {"MaxBuyIn", 10_000_000},
                        {"MaxPlayers", 8}
                    }
                },
                {"FortuneHippodrome", new Dictionary<string, int>
                    {
                        {"TableType", 0},
                        {"Experience", 50},
                        {"SmallBlind", 50000},
                        {"BigBlind", 100000},
                        {"MinBuyIn", 2_000_000},
                        {"MaxBuyIn", 20_000_000},
                        {"MaxPlayers", 5}
                    }
                }
            };
    };
}