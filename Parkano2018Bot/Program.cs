using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parkano2018Bot.Enums;
using Parkano2018Bot.Models;
using Parkano2018Bot.Properties;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using Game = Parkano2018Bot.Models.Game;
using User = Parkano2018Bot.Models.User;

namespace Parkano2018Bot
{
    public class Program
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("527433668:AAGJYOrhIzAGX2shDXmFBRlCBYif_imDsJI");

        public static class Constants
        {
            public static int LastGameId;
            public static string HomeOrAway = null;

        }
        static void Main(string[] args)
        {
            string baseAddress = Settings.Default.host;
            var me = Bot.GetMeAsync().Result;
            Console.Title = me.Username;

            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            Bot.OnInlineQuery += BotOnInlineQueryReceived;
            Bot.OnReceiveError += BotOnReceiveError;


            Bot.StartReceiving(Array.Empty<UpdateType>());
            using (var db = new ApplicationContext())
            {
                var count = db.Games.Count();
                db.SaveChanges();
            }

            // Start OWIN host 
            using (WebApp.Start<Startup>(url: baseAddress))
            {
                Console.WriteLine("api is run on " + baseAddress);
                // Create HttpCient and make a request to api/values 
                Console.ReadLine();
            }
        }
        private static async Task Help(Message message)
        {
            await Bot.SendTextMessageAsync(
                message.Chat.Id,
                Usage,
                replyMarkup: new ReplyKeyboardRemove());
        }
        public const string Usage = @"
               راهنمای استفاده:
               /help   - راهنما
               /list - لیست مسابقات پیش رو
               /mobile  - ثبت نام-وارد کردن شماره موبایل";

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message.Type == MessageType.Text)
            {
                switch (message.Text.Split(' ').First())
                {

                    // request location or contact
                    case "/mobile":
                        var requestReplyKeyboard = new ReplyKeyboardMarkup(new[]
                        {

                            KeyboardButton.WithRequestContact("شماره موبایلم را به بات میدهم")

                        });

                        await Bot.SendTextMessageAsync(
                            message.Chat.Id,
                            "لطفا شماره موبایل خود را به بات بدهید",
                            replyMarkup: requestReplyKeyboard);
                        break;


                    case "/help":
                        await Help(message);
                        break;


                    case "/list":
                        GetFutureGames(message, DateTime.Now);
                        break;
                    default:

                        await Bot.SendTextMessageAsync(
                            message.Chat.Id,
                            Usage,
                            replyMarkup: new ReplyKeyboardRemove());
                        break;
                }

            }
            else if (message.Type == MessageType.Contact)
            {
                using (var db = new ApplicationContext())
                {
                    var find = db.Users.AsQueryable().SingleOrDefault(x => x.PhoneNumber == message.Contact.PhoneNumber);
                    if (find == null)
                    {
                        var user = new User()
                        {
                            Id = Guid.NewGuid(),
                            FirstName = message.Contact.FirstName,
                            LastName = message.Contact.LastName,
                            PhoneNumber = message.Contact.PhoneNumber,
                            TelegramUserId = message.Contact.UserId
                        };
                        db.Users.Add(user);
                        db.SaveChanges();
                        await Help(message);
                    }
                    else
                    {
                        await Bot.SendTextMessageAsync(
                            message.Chat.Id,
                            "شما قبلا ثبت نام کرده اید",
                            replyMarkup: new ReplyKeyboardRemove());
                    }
                }
            }

        }



        private static async void GetFutureGames(Message message, DateTime now)
        {
            using (var db = new ApplicationContext())
            {
                var games = db.Games.Where(x => x.MatchDateTime >= now).OrderByDescending(x => x.MatchDateTime).Take(3).ToList();
                var inlineKeyboard = new InlineKeyboardMarkup(games.Select(x => new InlineKeyboardButton()
                {
                    Text = x.HomeTeam + " - " + x.AwayTeam,
                    CallbackData = x.Id.ToString()
                }));

                await Bot.SendTextMessageAsync(
                    message.Chat.Id,
                    "بازی مد نظر خود را انتخاب کنید",
                    replyMarkup: inlineKeyboard);
            }

        }

        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;
            using (var db = new ApplicationContext())
            {
                var user = db.Users.AsQueryable().SingleOrDefault(x => x.TelegramUserId == callbackQuery.From.Id);
                if (user != null)
                {
                    try
                    {
                        var data = JsonConvert.DeserializeObject<GamePollCallBack>(callbackQuery.Data);
                        var gamePoll = JsonConvert.DeserializeObject<GamePoll>(callbackQuery.Data);

                        var userpollgame = user.Polls.SingleOrDefault(x => x.GameId == data.GameId);
                        var game = db.Games.AsQueryable().SingleOrDefault(x => x.Id == data.GameId);

                        if (data.TeamType == TeamType.Home && gamePoll.Team == null)
                        {
                            await GetHomeTeam(game, callbackQuery);
                        }
                        if (DateTime.Now >= game.MatchDateTime.AddMinutes(-10))
                        {
                            await Bot.SendTextMessageAsync(
                                callbackQuery.Message.Chat.Id,
                                "مهلت ثبت/ویرایش نتیجه به پایان رسیده است");
                            await Help(callbackQuery.Message);
                        }
                        if (data.TeamType == TeamType.Home && gamePoll.Team != null && DateTime.Now < game.MatchDateTime.AddMinutes(-10))
                        {
                            if (userpollgame == null)
                            {
                                user.Polls.Add(new Poll()
                                {
                                    Id = Guid.NewGuid(),
                                    GameId = game.Id,
                                    HomeGoal = gamePoll.Goal,
                                    AwayGoal = -1,
                                    CanEdit = true,
                                    DateTime = DateTime.Now
                                });
                            }
                            else
                            {
                                userpollgame.HomeGoal = gamePoll.Goal;
                            }

                            db.SaveChanges();
                            await Bot.AnswerCallbackQueryAsync(
                                callbackQuery.Id,
                                $"تعداد گل انتخابی شما برای {game.HomeTeam} برابر {gamePoll.Goal} میباشد", false, null, 0, CancellationToken.None);
                            var poll = user.Polls.SingleOrDefault(x => x.GameId == game.Id);

                            if (poll.AwayGoal < 0)
                            {
                                await GetAwayTeam(game, callbackQuery);
                            }
                            else
                            {
                                await GetPollResult(callbackQuery, game, poll);

                            }
                        }
                        if (data.TeamType == TeamType.Away && gamePoll.Team == null)
                        {
                            await GetAwayTeam(game, callbackQuery);
                        }
                        if (data.TeamType == TeamType.Away && gamePoll.Team != null && DateTime.Now < game.MatchDateTime.AddMinutes(-10))
                        {
                            if (userpollgame == null)
                            {
                                user.Polls.Add(new Poll()
                                {
                                    Id = Guid.NewGuid(),
                                    GameId = game.Id,
                                    AwayGoal = gamePoll.Goal,
                                    HomeGoal = -1,
                                    CanEdit = true,
                                    DateTime = DateTime.Now
                                });
                            }
                            else
                            {
                                userpollgame.AwayGoal = gamePoll.Goal;
                            }

                            db.SaveChanges();
                            await Bot.AnswerCallbackQueryAsync(
                                callbackQuery.Id,
                                $"تعداد گل انتخابی شما برای {game.AwayTeam} برابر {gamePoll.Goal} میباشد", false, null, 0, CancellationToken.None);
                            var poll = user.Polls.SingleOrDefault(x => x.GameId == game.Id);
                            //if (poll.AwayGoal > -1 && poll.HomeGoal > -1)
                            //{
                            //    GetFutureGames(callbackQuery.Message, DateTime.Now);
                            //}
                            if (poll.HomeGoal < 0)
                            {
                                await GetHomeTeam(game, callbackQuery);
                            }
                            else
                            {
                                await GetPollResult(callbackQuery, game, poll);
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        var lastGameId = int.Parse(callbackQuery.Data);
                        var game = db.Games.AsQueryable().SingleOrDefault(x => x.Id == lastGameId);

                        try
                        {
                            var gameId = lastGameId;
                            var poll = user.Polls.SingleOrDefault(x => x.GameId == gameId);
                            if (poll != null && poll.CanEdit == false)
                            {
                                await Bot.SendTextMessageAsync(
                                    callbackQuery.Message.Chat.Id,
                                    "شما قبلا نتیجه این بازی را پیش بینی کرده اید");
                            }
                            else
                            {
                                //await Bot.AnswerCallbackQueryAsync(
                                //    callbackQuery.Message.Chat.Id.ToString(),
                                //    $"Received {game.HomeTeam} - {game.AwayTeam}",false,null,0,CancellationToken.None);

                                await Bot.SendTextMessageAsync(
                                    callbackQuery.Message.Chat.Id,
                                    $"بازی انتخابی :  {game.HomeTeam} - {game.AwayTeam}" + "\n");

                                var gamePollCallBack = new GamePollCallBack()
                                {
                                    GameId = gameId,
                                    TeamType = TeamType.Home
                                };
                                var json = JsonConvert.SerializeObject(gamePollCallBack, Formatting.Indented);


                                var pollCallBack = new GamePollCallBack()
                                {
                                    GameId = gameId,
                                    TeamType = TeamType.Away
                                };
                                var awayjson = JsonConvert.SerializeObject(pollCallBack, Formatting.Indented);
                                var team = new InlineKeyboardMarkup(new List<InlineKeyboardButton>()
                            {
                                new InlineKeyboardButton()
                                {
                                    Text = game.HomeTeam,
                                    CallbackData = json
                                },
                                new InlineKeyboardButton()
                                {
                                    Text = game.AwayTeam,
                                    CallbackData = awayjson
                                }
                            });
                                await Bot.SendTextMessageAsync(
                                    callbackQuery.Message.Chat.Id,
                                    $"تیم مد نظر خود را انتخاب کنید", ParseMode.Default, false, false, 0, team);
                            }

                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.Contains("Guid should contain 32 digits with 4 dashes"))
                            {
                                Console.WriteLine(Constants.LastGameId);
                            }
                        }
                    }
                }
                else
                {
                    await Bot.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        "ابتدا ثبت نام کنید");
                    await Help(callbackQueryEventArgs.CallbackQuery.Message);
                }
            }



        }

        private static async Task GetPollResult(CallbackQuery callbackQuery, Game game, Poll poll)
        {
            await Bot.SendTextMessageAsync(
                callbackQuery.Message.Chat.Id,
                "نتیجه پیش بینی شما  \n" + $"#{game.HomeTeam} : {poll.HomeGoal} \n #{game.AwayTeam} : {poll.AwayGoal}");
            GetFutureGames(callbackQuery.Message, DateTime.Now);
        }

        private static async Task GetHomeTeam(Game game, CallbackQuery callbackQuery)
        {
            var inlineKeyboardButtons = new List<InlineKeyboardButton>() { };
            var inlineKeyboard = new InlineKeyboardMarkup(inlineKeyboardButtons);
            for (var i = 0; i < 9; i++)
            {
                inlineKeyboardButtons.Add(new InlineKeyboardButton()
                {
                    Text = i.ToString(),
                    CallbackData = JsonConvert.SerializeObject(new GamePoll()
                    {
                        GameId = game.Id,
                        Team = game.HomeTeam,
                        TeamType = TeamType.Home.ToString(),
                        Goal = i
                    })
                });
            }

            await Bot.SendTextMessageAsync(
                callbackQuery.Message.Chat.Id,
                $"تعداد گل های {game.HomeTeam} را وارد کنید", ParseMode.Default, false, false, 0, inlineKeyboard);
        }

        private static async Task GetAwayTeam(Game game, CallbackQuery callbackQuery)
        {
            var inlineKeyboardButtons = new List<InlineKeyboardButton>() { };

            var inlineKeyboard = new InlineKeyboardMarkup(inlineKeyboardButtons);
            for (var i = 0; i < 9; i++)
            {
                inlineKeyboardButtons.Add(new InlineKeyboardButton()
                {
                    Text = i.ToString(),
                    CallbackData = JsonConvert.SerializeObject(new GamePoll()
                    {
                        GameId = game.Id,
                        Team = game.AwayTeam,
                        TeamType = TeamType.Away.ToString(),
                        Goal = i
                    })
                });
            }

            await Bot.SendTextMessageAsync(
                callbackQuery.Message.Chat.Id,
                $"تعداد گل های {game.AwayTeam} را وارد کنید", ParseMode.Default, false, false, 0, inlineKeyboard);
        }



        private static async void BotOnInlineQueryReceived(object sender, InlineQueryEventArgs inlineQueryEventArgs)
        {
            Console.WriteLine($"Received inline query from: {inlineQueryEventArgs.InlineQuery.From.Id}");

            InlineQueryResultBase[] results = {
                new InlineQueryResultLocation(
                    id: "1",
                    latitude: 40.7058316f,
                    longitude: -74.2581888f,
                    title: "New York")   // displayed result
                    {
                        InputMessageContent = new InputLocationMessageContent(
                            latitude: 40.7058316f,
                            longitude: -74.2581888f)    // message if result is selected
                    },

                new InlineQueryResultLocation(
                    id: "2",
                    latitude: 13.1449577f,
                    longitude: 52.507629f,
                    title: "Berlin") // displayed result
                    {

                        InputMessageContent = new InputLocationMessageContent(
                            latitude: 13.1449577f,
                            longitude: 52.507629f)   // message if result is selected
                    }
            };

            await Bot.AnswerInlineQueryAsync(
                inlineQueryEventArgs.InlineQuery.Id,
                results,
                isPersonal: true,
                cacheTime: 0);
        }

        private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Console.WriteLine($"Received inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Console.WriteLine("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message);
        }

    }
}
