using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Parkano2018Bot.Models;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using User = Parkano2018Bot.Models.User;

namespace Parkano2018Bot
{
    public class Program
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("527433668:AAGJYOrhIzAGX2shDXmFBRlCBYif_imDsJI");

        static void Main(string[] args)
        {
            string baseAddress = "http://localhost:9010/";
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
               /inline   - send inline keyboard
               /list - لیست مسابقات پیش رو
               /mobile  - وارد کردن شماره تماس";

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;

            if (message.Type == MessageType.Text)
            {
                switch (message.Text.Split(' ').First())
                {
                    // send inline keyboard
                    case "/inline":
                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                        await Task.Delay(500); // simulate longer running task
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

                        //await Bot.SendTextMessageAsync(
                        //    message.Chat.Id,
                        //    "Choose",
                        //    replyMarkup: kmu);
                        break;

                    // send custom keyboard
                    case "/keyboard":
                        ReplyKeyboardMarkup replyKeyboard = new[]
                        {
                        new[] { "1.1", "1.2" },
                        new[] { "2.1", "2.2" },
                    };

                        await Bot.SendTextMessageAsync(
                            message.Chat.Id,
                            "Choose",
                            replyMarkup: replyKeyboard);
                        break;

                    // request location or contact
                    case "/mobile":
                        var requestReplyKeyboard = new ReplyKeyboardMarkup(new[]
                        {
                        KeyboardButton.WithRequestContact("شماره موبایلم را به بات میدهم"),
                    });

                        await Bot.SendTextMessageAsync(
                            message.Chat.Id,
                            "لطفا شماره موبایل خود را وارد کنید",
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
                var games = db.Games.Where(x => x.MatchDateTime >= DateTime.Now).OrderByDescending(x => x.MatchDateTime).ToList();
                var inlineKeyboard = new InlineKeyboardMarkup(games.Select(x => new InlineKeyboardButton()
                {
                    Text = x.HomeTeam + " - " + x.AwayTeam,
                    CallbackData = x.Id.ToString(),
                }));

                await Bot.SendTextMessageAsync(
                    message.Chat.Id,
                    "Choose",
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
                    var gameId = Guid.Parse(callbackQuery.Data);
                    var poll = user.Polls.SingleOrDefault(x => x.GameId == gameId);
                    if (poll != null)
                    {
                        await Bot.SendTextMessageAsync(
                            callbackQuery.Message.Chat.Id,
                            "شما قبلا نتیجه این بازی را پیش بینی کرده اید");
                    }
                    else
                    {
                        var game = db.Games.AsQueryable().SingleOrDefault(x => x.Id == gameId);
                        //await Bot.AnswerCallbackQueryAsync(
                        //    callbackQuery.Id,
                        //    $"Received {game.HomeTeam} - {game.AwayTeam}");

                        await Bot.SendTextMessageAsync(
                            callbackQuery.Message.Chat.Id,
                            $"بازی انتخابی :  {game.HomeTeam} - {game.AwayTeam}" + "\n");
                        //var rkb = new ReplyKeyboardMarkup(new List<KeyboardButton>
                        //{
                        //    new KeyboardButton("1"),
                        //    new KeyboardButton("2")
                        //},true,true);
                        var inlineKeyboard = new InlineKeyboardMarkup(new List<InlineKeyboardButton>()
                        {
                            new InlineKeyboardButton()
                            {
                                Text = "1",
                                CallbackData = "1"
                            },new InlineKeyboardButton()
                            {
                                Text = "2",
                                CallbackData = "2"
                            },new InlineKeyboardButton()
                            {
                                Text = "3",
                                CallbackData = "3"
                            },new InlineKeyboardButton()
                            {
                                Text = "4",
                                CallbackData = "4"
                            },new InlineKeyboardButton()
                            {
                                Text = "5",
                                CallbackData = "5"
                            },
                        });
                        var message = await Bot.SendTextMessageAsync(
                            callbackQuery.Message.Chat.Id,
                            $"تعداد گل های {game.HomeTeam} را وارد کنید", ParseMode.Default, false, false, 0, inlineKeyboard);

                    }
                }
                else
                {
                    await Help(callbackQueryEventArgs.CallbackQuery.Message);
                }
            }



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
