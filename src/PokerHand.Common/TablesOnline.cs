using System;
using System.Collections.Generic;
using System.Linq;
using PokerHand.Common.Helpers.Table;

namespace PokerHand.Common
{
    public interface ITablesOnline
    {
        Table GetById(Guid tableId);
        Table GetByPlayerId(Guid playerId);
        List<Table> GetManyByTitle(TableTitle title);
        void Add(Table table);
        void Remove(Guid tableId);
    }
    
    public class TablesOnline : ITablesOnline
    {
        private List<Table> Tables { get; set; }
        
        public TablesOnline()
        {
            Tables = new List<Table>();
        }

        public Table GetById(Guid tableId) =>
            Tables
                .FirstOrDefault(t => t.Id == tableId);

        public Table GetByPlayerId(Guid playerId) => 
            Tables
                .FirstOrDefault(t => t.Players.FirstOrDefault(p => p.Id == playerId) != null);
        
        public List<Table> GetManyByTitle(TableTitle title) =>
            Tables.Where(t => t.Title == title).ToList();

        public void Add(Table table) =>
            Tables.Add(table);

        public void Remove(Guid tableId)
        {
            if (Tables.Any(t => t.Id == tableId))
                Tables.Remove(Tables.First(t => t.Id == tableId));
        }
    }
}