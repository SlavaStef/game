using System.Collections.Generic;
using PokerHand.Common.Entities;

namespace PokerHand.Common.Dto
{
    /// <summary>
    /// This object is sent to client after every change of table state.
    /// Current
    /// </summary>
    public class TableUpdateDto
    {
        public TableDto NewTableState { get; set; }
        public PlayerDto CurrentPlayer { get; set; }
        public List<Player> OtherPlayers { get; set; }
    }
}