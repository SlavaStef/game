﻿using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PokerHand.Common.Helpers;

namespace PokerHand.Common.Entities
{
    public class Card
    {
        public CardRankNumber Rank { get; set; }
        public SuitTypeNumber Suit { get; set; }
    }
}