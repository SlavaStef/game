using System;
using System.Collections.Generic;

namespace PokerHand.Common.Helpers.Present
{
    public class Present
    {
        public PresentName Name { get; set; }
        public int SenderIndexNumber { get; set; }
        public List<int> RecipientsIndexNumbers { get; set; }
    }
}