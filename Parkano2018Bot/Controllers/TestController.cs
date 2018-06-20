using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Parkano2018Bot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Parkano2018Bot.Controllers
{
    public class TestController : ApiController
    {
        public async Task<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        public string Get(int id)
        {
            return "value";
        }

        public void Post(UserPollCommand command)
        {
            var botClient = Helper.RunBot();

            //botClient.SendTextMessageAsync(new ChatId("@Parkano2018"), "نتیجه بازی", ParseMode.Default, true, false, 0,
            //    null, CancellationToken.None);
        }

        public void Put(int id, [FromBody]string value)
        {
        }

        public void Delete(int id)
        {
        }
    }

    public class LastGameController : ApiController
    {
        public void Get()
        {
            var telegramBotClient = Helper.RunBot();
            using (var db = new ApplicationContext())
            {
                var game = db.Games.OrderByDescending(x => x.MatchDateTime).Single();
                telegramBotClient.SendTextMessageAsync(new ChatId("@Parkano2018"),
                    $"نتیجه دیدار تیم {game.HomeTeam} و تیم {game.AwayTeam} چه میشود", ParseMode.Default, true, false, 0, null, CancellationToken.None);

            }
        }
    }

    public class Helper
    {
        public static TelegramBotClient RunBot()
        {
            var botClient = new TelegramBotClient("527433668:AAGJYOrhIzAGX2shDXmFBRlCBYif_imDsJI");
            return botClient;
        }
    }
}