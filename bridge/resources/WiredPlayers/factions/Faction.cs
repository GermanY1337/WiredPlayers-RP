using GTANetworkAPI;
using WiredPlayers.model;
using WiredPlayers.globals;
using WiredPlayers.chat;
using WiredPlayers.database;
using WiredPlayers.messages.error;
using WiredPlayers.messages.information;
using WiredPlayers.messages.general;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System;

namespace WiredPlayers.factions
{
    public class Faction : Script
    {
        public static List<ChannelModel> channelList;
        public static List<FactionWarningModel> factionWarningList;

        public static string GetPlayerFactionRank(Client player)
        {
            string rankString = string.Empty;
            int faction = player.GetData(EntityData.PLAYER_FACTION);
            int rank = player.GetData(EntityData.PLAYER_RANK);
            foreach (FactionModel factionModel in Constants.FACTION_RANK_LIST)
            {
                if (factionModel.faction == faction && factionModel.rank == rank)
                {
                    rankString = player.GetData(EntityData.PLAYER_SEX) == Constants.SEX_MALE ? factionModel.descriptionMale : factionModel.descriptionFemale;
                    break;
                }
            }
            return rankString;
        }

        public static FactionWarningModel GetFactionWarnByTarget(int playerId, int faction)
        {
            FactionWarningModel warn = null;
            foreach (FactionWarningModel factionWarn in factionWarningList)
            {
                if (factionWarn.playerId == playerId && factionWarn.faction == faction)
                {
                    warn = factionWarn;
                    break;
                }
            }
            return warn;
        }

        private ChannelModel GetPlayerOwnedChannel(int playerId)
        {
            ChannelModel channel = null;
            foreach (ChannelModel channelModel in channelList)
            {
                if (channelModel.owner == playerId)
                {
                    channel = channelModel;
                    break;
                }
            }
            return channel;
        }

        private string GetMd5Hash(MD5 md5Hash, string input)
        {
            StringBuilder sBuilder = new StringBuilder();
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        private bool CheckInternalAffairs(int faction, Client target)
        {
            bool isInternalAffairs = false;

            if (faction == Constants.FACTION_TOWNHALL && (target.GetData(EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE && target.GetData(EntityData.PLAYER_RANK) == 7))
            {
                isInternalAffairs = true;
            }

            return isInternalAffairs;
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            factionWarningList = new List<FactionWarningModel>();
        }

        [ServerEvent(Event.PlayerEnterCheckpoint)]
        public void OnPlayerEnterCheckpoint(Checkpoint checkpoint, Client player)
        {
            if (player.HasData(EntityData.PLAYER_FACTION_WARNING) == true)
            {
                Checkpoint locationCheckpoint = player.GetData(EntityData.PLAYER_FACTION_WARNING);
                locationCheckpoint.Delete();

                // Delete map blip
                player.TriggerEvent("deleteFactionWarning");
                
                player.ResetData(EntityData.PLAYER_FACTION_WARNING);

                // Remove the report
                factionWarningList.RemoveAll(x => x.takenBy == player.Value);
            }
        }

        [Command(Commands.COM_F, Commands.HLP_F_COMMAND, GreedyArg = true)]
        public void FCommand(Client player, string message)
        {
            int faction = player.GetData(EntityData.PLAYER_FACTION);
            if (faction > 0 && faction < Constants.LAST_STATE_FACTION)
            {
                string rank = GetPlayerFactionRank(player);
                
                string secondMessage = string.Empty;

                if (message.Length > Constants.CHAT_LENGTH)
                {
                    // We need two lines to write the message
                    secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                    message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                }

                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    if (target.HasData(EntityData.PLAYER_PLAYING) && target.GetData(EntityData.PLAYER_FACTION) == faction)
                    {
                       target.SendChatMessage(secondMessage.Length > 0 ? Constants.COLOR_CHAT_FACTION + "(([ID: " + player.Value + "] " + rank + " " + player.Name + ": " + message + "..." : Constants.COLOR_CHAT_FACTION + "(([ID: " + player.Value + "] " + rank + " " + player.Name + ": " + message + "))");
                        if (secondMessage.Length > 0)
                        {
                           target.SendChatMessage(Constants.COLOR_CHAT_FACTION + secondMessage + "))");
                        }
                    }
                }
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_state_faction);
            }
        }

