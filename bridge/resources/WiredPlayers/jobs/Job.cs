using GTANetworkAPI;
using WiredPlayers.globals;
using WiredPlayers.model;
using WiredPlayers.character;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.information;

namespace WiredPlayers.jobs
{
    public class Job : Script
    {
        public static int GetJobPoints(Client player, int job)
        {
            string jobPointsString = player.GetData(EntityData.PLAYER_JOB_POINTS);
            return int.Parse(jobPointsString.Split(',')[job]);
        }

        public static void SetJobPoints(Client player, int job, int points)
        {
            string jobPointsString = player.GetData(EntityData.PLAYER_JOB_POINTS);
            string[] jobPointsArray = jobPointsString.Split(',');
            jobPointsArray[job] = points.ToString();
            jobPointsString = string.Join(",", jobPointsArray);
            player.SetData(EntityData.PLAYER_JOB_POINTS, jobPointsString);
        }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            Blip trashBlip = NAPI.Blip.CreateBlip(new Vector3(-322.088f, -1546.014f, 31.01991f));
            trashBlip.Name = GenRes.garbage_job;
            trashBlip.ShortRange = true;
            trashBlip.Sprite = 318;

            Blip mechanicBlip = NAPI.Blip.CreateBlip(new Vector3(486.5268f, -1314.683f, 29.22961f));
            mechanicBlip.Name = GenRes.mechanic_job;
            mechanicBlip.ShortRange = true;
            mechanicBlip.Sprite = 72;

            Blip fastFoodBlip = NAPI.Blip.CreateBlip(new Vector3(-1037.697f, -1397.189f, 5.553192f));
            fastFoodBlip.Name = GenRes.fastfood_job;
            fastFoodBlip.ShortRange = true;
            fastFoodBlip.Sprite = 501;

            foreach (JobPickModel job in Constants.JOB_PICK_LIST)
            {
                NAPI.TextLabel.CreateTextLabel("/" + Commands.COM_JOB, job.position, 10.0f, 0.5f, 4, new Color(255, 255, 153), false, 0);
                NAPI.TextLabel.CreateTextLabel(GenRes.job_help, new Vector3(job.position.X, job.position.Y, job.position.Z - 0.1f), 10.0f, 0.5f, 4, new Color(0, 0, 0), false, 0);
            }
        }

        [Command(Commands.COM_JOB, Commands.HLP_JOB_COMMAND)]
        public void JobCommand(Client player, string action)
        {
            int faction = player.GetData(EntityData.PLAYER_FACTION);
            int job = player.GetData(EntityData.PLAYER_JOB);

            switch (action.ToLower())
            {
                case Commands.ARG_INFO:
                    foreach (JobPickModel jobPick in Constants.JOB_PICK_LIST)
                    {
                        if (player.Position.DistanceTo(jobPick.position) < 1.5f)
                        {
                            player.SendChatMessage(Constants.COLOR_INFO + jobPick.description);
                            break;
                        }
                    }
                    break;
                case Commands.ARG_ACCEPT:
                    if (faction > 0 && faction < Constants.LAST_STATE_FACTION)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_job_state_faction);
                    }
                    else if (job > 0)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_has_job);
                    }
                    else
                    {
                        foreach (JobPickModel jobPick in Constants.JOB_PICK_LIST)
                        {
                            if (player.Position.DistanceTo(jobPick.position) < 1.5f)
                            {
                                player.SetData(EntityData.PLAYER_JOB, jobPick.job);
                                player.SetData(EntityData.PLAYER_EMPLOYEE_COOLDOWN, 5);
                                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.job_accepted);
                                break;
                            }
                        }
                    }
                    break;
                case Commands.ARG_LEAVE:
                    // Get the hours spent in the current job
                    int employeeCooldown = player.GetData(EntityData.PLAYER_EMPLOYEE_COOLDOWN);

                    if (employeeCooldown > 0)
                    {
                        string message = string.Format(ErrRes.employee_cooldown, employeeCooldown);
                        player.SendChatMessage(Constants.COLOR_ERROR + message);
                    }
                    else if (player.GetData(EntityData.PLAYER_JOB_RESTRICTION) > 0)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_job_restriction);
                    }
                    else if (job == 0)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_no_job);
                    }
                    else
                    {
                        player.SetData(EntityData.PLAYER_JOB, 0);
                        player.SendChatMessage(Constants.COLOR_INFO + InfoRes.job_left);
                    }
                    break;
                default:
                    player.SendChatMessage(Constants.COLOR_HELP + Commands.HLP_JOB_COMMAND);
                    break;
            }
        }

        [Command(Commands.COM_DUTY)]
        public void DutyCommand(Client player)
        {
            // We get the sex, job and faction from the player
            int playerSex = player.GetData(EntityData.PLAYER_SEX);
            int playerJob = player.GetData(EntityData.PLAYER_JOB);
            int playerFaction = player.GetData(EntityData.PLAYER_FACTION);

            if (player.GetData(EntityData.PLAYER_KILLED) != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else if (playerJob == 0 && playerFaction == 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_no_job);
            }
            else if (player.GetData(EntityData.PLAYER_ON_DUTY) == 1)
            {
                // Populate player's clothes
                Customization.ApplyPlayerClothes(player);

                // We set the player off duty
                player.SetData(EntityData.PLAYER_ON_DUTY, 0);

                // Notification sent to the player
                player.SendNotification(InfoRes.player_free_time);
            }
            else
            {
                // Dress the player with the uniform
                foreach (UniformModel uniform in Constants.UNIFORM_LIST)
                {
                    if (uniform.type == 0 && uniform.factionJob == playerFaction && playerSex == uniform.characterSex)
                    {
                        player.SetClothes(uniform.uniformSlot, uniform.uniformDrawable, uniform.uniformTexture);
                    }
                    else if (uniform.type == 1 && uniform.factionJob == playerJob && playerSex == uniform.characterSex)
                    {
                        player.SetClothes(uniform.uniformSlot, uniform.uniformDrawable, uniform.uniformTexture);
                    }
                }

                // We set the player on duty
                player.SetData(EntityData.PLAYER_ON_DUTY, 1);

                // Notification sent to the player
                player.SendNotification(InfoRes.player_on_duty);
            }
        }

        [Command(Commands.COM_ORDERS)]
        public void OrdersCommand(Client player)
        {
            if (player.GetData(EntityData.PLAYER_KILLED) != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else if (player.GetData(EntityData.PLAYER_ON_DUTY) == 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_on_duty);
            }
            else if (player.HasData(EntityData.PLAYER_DELIVER_ORDER) == true)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.order_delivering);
            }
            else
            {
                if (player.GetData(EntityData.PLAYER_JOB) == Constants.JOB_FASTFOOD)
                {
                    // Get the fastfood deliverer orders
                    FastFood.CheckFastfoodOrders(player);
                    return;
                }

                if (player.GetData(EntityData.PLAYER_JOB) == Constants.JOB_TRUCKER)
                {
                    // Get the trucker orders
                    Trucker.CheckTruckerOrders(player);
                    return;
                }
            }
        }
    }
}
