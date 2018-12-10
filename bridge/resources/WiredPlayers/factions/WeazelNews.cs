﻿using GTANetworkAPI;
using WiredPlayers.database;
using WiredPlayers.globals;
using WiredPlayers.model;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.information;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WiredPlayers.factions
{
    public class WeazelNews : Script
    {
        public static List<AnnoucementModel> annoucementList;

        public static void SendNewsMessage(Client player, string message)
        {
            string secondMessage = string.Empty;

            if (message.Length > Constants.CHAT_LENGTH)
            {
                // We need two lines to print the message
                secondMessage = message.Substring(Constants.CHAT_LENGTH, message.Length - Constants.CHAT_LENGTH);
                message = message.Remove(Constants.CHAT_LENGTH, secondMessage.Length);
            }

            if (player.HasData(EntityData.PLAYER_ON_AIR) && player.GetData(EntityData.PLAYER_FACTION) == Constants.FACTION_NEWS)
            {
                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    if (target.HasData(EntityData.PLAYER_PLAYING) == true)
                    {
                       target.SendChatMessage(secondMessage.Length > 0 ? Constants.COLOR_NEWS + GenRes.interviewer + player.Name + ": " + message + "..." : Constants.COLOR_NEWS + GenRes.interviewer + player.Name + ": " + message);
                        if (secondMessage.Length > 0)
                        {
                           target.SendChatMessage(Constants.COLOR_NEWS + secondMessage);
                        }
                    }
                }
            }
            else if (player.HasData(EntityData.PLAYER_ON_AIR))
            {
                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    if (target.HasData(EntityData.PLAYER_PLAYING) == true)
                    {
                       target.SendChatMessage(secondMessage.Length > 0 ? Constants.COLOR_NEWS + GenRes.guest + player.Name + ": " + message + "..." : Constants.COLOR_NEWS + GenRes.guest + player.Name + ": " + message);
                        if (secondMessage.Length > 0)
                        {
                           target.SendChatMessage(Constants.COLOR_NEWS + secondMessage);
                        }
                    }
                }
            }
            else
            {
                foreach (Client target in NAPI.Pools.GetAllPlayers())
                {
                    if (target.HasData(EntityData.PLAYER_PLAYING) == true)
                    {
                       target.SendChatMessage(secondMessage.Length > 0 ? Constants.COLOR_NEWS + GenRes.announcement + message + "..." : Constants.COLOR_NEWS + GenRes.announcement + message);
                        if (secondMessage.Length > 0)
                        {
                           target.SendChatMessage(Constants.COLOR_NEWS + secondMessage);
                        }
                    }
                }
            }
        }

        private int GetRemainingFounds()
        {
            int remaining = 0;

            foreach (AnnoucementModel announcement in annoucementList)
            {
                remaining += announcement.given ? -announcement.amount : announcement.amount;
            }
            return remaining;
        }

        [Command(Commands.COM_INTERVIEW, Commands.HLP_OFFER_ON_AIR_COMMAND)]
        public void EntrevistarCommand(Client player, string targetString)
        {
            if (player.GetData(EntityData.PLAYER_KILLED) != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else if (player.GetData(EntityData.PLAYER_FACTION) != Constants.FACTION_NEWS)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_news_faction);
            }
            else
            {
                Vehicle vehicle = Globals.GetClosestVehicle(player);
                if (vehicle.GetData(EntityData.VEHICLE_FACTION) != Constants.FACTION_NEWS && player.VehicleSeat != (int)VehicleSeat.LeftRear)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_in_news_van);
                }
                else
                {
                    Client target = int.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                    if (target.HasData(EntityData.PLAYER_ON_AIR) == true)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.on_air);
                    }
                    else
                    {
                        player.SetData(EntityData.PLAYER_ON_AIR, target);
                        player.SetData(EntityData.PLAYER_JOB_PARTNER, target);
                        player.SendChatMessage(Constants.COLOR_INFO + InfoRes.wz_offer_onair);
                        target.SendChatMessage(Constants.COLOR_INFO + InfoRes.wz_accept_onair);
                    }
                }
            }
        }

        [Command(Commands.COM_CUT_INTERVIEW, Commands.HLP_CUT_ON_AIR_COMMAND)]
        public void CutInterviewCommand(Client player, string targetString)
        {
            if (player.GetData(EntityData.PLAYER_KILLED) != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else if (player.GetData(EntityData.PLAYER_FACTION) != Constants.FACTION_NEWS)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_news_faction);
            }
            else
            {
                Client target = int.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);
                
                if (target.HasData(EntityData.PLAYER_ON_AIR) == false)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_on_air);
                }
                else if (target == player)
                {
                    foreach (Client interviewed in NAPI.Pools.GetAllPlayers())
                    {
                        if (interviewed.HasData(EntityData.PLAYER_ON_AIR) && interviewed != player)
                        {
                            interviewed.ResetData(EntityData.PLAYER_ON_AIR);
                            interviewed.ResetData(EntityData.PLAYER_JOB_PARTNER);
                            interviewed.SendChatMessage(Constants.COLOR_INFO + InfoRes.player_on_air_cutted);
                        }
                    }
                    
                    player.ResetData(EntityData.PLAYER_ON_AIR);
                    player.ResetData(EntityData.PLAYER_JOB_PARTNER);
                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.reporter_on_air_cutted);
                }
                else
                {
                    target.ResetData(EntityData.PLAYER_ON_AIR);
                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.reporter_on_air_cutted);
                    target.SendChatMessage(Constants.COLOR_INFO + InfoRes.player_on_air_cutted);
                }
            }
        }

        [Command(Commands.COM_PRIZE, Commands.HLP_PRIZE_COMMAND, GreedyArg = true)]
        public void PrizeCommand(Client player, string targetString, int prize, string contest)
        {
            if (player.GetData(EntityData.PLAYER_KILLED) != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else if (player.GetData(EntityData.PLAYER_FACTION) != Constants.FACTION_NEWS)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_news_faction);
            }
            else
            {
                Client target = int.TryParse(targetString, out int targetId) ? Globals.GetPlayerById(targetId) : NAPI.Player.GetPlayerFromName(targetString);

                if (target != null && target.HasData(EntityData.PLAYER_PLAYING) == true)
                {
                    int prizeAmount = GetRemainingFounds();
                    if (prizeAmount >= prize)
                    {
                        AnnoucementModel prizeModel = new AnnoucementModel();
                        int targetMoney = target.GetSharedData(EntityData.PLAYER_MONEY);

                        string playerMessage = string.Format(InfoRes.prize_given, prize, target.Name);
                        string targetMessage = string.Format(InfoRes.prize_received, player.Name, prize, contest);

                        targetMoney += prize;
                        target.SetSharedData(EntityData.PLAYER_MONEY, targetMoney);

                        prizeModel.amount = prize;
                        prizeModel.winner = target.GetData(EntityData.PLAYER_SQL_ID);
                        prizeModel.annoucement = contest;
                        prizeModel.journalist = player.GetData(EntityData.PLAYER_SQL_ID);
                        prizeModel.given = true;

                        player.SendChatMessage(Constants.COLOR_INFO + playerMessage);
                       target.SendChatMessage(Constants.COLOR_INFO + targetMessage);

                        Task.Factory.StartNew(() =>
                        {
                            prizeModel.id = Database.GivePrize(prizeModel);
                            annoucementList.Add(prizeModel);

                            // Log the money won
                            Database.LogPayment(player.Name, target.Name, GenRes.news_prize, prize);
                        });
                    }
                    else
                    {
                        player.SendChatMessage(Constants.COLOR_INFO + ErrRes.faction_not_enough_money);
                    }
                }
                else
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                }
            }
        }

        [Command(Commands.COM_ANNOUNCE, Commands.HLP_ANNOUCEMENT_COMMAND, GreedyArg = true)]
        public void AnnounceCommand(Client player, string message)
        {
            if (player.GetData(EntityData.PLAYER_KILLED) != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else
            {
                int money = player.GetSharedData(EntityData.PLAYER_MONEY);
                if (money < Constants.PRICE_ANNOUNCEMENT)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_enough_money);
                }
                else
                {
                    AnnoucementModel annoucement = new AnnoucementModel();
                    {
                        annoucement.winner = player.GetData(EntityData.PLAYER_SQL_ID);
                        annoucement.amount = Constants.PRICE_ANNOUNCEMENT;
                        annoucement.annoucement = message;
                        annoucement.given = false;
                    }

                    // Removes player money
                    player.SetSharedData(EntityData.PLAYER_MONEY, money - Constants.PRICE_ANNOUNCEMENT);

                    SendNewsMessage(player, message);
                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.announce_published);

                    Task.Factory.StartNew(() =>
                    {
                        // Log the announcement into the database
                        annoucement.id = Database.SendAnnoucement(annoucement);
                        Database.LogPayment(player.Name, GenRes.faction_news, GenRes.news_announce, Constants.PRICE_ANNOUNCEMENT);
                    });
                }
            }
        }

        [Command(Commands.COM_NEWS, Commands.HLP_NEWS_COMMAND, GreedyArg = true)]
        public void NewsCommand(Client player, string message)
        {
            if (player.GetData(EntityData.PLAYER_KILLED) != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else if (player.GetData(EntityData.PLAYER_FACTION) != Constants.FACTION_NEWS)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_news_faction);
            }
            else
            {
                Vehicle vehicle = Globals.GetClosestVehicle(player);
                if (vehicle.GetData(EntityData.VEHICLE_FACTION) != Constants.FACTION_NEWS && player.VehicleSeat != (int)VehicleSeat.LeftRear)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_in_news_van);
                }
                else
                {
                    SendNewsMessage(player, message);
                }
            }
        }
    }
}

