using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using ScarletTeleports.Utils;

namespace ScarletTeleports.Systems;

public static class SystemMessages {
  public static void Send(User user, string message) {
    var messageBytes = new FixedString512Bytes(message.White());
    ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, user, ref messageBytes);
  }

  public static void SendAll(string message) {
    var messageBytes = new FixedString512Bytes(message.White());
    ServerChatUtils.SendSystemMessageToAllClients(Core.EntityManager, ref messageBytes);
  }

  public static void SendAdmins(string message) {
    var messageBytes = new FixedString512Bytes(message.White());
    var admins = Core.Players.GetAdmins();

    foreach (var admin in admins) {
      var user = admin.UserEntity.Read<User>();
      ServerChatUtils.SendSystemMessageToClient(Core.EntityManager, user, ref messageBytes);
    }
  }
}