using System;
using System.Collections.Concurrent;
using System.Linq;

namespace PokerHand.Common
{
    public interface IPlayersOnline
    {
        ConcurrentDictionary<Guid, string> GetAll();
        void Add(Guid playerId, string connectionId);
        void Remove(Guid playerId);
        void AddOrUpdate(Guid playerId, string newConnectionId);
        string GetValueByKey(Guid playerId);
        Guid GetKeyByValue(string connectionId);
        bool ContainsKey(Guid playerId);
    }
    
    public class PlayersOnline : IPlayersOnline
    {
        private ConcurrentDictionary<Guid, string> Players { get; set; }

        public PlayersOnline()
        {
            Players = new ConcurrentDictionary<Guid, string>();
        }

        public ConcurrentDictionary<Guid, string> GetAll() =>
            Players;

        public void Add(Guid playerId, string connectionId) => 
            Players.TryAdd(playerId, connectionId);

        public void Remove(Guid playerId) =>
            Players.TryRemove(playerId, out var connectionId);

        public void AddOrUpdate(Guid playerId, string newConnectionId) =>
            Players.AddOrUpdate(playerId, newConnectionId, (key, value) => newConnectionId);

        public string GetValueByKey(Guid playerId) => 
            Players
                .FirstOrDefault(p => p.Key == playerId)
                .Value;

        public Guid GetKeyByValue(string connectionId) =>
            Players
                .FirstOrDefault(p => p.Value == connectionId)
                .Key;

        public bool ContainsKey(Guid playerId) => 
            Players.ContainsKey(playerId);
    }
}