/*using System.Collections.Generic;
using PokerHand.Common.Entities;

namespace PokerHand.Server.Helpers
{
    public class PotsManager
    {
        private Player Chair0 { get; set; }
        private Player Chair1 { get; set; }
        private Player Chair2 { get; set; }
        private Player Chair3 { get; set; }
        private Player Chair4 { get; set; }

        private int PotOfChair0 { get; set; }
        private int PotOfChair1 { get; set; }
        private int PotOfChair2 { get; set; }
        private int PotOfChair3 { get; set; }
        private int PotOfChair4 { get; set; }
        
        
        public List<SidePot> CalculateMainAndSidePots()
        {
            var finalPots = new List<SidePot>(5);

            var sidePotCounter = 0;
            
            while (GetZeroPotsAmount() < 4)
            {
                int smallestDeck = GetMinPotAmountGreaterThanZero();
                List<Player> contestedBy = new List<Player>();
                SidePot sidePot = new SidePot();

                // In that case, 5 is the max players occupied.
                for (int i = 0; i < 5; i++)
                {
                    if (GetPotByIndex(i) >= smallestDeck)
                    {
                        // Saves the original pot of the user
                        if (sidePot.OriginalPotAmount == 0)
                            sidePot.OriginalPotAmount = GetPotByIndex(i);

                        SetPotByIndex(i, GetPotByIndex(i) - smallestDeck);
                        contestedBy.Add(GetUserByIndex(i));
                    }
                }

                string potName = sidePotCounter == 0 ? "Main Pot" : "Side Pot " + sidePotCounter;
                sidePot.Name = potName;
                sidePot.PotAmount = smallestDeck * contestedBy.Count;
                sidePot.ContestedBy = contestedBy;

                finalPots.Add(sidePot);

                sidePotCounter++;
            }

            // Edge case where there's still money in the room's sum pot, in that case we need to return the money to the user owned it.
            if (GetZeroPotsAmount() == 4)
            {
                for (var i = 0; i < 5; i++)
                {
                    if (GetPotByIndex(i) > 0)
                    {
                        var user = GetUserByIndex(i);
                        user.StackMoney += GetPotByIndex(i);
                        SetPotByIndex(i, 0);

                        break;
                    }
                }
            }

            return finalPots;
        }
        
        private int GetZeroPotsAmount()
        {
            var zeroCounts = 0;
            
            for (var i = 0; i < 5; i++)
            {
                if (GetPotByIndex(i) == 0)
                    zeroCounts++;
            }

            return zeroCounts;
        }
        
        // Will return zero if and only if getZeroPotAmount() = 5
        private int GetMinPotAmountGreaterThanZero()
        {
            var minAmount = int.MaxValue;
            
            for (var i = 0; i < 5; i++)
            {
                var getPot = GetPotByIndex(i);
                
                if (minAmount > getPot && getPot > 0)
                {
                    minAmount = getPot;
                }
            }

            return minAmount;
        }
        
        private int GetPotByIndex(int index)
        {
            return index switch
            {
                0 => PotOfChair0,
                1 => PotOfChair1,
                2 => PotOfChair2,
                3 => PotOfChair3,
                4 => PotOfChair4,
                _ => -1
            };
        }

        private void SetPotByIndex(int index, int amount)
        {
            switch (index)
            {
                case 0: PotOfChair0 = amount; break;
                case 1: PotOfChair1 = amount; break;
                case 2: PotOfChair2 = amount; break;
                case 3: PotOfChair3 = amount; break;
                case 4: PotOfChair4 = amount; break;
            }
        }

        private Player GetUserByIndex(int index)
        {
            return index switch
            {
                0 => Chair0,
                1 => Chair1,
                2 => Chair2,
                3 => Chair3,
                4 => Chair4,
                _ => null
            };
        }
    }
    
    public class SidePot
    {
        public string Name { get; set; } // for example Main Pot, Side Pot 1, Side Pot 2 , ...
        public int PotAmount { get; set; }
        public int OriginalPotAmount { get; set; }
        public List<Player> ContestedBy { get; set; }
        public List<Player> Winners { get; set; }
        public string RankName { get; set; } // For example: Full House, Straight , High Card , ...
        public List<string[]> WinningCards { get; set; }
    }
}*/