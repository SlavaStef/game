﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using PokerHand.Common.Helpers.Present;

namespace PokerHand.Server.Hubs
{
    public partial class GameHub
    {
        public async Task SendPresent(string senderIdJson, string recipientsIdsJson, string presentNameJson)
        {
            var senderId = JsonSerializer.Deserialize<Guid>(senderIdJson);
            var recipientsIds = JsonSerializer.Deserialize<List<Guid>>(recipientsIdsJson);
            var presentName = JsonSerializer.Deserialize<PresentName>(presentNameJson);

            await _presentService.SendPresent(senderId, recipientsIds, presentName);
        }
    }
}