        [Command(Commands.COM_R, Commands.HLP_R_COMMAND, GreedyArg = true)]
        public void RCommand(Client player, string message)
        {
            if (player.GetData(EntityData.PLAYER_KILLED) != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else
            {
                int faction = player.GetData(EntityData.PLAYER_FACTION);
                if (faction > 0 && faction < Constants.LAST_STATE_FACTION)
                {
                    // Get player's rank in faction
                    string rank = GetPlayerFactionRank(player);
                    
                    string secondMessage = string.Empty;

                    if (message.Length > Constants.CHAT_LENGTH)
                    {
                        // We need two lines to write the message
                        secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                        message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                    }

                    foreach (Client target in NAPI.Pools.GetAllPlayers())
                    {
                        if (target.HasData(EntityData.PLAYER_PLAYING) && (target.GetData(EntityData.PLAYER_FACTION) == faction || CheckInternalAffairs(faction, target) == true))
                        {
                           target.SendChatMessage(secondMessage.Length > 0 ? Constants.COLOR_RADIO + GenRes.radio + rank + " " + player.Name + GenRes.chat_say + message + "..." : Constants.COLOR_RADIO + GenRes.radio + rank + " " + player.Name + GenRes.chat_say + message);
                            if (secondMessage.Length > 0)
                            {
                               target.SendChatMessage(Constants.COLOR_RADIO + secondMessage);
                            }
                        }
                    }

                    // Send the chat message to near players
                    Chat.SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_RADIO, player.Dimension > 0 ? 7.5f : 10.0f);

                }
                else
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_state_faction);
                }
            }
        }

        [Command(Commands.COM_DP, Commands.HLP_DP_COMMAND, GreedyArg = true)]
        public void DpCommand(Client player, string message)
        {
            if (player.GetData(EntityData.PLAYER_KILLED) != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else
            {
                if (player.GetData(EntityData.PLAYER_FACTION) == Constants.FACTION_EMERGENCY)
                {
                    string rank = GetPlayerFactionRank(player);
                    
                    string secondMessage = string.Empty;

                    if (message.Length > Constants.CHAT_LENGTH)
                    {
                        // We need two lines to write the message
                        secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                        message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                    }

                    foreach (Client target in NAPI.Pools.GetAllPlayers())
                    {
                        if (target.HasData(EntityData.PLAYER_PLAYING) && target.GetData(EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE)
                        {
                           target.SendChatMessage(secondMessage.Length > 0 ? Constants.COLOR_RADIO + GenRes.radio + rank + " " + player.Name + GenRes.chat_say + message + "..." : Constants.COLOR_RADIO + GenRes.radio + rank + " " + player.Name + GenRes.chat_say + message);
                            if (secondMessage.Length > 0)
                            {
                               target.SendChatMessage(Constants.COLOR_RADIO + secondMessage);
                            }
                        }
                        else if (target.HasData(EntityData.PLAYER_PLAYING) && target.GetData(EntityData.PLAYER_FACTION) == Constants.FACTION_EMERGENCY)
                        {
                           target.SendChatMessage(secondMessage.Length > 0 ? Constants.COLOR_RADIO_POLICE + GenRes.radio + rank + " " + player.Name + GenRes.chat_say + message + "..." : Constants.COLOR_RADIO_POLICE + GenRes.radio + rank + " " + player.Name + GenRes.chat_say + message);
                            if (secondMessage.Length > 0)
                            {
                               target.SendChatMessage(Constants.COLOR_RADIO_POLICE + secondMessage);
                            }
                        }
                    }

                    // Send the chat message to near players
                    Chat.SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_RADIO, player.Dimension > 0 ? 7.5f : 10.0f);
                }
                else
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_emergency_faction);
                }
            }
        }

        [Command(Commands.COM_DE, Commands.HLP_DE_COMMAND, GreedyArg = true)]
        public void DeCommand(Client player, string message)
        {
            if (player.GetData(EntityData.PLAYER_KILLED) != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else
            {
                if (player.GetData(EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE)
                {
                    string rank = GetPlayerFactionRank(player);
                    
                    string secondMessage = string.Empty;

                    if (message.Length > Constants.CHAT_LENGTH)
                    {
                        // We need two lines to write the message
                        secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                        message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                    }

                    foreach (Client target in NAPI.Pools.GetAllPlayers())
                    {
                        if (target.HasData(EntityData.PLAYER_PLAYING) && target.GetData(EntityData.PLAYER_FACTION) == Constants.FACTION_POLICE)
                        {
                           target.SendChatMessage(secondMessage.Length > 0 ? Constants.COLOR_RADIO_POLICE + GenRes.radio + rank + " " + player.Name + GenRes.chat_say + message + "..." : Constants.COLOR_RADIO_POLICE + GenRes.radio + rank + " " + player.Name + GenRes.chat_say + message);
                            if (secondMessage.Length > 0)
                            {
                               target.SendChatMessage(Constants.COLOR_RADIO_POLICE + secondMessage);
                            }
                        }
                        else if (target.HasData(EntityData.PLAYER_PLAYING) && target.GetData(EntityData.PLAYER_FACTION) == Constants.FACTION_EMERGENCY)
                        {
                           target.SendChatMessage(secondMessage.Length > 0 ? Constants.COLOR_RADIO + GenRes.radio + rank + " " + player.Name + GenRes.chat_say + message + "..." : Constants.COLOR_RADIO + GenRes.radio + rank + " " + player.Name + GenRes.chat_say + message);
                            if (secondMessage.Length > 0)
                            {
                               target.SendChatMessage(Constants.COLOR_RADIO + secondMessage);
                            }
                        }
                    }

                    // Send the chat message to near players
                    Chat.SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_RADIO, player.Dimension > 0 ? 7.5f : 10.0f);
                }
                else
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_emergency_faction);
                }
            }
        }

        [Command(Commands.COM_FR, Commands.HLP_FR_COMMAND, GreedyArg = true)]
        public void FrCommand(Client player, string message)
        {
            if (player.GetData(EntityData.PLAYER_KILLED) != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else
            {
                int radio = player.GetData(EntityData.PLAYER_RADIO);
                if (radio > 0)
                {
                    string name = player.GetData(EntityData.PLAYER_NAME);
                    
                    string secondMessage = string.Empty;

                    if (message.Length > Constants.CHAT_LENGTH)
                    {
                        // We need two lines to write the message
                        secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                        message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
                    }

                    foreach (Client target in NAPI.Pools.GetAllPlayers())
                    {
                        if (target.HasData(EntityData.PLAYER_PLAYING) && target.GetData(EntityData.PLAYER_RADIO) == radio)
                        {
                           target.SendChatMessage(secondMessage.Length > 0 ? Constants.COLOR_RADIO + GenRes.radio + name + GenRes.chat_say + message + "..." : Constants.COLOR_RADIO + GenRes.radio + name + GenRes.chat_say + message);
                            if (secondMessage.Length > 0)
                            {
                               target.SendChatMessage(Constants.COLOR_RADIO + secondMessage);
                            }
                        }
                    }

                    // Send the chat message to near players
                    Chat.SendMessageToNearbyPlayers(player, message, Constants.MESSAGE_RADIO, player.Dimension > 0 ? 7.5f : 10.0f);
                }
                else
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.radio_frequency_none);
                }
            }
        }

        [Command(Commands.COM_FREQUENCY, Commands.HLP_FREQUENCY_COMMAND, GreedyArg = true)]
        public void FrequencyCommand(Client player, string args)
        {
            if (player.HasData(EntityData.PLAYER_RIGHT_HAND) == true)
            {
                int itemId = player.GetData(EntityData.PLAYER_RIGHT_HAND);
                ItemModel item = Globals.GetItemModelFromId(itemId);
                if (item != null && item.hash == Constants.ITEM_HASH_WALKIE)
                {
                    int playerId = player.GetData(EntityData.PLAYER_SQL_ID);
                    ChannelModel ownedChannel = GetPlayerOwnedChannel(playerId);
                    string[] arguments = args.Trim().Split(' ');
                    switch (arguments[0].ToLower())
                    {
                        case Commands.ARG_CREATE:
                            if (arguments.Length == 2)
                            {
                                if (ownedChannel == null)
                                {
                                    // We create the new frequency
                                    MD5 md5Hash = MD5.Create();
                                    ChannelModel channel = new ChannelModel();
                                    {
                                        channel.owner = playerId;
                                        channel.password = GetMd5Hash(md5Hash, arguments[1]);
                                    }
                                    
                                    Task.Factory.StartNew(() =>
                                    {
                                        // Create the new channel
                                        channel.id = Database.AddChannel(channel);
                                        channelList.Add(channel);

                                        // Sending the message with created channel
                                        string message = string.Format(InfoRes.channel_created, channel.id);
                                        player.SendChatMessage(Constants.COLOR_INFO + message);
                                    });
                                }
                                else
                                {
                                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.already_owned_channel);
                                }
                            }
                            else
                            {
                                player.SendChatMessage(Constants.COLOR_HELP + Commands.HLP_FREQUENCY_CREATE_COMMAND);
                            }
                            break;
                        case Commands.ARG_MODIFY:
                            if (arguments.Length == 2)
                            {
                                if (ownedChannel != null)
                                {
                                    MD5 md5Hash = MD5.Create();
                                    ownedChannel.password = GetMd5Hash(md5Hash, arguments[1]);

                                    // We kick all the players from the channel
                                    foreach (Client target in NAPI.Pools.GetAllPlayers())
                                    {
                                        int targetId = player.GetData(EntityData.PLAYER_SQL_ID);
                                        if (target.GetData(EntityData.PLAYER_RADIO) == ownedChannel.id && targetId != ownedChannel.owner)
                                        {
                                            target.SetData(EntityData.PLAYER_RADIO, 0);
                                           target.SendChatMessage(Constants.COLOR_INFO + InfoRes.channel_disconnected);
                                        }
                                    }


                                    Task.Factory.StartNew(() =>
                                    {
                                        // Update the channel and disconnect the leader
                                        Database.UpdateChannel(ownedChannel);
                                        Database.DisconnectFromChannel(ownedChannel.id);

                                        // Message sent with the confirmation
                                        player.SendChatMessage(Constants.COLOR_INFO + InfoRes.channel_updated);
                                    });
                                }
                                else
                                {
                                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_owned_channel);
                                }
                            }
                            else
                            {
                                player.SendChatMessage(Constants.COLOR_HELP + Commands.HLP_FREQUENCY_MODIFY_COMMAND);
                            }
                            break;
                        case Commands.ARG_REMOVE:
                            if (ownedChannel != null)
                            {
                                // We kick all the players from the channel
                                foreach (Client target in NAPI.Pools.GetAllPlayers())
                                {
                                    int targetId = player.GetData(EntityData.PLAYER_SQL_ID);
                                    if (target.GetData(EntityData.PLAYER_RADIO) == ownedChannel.id)
                                    {
                                        target.SetData(EntityData.PLAYER_RADIO, 0);
                                        if (ownedChannel.owner != targetId)
                                        {
                                           target.SendChatMessage(Constants.COLOR_INFO + InfoRes.channel_disconnected);
                                        }
                                    }
                                }

                                Task.Factory.StartNew(() =>
                                {
                                    // Disconnect the leader from the channel
                                    Database.DisconnectFromChannel(ownedChannel.id);

                                    // We destroy the channel
                                    Database.RemoveChannel(ownedChannel.id);
                                    channelList.Remove(ownedChannel);

                                    // Message sent with the confirmation
                                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.channel_deleted);
                                });
                            }
                            else
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_owned_channel);
                            }
                            break;
                        case Commands.ARG_CONNECT:
                            if (arguments.Length == 3)
                            {
                                if (int.TryParse(arguments[1], out int frequency) == true)
                                {
                                    // We encrypt the password
                                    MD5 md5Hash = MD5.Create();
                                    string password = GetMd5Hash(md5Hash, arguments[2]);

                                    foreach (ChannelModel channel in channelList)
                                    {
                                        if (channel.id == frequency && channel.password == password)
                                        {
                                            string message = string.Format(InfoRes.channel_connected, channel.id);
                                            player.SetData(EntityData.PLAYER_RADIO, channel.id);
                                            player.SendChatMessage(Constants.COLOR_INFO + message);
                                            return;
                                        }
                                    }

                                    // Couldn't find any channel with that id
                                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.channel_not_found);
                                }
                                else
                                {
                                    player.SendChatMessage(Constants.COLOR_HELP + Commands.HLP_FREQUENCY_CONNECT_COMMAND);
                                }
                            }
                            else
                            {
                                player.SendChatMessage(Constants.COLOR_HELP + Commands.HLP_FREQUENCY_CONNECT_COMMAND);
                            }
                            break;
                        case Commands.ARG_DISCONNECT:
                            player.SetData(EntityData.PLAYER_RADIO, 0);
                            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.channel_disconnected);
                            break;
                        default:
                            player.SendChatMessage(Constants.COLOR_HELP + Commands.HLP_FREQUENCY_COMMAND);
                            break;
                    }
                }
                else
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_walkie_in_hand);
                }
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.right_hand_empty);
            }
        }

        [Command(Commands.COM_RECRUIT, Commands.HLP_RECRUIT_COMMAND)]
        public void RecruitCommand(Client player, string targetString)
        {
            int faction = player.GetData(EntityData.PLAYER_FACTION);

            if (faction > Constants.FACTION_NONE)
            {
                Client target = int.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (target == null)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                }
                else if (target.GetData(EntityData.PLAYER_FACTION) > 0)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_already_faction);
                }
                else
                {
                    int rank = player.GetData(EntityData.PLAYER_RANK);

                    switch (faction)
                    {
                        case Constants.FACTION_POLICE:
                            if (target.GetData(EntityData.PLAYER_JOB) > 0)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_already_job);
                            }
                            else if (rank < 6)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_recruit);
                            }
                            else
                            {
                                string targetMessage = string.Format(InfoRes.faction_recruited, GenRes.faction_lspd);

                                // We get the player into the faction
                                target.SetData(EntityData.PLAYER_FACTION, Constants.FACTION_POLICE);
                                target.SetData(EntityData.PLAYER_RANK, 1);

                                // Sending the message to the player
                               target.SendChatMessage(Constants.COLOR_INFO + targetMessage);
                            }
                            break;
                        case Constants.FACTION_EMERGENCY:
                            if (target.GetData(EntityData.PLAYER_JOB) > 0)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_already_job);
                            }
                            else if (rank < 10)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_recruit);
                            }
                            else
                            {
                                string targetMessage = string.Format(InfoRes.faction_recruited, GenRes.faction_ems);

                                // We get the player into the faction
                                target.SetData(EntityData.PLAYER_FACTION, Constants.FACTION_EMERGENCY);
                                target.SetData(EntityData.PLAYER_RANK, 1);

                                // Sending the message to the player
                               target.SendChatMessage(Constants.COLOR_INFO + targetMessage);
                            }
                            break;
                        case Constants.FACTION_NEWS:
                            if (target.GetData(EntityData.PLAYER_JOB) > 0)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_already_job);
                            }
                            else if (rank < 5)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_recruit);
                            }
                            else
                            {
                                string targetMessage = string.Format(InfoRes.faction_recruited, GenRes.faction_news);

                                // We get the player into the faction
                                target.SetData(EntityData.PLAYER_FACTION, Constants.FACTION_NEWS);
                                target.SetData(EntityData.PLAYER_RANK, 1);

                                // Sending the message to the player
                               target.SendChatMessage(Constants.COLOR_INFO + targetMessage);
                            }
                            break;
                        case Constants.FACTION_TOWNHALL:
                            if (target.GetData(EntityData.PLAYER_JOB) > 0)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_already_job);
                            }
                            else if (rank < 3)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_recruit);
                            }
                            else
                            {
                                string targetMessage = string.Format(InfoRes.faction_recruited, GenRes.faction_townhall);

                                // We get the player into the faction
                                target.SetData(EntityData.PLAYER_FACTION, Constants.FACTION_TOWNHALL);
                                target.SetData(EntityData.PLAYER_RANK, 1);

                                // Sending the message to the player
                               target.SendChatMessage(Constants.COLOR_INFO + targetMessage);
                            }
                            break;
                        case Constants.FACTION_TAXI_DRIVER:
                            if (target.GetData(EntityData.PLAYER_JOB) > 0)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_already_job);
                            }
                            else if (rank < 5)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_recruit);
                            }
                            else
                            {
                                string targetMessage = string.Format(InfoRes.faction_recruited, GenRes.faction_transport);

                                // We get the player into the faction
                                target.SetData(EntityData.PLAYER_FACTION, Constants.FACTION_TAXI_DRIVER);
                                target.SetData(EntityData.PLAYER_RANK, 1);

                                // Sending the message to the player
                               target.SendChatMessage(Constants.COLOR_INFO + targetMessage);
                            }
                            break;
                        default:
                            if (rank < 6)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_recruit);
                            }
                            else
                            {
                                string targetMessage = string.Format(InfoRes.faction_recruited, faction);

                                // We get the player into the faction
                                target.SetData(EntityData.PLAYER_FACTION, faction);
                                target.SetData(EntityData.PLAYER_RANK, 1);

                                // Sending the message to the player
                               target.SendChatMessage(Constants.COLOR_INFO + targetMessage);
                            }
                            break;
                    }

                    // We send the message to the recruiter
                    string playerMessage = string.Format(InfoRes.player_recruited, target.Name);
                    player.SendChatMessage(Constants.COLOR_INFO + playerMessage);
                }
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_no_faction);
            }
        }

        [Command(Commands.COM_DISMISS, Commands.HLP_DISMISS_COMMAND)]
        public void DismissCommand(Client player, string targetString)
        {
            int faction = player.GetData(EntityData.PLAYER_FACTION);

            if (faction != Constants.FACTION_NONE)
            {
                Client target = int.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (target == null)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                }
                else if (target.GetData(EntityData.PLAYER_FACTION) != faction)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_in_same_faction);
                }
                else
                {
                    int rank = player.GetData(EntityData.PLAYER_RANK);

                    switch (faction)
                    {
                        case Constants.FACTION_POLICE:
                            if (rank < 6)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_dismiss);
                            }
                            else
                            {
                                // We kick the player from the faction
                                target.SetData(EntityData.PLAYER_FACTION, 0);
                                target.SetData(EntityData.PLAYER_RANK, 0);
                            }
                            break;
                        case Constants.FACTION_EMERGENCY:
                            if (rank < 10)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_dismiss);
                            }
                            else
                            {
                                // We kick the player from the faction
                                target.SetData(EntityData.PLAYER_FACTION, 0);
                                target.SetData(EntityData.PLAYER_RANK, 0);
                            }
                            break;
                        case Constants.FACTION_NEWS:
                            if (rank < 5)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_dismiss);
                            }
                            else
                            {
                                // We kick the player from the faction
                                target.SetData(EntityData.PLAYER_FACTION, 0);
                                target.SetData(EntityData.PLAYER_RANK, 0);
                            }
                            break;
                        case Constants.FACTION_TOWNHALL:
                            if (rank < 3)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_dismiss);
                            }
                            else
                            {
                                // We kick the player from the faction
                                target.SetData(EntityData.PLAYER_FACTION, 0);
                                target.SetData(EntityData.PLAYER_RANK, 0);
                            }
                            break;
                        case Constants.FACTION_TAXI_DRIVER:
                            if (rank < 5)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_dismiss);
                            }
                            else
                            {
                                // We kick the player from the faction
                                target.SetData(EntityData.PLAYER_FACTION, 0);
                                target.SetData(EntityData.PLAYER_RANK, 0);
                            }
                            break;
                        default:
                            if (rank < 6)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_dismiss);
                            }
                            else
                            {
                                // We kick the player from the faction
                                target.SetData(EntityData.PLAYER_FACTION, 0);
                                target.SetData(EntityData.PLAYER_RANK, 0);
                            }
                            break;
                    }

                    string playerMessage = string.Format(InfoRes.player_dismissed, target.Name);
                    string targetMessage = string.Format(InfoRes.faction_dismissed, player.Name);

                    // Send the messages to both players
                    player.SendChatMessage(Constants.COLOR_INFO + playerMessage);
                   target.SendChatMessage(Constants.COLOR_INFO + targetMessage);
                }
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_no_faction);
            }
        }

        [Command(Commands.COM_RANK, Commands.HLP_RANK_COMMAND)]
        public void RankCommand(Client player, string arguments)
        {
            int faction = player.GetData(EntityData.PLAYER_FACTION);

            if (faction != Constants.FACTION_NONE)
            {
                string[] args = arguments.Split(' ');

                // Get the target player
                Client target = int.TryParse(args[0], out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(args[0] + " " + args[1]);

                if (target == null)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                }
                else if (target.GetData(EntityData.PLAYER_FACTION) != faction)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_in_same_faction);
                }
                else
                {
                    int rank = player.GetData(EntityData.PLAYER_RANK);
                    int givenRank = args.Length > 2 ? int.Parse(args[2]) : int.Parse(args[1]);

                    switch (faction)
                    {
                        case Constants.FACTION_POLICE:
                            if (rank < 6)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_rank);
                            }
                            else
                            {
                                // Change player's rank
                                target.SetData(EntityData.PLAYER_RANK, givenRank);
                            }
                            break;
                        case Constants.FACTION_EMERGENCY:
                            if (rank < 10)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_rank);
                            }
                            else
                            {
                                // Change player's rank
                                target.SetData(EntityData.PLAYER_RANK, givenRank);
                            }
                            break;
                        case Constants.FACTION_NEWS:
                            if (rank < 5)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_rank);
                            }
                            else
                            {
                                // Change player's rank
                                target.SetData(EntityData.PLAYER_RANK, givenRank);
                            }
                            break;
                        case Constants.FACTION_TOWNHALL:
                            if (rank < 3)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_rank);
                            }
                            else
                            {
                                // Change player's rank
                                target.SetData(EntityData.PLAYER_RANK, givenRank);
                            }
                            break;
                        case Constants.FACTION_TAXI_DRIVER:
                            if (rank < 5)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_rank);
                            }
                            else
                            {
                                // Change player's rank
                                target.SetData(EntityData.PLAYER_RANK, givenRank);
                            }
                            break;
                        default:
                            if (rank < 6)
                            {
                                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_rank);
                            }
                            else
                            {
                                // Change player's rank
                                target.SetData(EntityData.PLAYER_RANK, givenRank);
                            }
                            break;
                    }

                    string playerMessage = string.Format(InfoRes.player_rank_changed, target.Name, givenRank);
                    string targetMessage = string.Format(InfoRes.faction_rank_changed, player.Name, givenRank);

                    // Send the message to both players
                    player.SendChatMessage(Constants.COLOR_INFO + playerMessage);
                   target.SendChatMessage(Constants.COLOR_INFO + targetMessage);
                }
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_no_faction);
            }
        }

        [Command(Commands.COM_REPORTS)]
        public void ReportsCommand(Client player)
        {
            int faction = player.GetData(EntityData.PLAYER_FACTION);

            if (faction == Constants.FACTION_POLICE || faction == Constants.FACTION_EMERGENCY)
            {
                int currentElement = 0;
                int totalWarnings = 0;

                // Reports' header
                player.SendChatMessage(Constants.COLOR_INFO + GenRes.reports_header);

                foreach (FactionWarningModel factionWarning in factionWarningList)
                {
                    if (factionWarning.faction == faction)
                    {
                        string message = string.Empty;
                        if (factionWarning.place.Length > 0)
                        {
                            message = currentElement + ". " + GenRes.time + factionWarning.hour + ", " + GenRes.place + factionWarning.place;
                        }
                        else
                        {
                            message = currentElement + ". " + GenRes.time + factionWarning.hour;
                        }

                        // Check if attended
                        if (factionWarning.takenBy > -1)
                        {
                            Client target = Globals.GetPlayerById(factionWarning.takenBy);
                            message += ", " + GenRes.attended_by + target.Name;
                        }
                        else
                        {
                            message += ", " + GenRes.unattended;
                        }

                        // We send the message to the player
                        player.SendChatMessage(Constants.COLOR_HELP + message);
                        
                        totalWarnings++;
                    }
                    
                    currentElement++;
                }
                
                if (totalWarnings == 0)
                {
                    // There are no reports in the list
                    player.SendChatMessage(Constants.COLOR_HELP + GenRes.not_faction_warning);
                }
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_police_emergency_faction);
            }
        }

        [Command(Commands.COM_ATTEND, Commands.HLP_ATTEND_COMMAND)]
        public void AttendCommand(Client player, int warning)
        {
            int faction = player.GetData(EntityData.PLAYER_FACTION);

            if (faction == Constants.FACTION_POLICE || faction == Constants.FACTION_EMERGENCY)
            {
                try
                {
                    FactionWarningModel factionWarning = factionWarningList.ElementAt(warning);

                    // Check the faction and whether the report is attended
                    if (factionWarning.faction != faction)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.faction_warning_not_found);
                    }
                    else if (factionWarning.takenBy > -1)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.faction_warning_taken);
                    }
                    else if (player.HasData(EntityData.PLAYER_FACTION_WARNING) == true)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_have_faction_warning);
                    }
                    else
                    {
                        Checkpoint factionWarningCheckpoint = NAPI.Checkpoint.CreateCheckpoint(4, factionWarning.position, new Vector3(0.0f, 0.0f, 0.0f), 2.5f, new Color(198, 40, 40, 200));
                        player.SetData(EntityData.PLAYER_FACTION_WARNING, factionWarningCheckpoint);
                        factionWarning.takenBy = player.Value;

                        player.SendChatMessage(Constants.COLOR_INFO + InfoRes.faction_warning_taken);

                        player.TriggerEvent("showFactionWarning", factionWarning.position);
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.faction_warning_not_found);
                }
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_police_emergency_faction);
            }
        }

        [Command(Commands.COM_CLEAR_REPORTS, Commands.HLP_CLEAR_REPORTS_COMMAND)]
        public void ClearReportsCommand(Client player, int warning)
        {
            int faction = player.GetData(EntityData.PLAYER_FACTION);

            if (faction == Constants.FACTION_POLICE || faction == Constants.FACTION_EMERGENCY)
            {
                try
                {
                    FactionWarningModel factionWarning = factionWarningList.ElementAt(warning);

                    // Check the faction and whether the report is attended
                    if (factionWarning.faction != faction)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.faction_warning_not_found);
                    }
                    else
                    {
                        // We remove the report
                        factionWarningList.Remove(factionWarning);

                        // Send the message to the user
                        string message = string.Format(InfoRes.faction_warning_deleted, warning);
                        player.SendChatMessage(Constants.COLOR_INFO + message);
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.faction_warning_not_found);
                }
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_police_emergency_faction);
            }
        }

        [Command(Commands.COM_MEMBERS)]
        public void MembersCommand(Client player)
        {
            int faction = player.GetData(EntityData.PLAYER_FACTION);
            if (faction > 0)
            {
                player.SendChatMessage(Constants.COLOR_INFO + GenRes.members_online);
                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    if (target.HasData(EntityData.PLAYER_PLAYING) && target.GetData(EntityData.PLAYER_FACTION) == faction)
                    {
                        string rank = GetPlayerFactionRank(target);

                        if (rank == string.Empty)
                        {
                            player.SendChatMessage(Constants.COLOR_HELP + "[Id: " + player.Value + "] " + target.Name);
                        }
                        else
                        {
                            player.SendChatMessage(Constants.COLOR_HELP + "[Id: " + player.Value + "] " + rank + " " + target.Name);
                        }
                    }
                }
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_no_faction);
            }
        }
    }
}
