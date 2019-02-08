using GTANetworkAPI;
using WiredPlayers.globals;
using WiredPlayers.factions;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.success;
using System.Linq;
using System;

namespace WiredPlayers.chat
{
    public class Chat : Script
    {
        public static void OnPlayerDisconnected(Client player, DisconnectionType type, string reason)
        {
            // Deleting player's attached label
            if (player.HasData(EntityData.PLAYER_AME) == true)
            {
                TextLabel label = player.GetData(EntityData.PLAYER_AME);
                label.Detach();
                label.Delete();
            }
        }

        public static void SendMessageToNearbyPlayers(Client player, string message, int type, float range, bool excludePlayer = false)
        {
            string secondMessage = string.Empty;
            float distanceGap = range / Constants.CHAT_RANGES;            

            if (message.Length > Constants.CHAT_LENGTH)
            {
                // We need two lines to show the message
                secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
            }

            foreach (Client target in NAPI.Pools.GetAllPlayers())
            {
                if (target.HasData(EntityData.PLAYER_PLAYING) && player.Dimension == target.Dimension)
                {
                    if (player != target || (player == target && !excludePlayer))
                    {
                        float distance = player.Position.DistanceTo(target.Position);

                        if (distance <= range)
                        {
                            // Getting message color
                            string chatMessageColor = GetChatMessageColor(distance, distanceGap, true);
                            string oocMessageColor = GetChatMessageColor(distance, distanceGap, false);

                            switch (type)
                            {
                                case Constants.MESSAGE_TALK:
                                    // We send the message
                                   target.SendChatMessage(secondMessage.Length > 0 ? chatMessageColor + player.Name + GenRes.chat_say + message + "..." : chatMessageColor + player.Name + GenRes.chat_say + message);
                                    if (secondMessage.Length > 0)
                                    {
                                       target.SendChatMessage(chatMessageColor + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_YELL:
                                    // We send the message
                                   target.SendChatMessage(secondMessage.Length > 0 ? chatMessageColor + player.Name + GenRes.chat_yell + message + "..." : chatMessageColor + player.Name + GenRes.chat_yell + message + "!");
                                    if (secondMessage.Length > 0)
                                    {
                                       target.SendChatMessage(chatMessageColor + secondMessage + "!");
                                    }
                                    break;
                                case Constants.MESSAGE_WHISPER:
                                    // We send the message
                                   target.SendChatMessage(secondMessage.Length > 0 ? chatMessageColor + player.Name + GenRes.chat_whisper + message + "..." : chatMessageColor + player.Name + GenRes.chat_whisper + message);
                                    if (secondMessage.Length > 0)
                                    {
                                       target.SendChatMessage(chatMessageColor + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_PHONE:
                                    // We send the message
                                   target.SendChatMessage(secondMessage.Length > 0 ? chatMessageColor + player.Name + GenRes.chat_phone + message + "..." : chatMessageColor + player.Name + GenRes.chat_phone + message);
                                    if (secondMessage.Length > 0)
                                    {
                                       target.SendChatMessage(chatMessageColor + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_RADIO:
                                    // We send the message
                                   target.SendChatMessage(secondMessage.Length > 0 ? chatMessageColor + player.Name + GenRes.chat_radio + message + "..." : chatMessageColor + player.Name + GenRes.chat_radio + message);
                                    if (secondMessage.Length > 0)
                                    {
                                       target.SendChatMessage(chatMessageColor + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_ME:
                                    // We send the message
                                   target.SendChatMessage(secondMessage.Length > 0 ? Constants.COLOR_CHAT_ME + player.Name + " " + message + "..." : Constants.COLOR_CHAT_ME + player.Name + " " + message);
                                    if (secondMessage.Length > 0)
                                    {
                                       target.SendChatMessage(Constants.COLOR_CHAT_ME + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_DO:
                                    // We send the message
                                   target.SendChatMessage(secondMessage.Length > 0 ? Constants.COLOR_CHAT_DO + "[ID: " + player.Value + "] " + message + "..." : Constants.COLOR_CHAT_DO + "[ID: " + player.Value + "] " + message);
                                    if (secondMessage.Length > 0)
                                    {
                                       target.SendChatMessage(Constants.COLOR_CHAT_DO + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_OOC:
                                    // We send the message
                                   target.SendChatMessage(secondMessage.Length > 0 ? oocMessageColor + "(([ID: " + player.Value + "] " + player.Name + ": " + message + "..." : oocMessageColor + "(([ID: " + player.Value + "] " + player.Name + ": " + message + "))");
                                    if (secondMessage.Length > 0)
                                    {
                                       target.SendChatMessage(oocMessageColor + secondMessage + "))");
                                    }
                                    break;
                                case Constants.MESSAGE_DISCONNECT:
                                    // We send the message
                                   target.SendChatMessage(secondMessage.Length > 0 ? Constants.COLOR_HELP + "[ID: " + player.Value + "] " + player.Name + ": " + message + "..." : Constants.COLOR_HELP + "[ID: " + player.Value + "] " + player.Name + ": " + message);
                                    if (secondMessage.Length > 0)
                                    {
                                       target.SendChatMessage(Constants.COLOR_HELP + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_MEGAPHONE:
                                    // We send the message
                                   target.SendChatMessage(secondMessage.Length > 0 ? Constants.COLOR_INFO + "[Megáfono de " + player.Name + "]: " + message + "..." : Constants.COLOR_INFO + "[Megáfono de " + player.Name + "]: " + message);
                                    if (secondMessage.Length > 0)
                                    {
                                       target.SendChatMessage(Constants.COLOR_INFO + secondMessage);
                                    }
                                    break;
                                case Constants.MESSAGE_SU_TRUE:
                                    // We send the message
                                    message = string.Format(SuccRes.possitive_result, player.Name);
                                   target.SendChatMessage(Constants.COLOR_SU_POSITIVE + message);
                                    break;
                                case Constants.MESSAGE_SU_FALSE:
                                    // We send the message
                                    message = string.Format(ErrRes.negative_result, player.Name);
                                   target.SendChatMessage(Constants.COLOR_ERROR + message);
                                    break;
                            }
                        }
                    }
                }
            }
        }
        
        private static string GetChatMessageColor(float distance, float distanceGap, bool ooc)
        {
            string color = null;
            if (distance < distanceGap)
            {
                color = ooc ? Constants.COLOR_OOC_CLOSE : Constants.COLOR_CHAT_CLOSE;
            }
            else if (distance < distanceGap * 2)
            {
                color = ooc ? Constants.COLOR_OOC_NEAR : Constants.COLOR_CHAT_NEAR;
            }
            else if (distance < distanceGap * 3)
            {
                color = ooc ? Constants.COLOR_OOC_MEDIUM : Constants.COLOR_CHAT_MEDIUM;
            }
            else if (distance < distanceGap * 4)
            {
                color = ooc ? Constants.COLOR_OOC_FAR : Constants.COLOR_CHAT_FAR;
            }
            else
            {
                color = ooc ? Constants.COLOR_OOC_LIMIT : Constants.COLOR_CHAT_LIMIT;
            }
            return color;
        }

        [ServerEvent(Event.ChatMessage)]
        public void OnChatMessage(Client player, string message)
        {
            if (player.HasData(EntityData.PLAYER_PLAYING) == false)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_cant_chat);
            }
            else if (player.GetData(EntityData.PLAYER_KILLED) != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else if (player.HasData(EntityData.PLAYER_ON_AIR) == true)
            {
                WeazelNews.SendNewsMessage(player, message);
            }
            else if (player.HasData(EntityData.PLAYER_PHONE_TALKING) == true)
            {
                // Target player of the message
                Client target = player.GetData(EntityData.PLAYER_PHONE_TALKING);
                
                string secondMessage = string.Empty;

                if (message.Length > Constants.CHAT_LENGTH)
                {
                    // We split the message in two lines
                    secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                    message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                }

                // We send the message to the player and target
                player.SendChatMessage(secondMessage.Length > 0 ? Constants.COLOR_CHAT_PHONE + GenRes.phone + player.Name + GenRes.chat_say + message + "..." : Constants.COLOR_CHAT_PHONE + GenRes.phone + player.Name + GenRes.chat_say + message);
               target.SendChatMessage(secondMessage.Length > 0 ? Constants.COLOR_CHAT_PHONE + GenRes.phone + player.Name + GenRes.chat_say + message + "..." : Constants.COLOR_CHAT_PHONE + GenRes.phone + player.Name + GenRes.chat_say + message);
                if (secondMessage.Length > 0)
                {
                    player.SendChatMessage(Constants.COLOR_CHAT_PHONE + secondMessage);
                   target.SendChatMessage(Constants.COLOR_CHAT_PHONE + secondMessage);
                }

                // We send the message to nearby players
                SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_PHONE, player.Dimension > 0 ? 7.5f : 10.0f, true);
            }
            else
            {
                SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_TALK, player.Dimension > 0 ? 7.5f : 10.0f);
                NAPI.Util.ConsoleOutput("[ID:" + player.Value + "]" + player.Name + GenRes.chat_say + message);
            }
        }

        [Command(Commands.COM_SAY, Commands.HLP_SAY_COMMAND, GreedyArg = true)]
        public void DecirCommand(Client player, string message)
        {
            if (player.GetData(EntityData.PLAYER_KILLED) != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else
            {
                SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_TALK, player.Dimension > 0 ? 7.5f : 10.0f);
            }
        }

        [Command(Commands.COM_YELL, Commands.HLP_YELL_COMMAND, GreedyArg = true)]
        public void GritarCommand(Client player, string message)
        {
            if (player.GetData(EntityData.PLAYER_KILLED) != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else
            {
                SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_YELL, 45.0f);
            }
        }

        [Command(Commands.COM_WHISPER, Commands.HLP_WHISPER_COMMAND, GreedyArg = true)]
        public void SusurrarCommand(Client player, string message)
        {
            if (player.GetData(EntityData.PLAYER_KILLED) != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else
            {
                SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_WHISPER, 3.0f);
            }
        }

        [Command(Commands.COM_ME, Commands.HLP_ME_COMMAND, GreedyArg = true)]
        public void MeCommand(Client player, string message)
        {
            SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_ME, player.Dimension > 0 ? 7.5f : 20.0f);
        }

        [Command(Commands.COM_DO, Commands.HLP_DO_COMMAND, GreedyArg = true)]
        public void DoCommand(Client player, string message)
        {
            SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_DO, player.Dimension > 0 ? 7.5f : 20.0f);
        }

        [Command(Commands.COM_OOC, Commands.HLP_OOC_COMMAND, GreedyArg = true)]
        public void OocCommand(Client player, string message)
        {
            SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_OOC, player.Dimension > 0 ? 5.0f : 10.0f);
        }

        [Command(Commands.COM_LUCK)]
        public void SuCommand(Client player)
        {
            if (player.GetData(EntityData.PLAYER_KILLED) != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else
            {
                Random random = new Random();
                int messageType = random.Next(0, 2) > 0 ? Constants.MESSAGE_SU_TRUE : Constants.MESSAGE_SU_FALSE;
                SendMessageToNearbyPlayers(player, string.Empty, messageType, 20.0f);
            }
        }

        [Command(Commands.COM_AME, Commands.HLP_AME_COMMAND, GreedyArg = true)]
        public void AmeCommand(Client player, string message = "")
        {
            if (player.HasData(EntityData.PLAYER_AME) == true)
            {
                // We get player's TextLabel
                TextLabel label = player.GetData(EntityData.PLAYER_AME);
                
                if (message.Length > 0)
                {
                    // We update label's text
                    label.Text = "*" + message + "*";
                }
                else
                {
                    // Deleting TextLabel
                    label.Detach();
                    label.Delete();
                    player.ResetData(EntityData.PLAYER_AME);
                }
            }
            else
            {
                TextLabel ameLabel = NAPI.TextLabel.CreateTextLabel("*" + message + "*", new Vector3(0.0f, 0.0f, 0.0f), 50.0f, 0.5f, 4, new Color(201, 90, 0, 255));
                ameLabel.AttachTo(player, "SKEL_Head", new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, 0.0f, 0.0f));
                player.SetData(EntityData.PLAYER_AME, ameLabel);
            }
        }

        [Command(Commands.COM_MEGAPHONE, Commands.HLP_MEGAPHONE_COMMAND, GreedyArg = true)]
        public void MegafonoCommand(Client player, string message)
        {
            if (player.IsInVehicle)
            {
                int vehicleFaction = player.Vehicle.GetData(EntityData.VEHICLE_FACTION);

                if (vehicleFaction == Constants.FACTION_POLICE || vehicleFaction == Constants.FACTION_EMERGENCY)
                {
                    SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_MEGAPHONE, 45.0f);
                }
                else
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_not_megaphone);
                }
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_vehicle);
            }
        }

        [Command(Commands.COM_PM, Commands.HLP_MP_COMMAND, GreedyArg = true)]
        public void MpCommand(Client player, string arguments)
        {
            Client target = null;
            string[] args = arguments.Trim().Split(' ');

            if (int.TryParse(args[0], out int targetId) == true)
            {
                // We get the player from the id
                target = Globals.GetPlayerById(targetId);
                args = args.Where(w => w != args[0]).ToArray();
                if (args.Length < 1)
                {
                    player.SendChatMessage(Constants.COLOR_HELP + Commands.HLP_MP_COMMAND);
                    return;
                }
            }
            else if (args.Length > 2)
            {
                target = NAPI.Player.GetPlayerFromName(args[0] + " " + args[1]);
                args = args.Where(w => w != args[1]).ToArray();
                args = args.Where(w => w != args[0]).ToArray();
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_HELP + Commands.HLP_MP_COMMAND);
                return;
            }
            
            if (target != null && target.HasData(EntityData.PLAYER_PLAYING) == true)
            {
                if (player.GetData(EntityData.PLAYER_ADMIN_RANK) == Constants.STAFF_NONE && target.GetData(EntityData.PLAYER_ADMIN_RANK) == Constants.STAFF_NONE)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.mps_only_admin);
                }
                else
                {
                    string message = string.Join(" ", args);
                    string secondMessage = string.Empty;

                    if (message.Length > Constants.CHAT_LENGTH)
                    {
                        // We split the message in two lines
                        secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                        message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                    }

                    // Sending messages to both players
                    player.SendChatMessage(secondMessage.Length > 0 ? Constants.COLOR_ADMIN_MP + "((" + GenRes.pm_to + "[ID: " + target.Value + "] " + target.Name + ": " + message + "..." : Constants.COLOR_ADMIN_MP + "((" + GenRes.pm_to + "[ID: " + target.Value + "] " + target.Name + ": " + message + "))");
                    target.SendChatMessage(secondMessage.Length > 0 ? Constants.COLOR_ADMIN_MP + "((" + GenRes.pm_from + "[ID: " + player.Value + "] " + player.Name + ": " + message + "..." : Constants.COLOR_ADMIN_MP + "((" + GenRes.pm_from + "[ID: " + player.Value + "] " + player.Name + ": " + message + "))");

                    if (secondMessage.Length > 0)
                    {
                        player.SendChatMessage(Constants.COLOR_ADMIN_MP + secondMessage + "))");
                       target.SendChatMessage(Constants.COLOR_ADMIN_MP + secondMessage + "))");
                    }
                }
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
            }
        }
    }
}
