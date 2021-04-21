using System.Collections.Generic;

namespace PokerHand.Common.Helpers.Present
{
    public static class PresentOptions
    {
        public static readonly Dictionary<PresentName, int> Presents = new()
        {
            {PresentName.Pepper, 1000},
            {PresentName.Cake, 1500},
            {PresentName.Pizza, 2000},
            {PresentName.Onion, 2500},
            {PresentName.Lollipop, 3000},
            {PresentName.Chocolate, 5000},
            {PresentName.Coffee, 7000},
            {PresentName.Wine, 10_000},
            {PresentName.Napkins, 10_000},
            {PresentName.Rose, 15_000},
            {PresentName.Karamel, 20_000},
            {PresentName.BottleWine, 50_000},
            {PresentName.Clover, 50_000},
            {PresentName.EnergyDrink, 80_000},
            {PresentName.HeartBox, 100_000},
            {PresentName.Shampoo, 100_000},
            {PresentName.Glasses, 150_000},
            {PresentName.Flowers, 200_000},
            {PresentName.SmokingPipe, 300_000},
            {PresentName.Compass, 500_000},
            {PresentName.Heart, 500_000},
            {PresentName.Horseshoe, 700_000},
            {PresentName.Necklace, 1_000_000},
            {PresentName.Ring, 2_000_000},
            {PresentName.Brilliant, 3_000_000}
        };
    }
}