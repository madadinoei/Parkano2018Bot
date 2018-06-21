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
using Telegram.Bot.Types.ReplyMarkups;
using static System.Data.Entity.Core.Objects.EntityFunctions;

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
            try
            {
                var telegramBotClient = Helper.RunBot();
                using (var db = new ApplicationContext())
                {
                    var game = db.Games.Where(x => x.MatchDateTime >= DateTime.Now).OrderByDescending(x => x.MatchDateTime).SingleOrDefault();
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new [] // first row
                        {
                            InlineKeyboardButton.WithCallbackData("1.1"),
                            InlineKeyboardButton.WithCallbackData("1.2"),
                        },
                        new [] // second row
                        {
                            InlineKeyboardButton.WithCallbackData("2.1"),
                            InlineKeyboardButton.WithCallbackData("2.2"),
                        }
                    });
                     telegramBotClient.SendChatActionAsync(telegramBotClient.GetMeAsync().Result.Id, ChatAction.Typing);

                     telegramBotClient.SendTextMessageAsync(
                        telegramBotClient.GetMeAsync().Result.Id,
                        "Choose",
                        replyMarkup: inlineKeyboard);

                    //return await telegramBotClient.SendTextMessageAsync(new ChatId("@Parkano2018"),
                    //    $"نتیجه دیدار تیم {game.HomeTeam} و تیم {game.AwayTeam} چه میشود", ParseMode.Default, false, false, 50, inlineKeyboard);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
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