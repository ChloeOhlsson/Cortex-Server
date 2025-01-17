using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using MySql.Data.MySqlClient;

using Cortex.Server.Game.Users;
using Cortex.Server.Game.Rooms;

using Cortex.Server.Socket.Clients;
using Cortex.Server.Socket.Events;
using Cortex.Server.Socket.Messages;

using Cortex.Server.Game.Furnitures;
using Cortex.Server.Game.Shop.Furnitures;

namespace Cortex.Server.Game.Shop.Events {
    class OnShopFurniturePurchase : ISocketEvent {
        public string Event => "OnShopFurniturePurchase";

        public int Execute(SocketClient client, JToken data) {
            int id = data.ToObject<int>();

            GameShopFurniture shopFurniture = GameShop.Furnitures.Find(x => x.Id == id);

            if(shopFurniture == null)
                return 0;

            using MySqlConnection connection = new MySqlConnection(Program.Connection);
            connection.Open();

            using MySqlCommand command = new MySqlCommand("INSERT INTO user_furnitures (user, furniture) VALUES (@user, @furniture)", connection);

            command.Parameters.AddWithValue("@user", client.User.Id);
            command.Parameters.AddWithValue("@furniture", shopFurniture.Furniture.Id);

            int result = command.ExecuteNonQuery();

            if(result == 0)
                return 0;

            client.User.Furnitures.Add(GameFurnitureManager.GetGameUserFurniture((int)command.LastInsertedId));

            client.Send(new SocketMessage("OnShopFurniturePurchase", true).Compose());

            client.Send(new SocketMessage("OnUserFurnitureUpdate", client.User.GetFurnitureMessages(shopFurniture.Furniture.Id)).Compose());

            return 1;
        }
    }
}
