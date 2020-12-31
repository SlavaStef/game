namespace PokerHand.Common.Dto
{
    public class TableInfoDto
    {
        public string Title { get; set; }
        public int TableType { get; set; }
        public int Experience { get; set; }
        public int SmallBlind { get; set; }
        public int BigBlind { get; set; }
        public int MinBuyIn { get; set; }
        public int MaxBuyIn { get; set; }
        public int MaxPlayers { get; set; }
    }
}