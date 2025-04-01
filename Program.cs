using System;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot
{
    internal class Program
    {
        private static ITelegramBotClient _botClient;

        private static Dictionary<long, GameSession> ActiveGames = new();


        static async Task Main()
        {
            string token = Config.GetBotToken();
            _botClient = new TelegramBotClient(token);

            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            _botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cts.Token);

            var botInfo = await _botClient.GetMe();
            Console.WriteLine($"Bot {botInfo.Username} has been started");

            await Task.Delay(-1);
        }

        static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            if (update.Message is not { } message) return;
            if (message.Text is not { } messageText) return;

            var botInfo = await _botClient.GetMe();
            long chatId = message.Chat.Id;
            long userId = message.From.Id;
            string userName = message.From.Username;


            Console.WriteLine($"Отримано повідомлення: {messageText}, від {chatId}, userName: {userName}");

            switch (messageText)
            {
                case "/help": await ShowCommands(bot, chatId); break;
                case "/play": await PlayBlackJack(bot, chatId, token, userId, userName); break;
                case "/join": await Join(bot, chatId, token, userId, userName); break;
                case "/start": await Start(bot, chatId, token); break;
                case "/hit": await Hit(bot, chatId, token, userId, userName); break;
                case "/stand": await Stand(bot, chatId, token, userId, userName); break;
                case "/show": await Show(bot, chatId, token, userId, userName); break;
                case "/help@vochyK_bot": await ShowCommands(bot, chatId); break;
                case "/play@vochyK_bot": await PlayBlackJack(bot, chatId, token, userId, userName); break;
                case "/join@vochyK_bot": await Join(bot, chatId, token, userId, userName); break;
                case "/start@vochyK_bot": await Start(bot, chatId, token); break;
                case "/hit@vochyK_bot": await Hit(bot, chatId, token, userId, userName); break;
                case "/stand@vochyK_bot": await Stand(bot, chatId, token, userId, userName); break;
                case "/show@vochyK_bot": await Show(bot, chatId, token, userId, userName); break;
            }
        }

        static async Task Hit(ITelegramBotClient bot, long chatId, CancellationToken token, long userId, string userName)
        {
            if (!ActiveGames.ContainsKey(chatId))
            {
                await bot.SendMessage(chatId, "You haven`t started the game!");
                return;
            }

            Player p = ActiveGames[chatId].GetPlayer(userId);
            if (p == null)
            {
                await bot.SendMessage(chatId, "There is no such player with this id");
                return;
            }

            if (p.Stand) return;

            ActiveGames[chatId].HitPlayer(p);

            await Show(bot, chatId, token, userId, userName);
        }

        static async Task Stand(ITelegramBotClient bot, long chatId, CancellationToken token, long userId, string userName)
        {
            if (!ActiveGames.ContainsKey(chatId))
            {
                await bot.SendMessage(chatId, "You haven`t started the game!");
                return;
            }

            Player p = ActiveGames[chatId].GetPlayer(userId);
            if (p == null)
            {
                await bot.SendMessage(chatId, "There is no such player with this id");
                return;
            }

            if (p.Stand) return;

            p.Stand = true;
        }

        static async Task Show(ITelegramBotClient bot, long chatId, CancellationToken token, long userId, string userName)
        {
            if (!ActiveGames.ContainsKey(chatId))
            {
                await bot.SendMessage(chatId, "You haven`t started the game!");
                return;
            }

            Player p = ActiveGames[chatId].GetPlayer(userId);
            if (p == null)
            {
                await bot.SendMessage(chatId, "There is no such player with this id");
                return;
            }

            await bot.SendMessage(userId, $"{userName}, you have {p.PrintCards()}, with a summary of {p.ShowValue()} points");
        }

        static async Task Start(ITelegramBotClient bot, long chatId, CancellationToken token)
        {
            if (!ActiveGames.ContainsKey(chatId))
            {
                await bot.SendMessage(chatId, "You haven`t used /play comand!");
                return;
            }

            if (ActiveGames[chatId].Players == 0)
            {
                await bot.SendMessage(chatId, "No players joined yet!");
                return;
            }

            var dc = ActiveGames[chatId].Start();
            await bot.SendMessage(chatId, "Players got their cards");

            foreach(var pl in ActiveGames[chatId].players)
            {
                await Show(bot, chatId, token, pl.Id, pl.Name);
            }

            await bot.SendMessage(chatId, $"Dealer got {dc}");

            // 1 version
            //while (ActiveGames[chatId].Turn()) {}

            //2 version
            //while (!ActiveGames[chatId].ToEnd()) ;

            //final version
            _ = Task.Run(async () =>
            {
                while (!ActiveGames[chatId].ToEnd())
                {
                    await Task.Delay(500);
                }

                var l = ActiveGames[chatId].DealerLogic();
                foreach (var t in l)
                {
                    await bot.SendMessage(chatId, $"Dealer gets {t}");
                    await Task.Delay(500);
                }

                var winner = ActiveGames[chatId].End();

                await bot.SendMessage(chatId, "Param pam pam....");
                var ls = ActiveGames[chatId].players;

                foreach(var el in ls)
                {
                    await bot.SendMessage(chatId, $"{el.Name} got {el.PrintCards()}, with total of {el.ShowValue()}");
                }
                await bot.SendMessage(chatId, $"Dealer got {ActiveGames[chatId].dealer.PrintCards()}, with total of {ActiveGames[chatId].dealer.ShowValue()}");

                if (winner.Id == -1)
                    await bot.SendMessage(chatId, $"Unfortunately, diller won");
                else
                    await bot.SendMessage(chatId, $"Congratulations to winner: {winner}");
                ActiveGames.Remove(chatId);
            });
        }

        static async Task Join(ITelegramBotClient bot, long chatId, CancellationToken token, long userId, string userName)
        {
            if (!ActiveGames.ContainsKey(chatId))
            {
                await bot.SendMessage(chatId, "You haven`t started the game");
                return;
            }

            ActiveGames[chatId].AddPlayer(new Player(userId, userName));
        }

        static async Task PlayBlackJack(ITelegramBotClient bot, long chatId, CancellationToken token, long userId, string userName)
        {
            if(ActiveGames.ContainsKey(chatId)) await bot.SendMessage(chatId, "You are already playing the game");

            var game = new GameSession();
            ActiveGames[chatId] = game;

            await Join(bot, chatId, token, userId, userName);
            
            await bot.SendMessage(chatId, "Waiting for players!");
        }

        static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
        {
            Console.WriteLine($"Помилка: {exception.Message}");
            return Task.CompletedTask;
        }

        static async Task SendMenu(ITelegramBotClient bot, long chatID)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] {"Список команд", "Про мене"},
                new KeyboardButton[] {"Контакти", "Закрити меню" }
            })
            { ResizeKeyboard = true };

            await bot.SendMessage(chatID, "Ось меню:", replyMarkup: keyboard);
        }

        static async Task ShowCommands(ITelegramBotClient bot, long chatID)
        {
            string list = "";
            list += "/play - Starts a queue for players for BJ game\n";
            list += "/start - Starts a the BJ game\n";
            list += "/join - To join the game\n";
            list += "/show - The bot will send you your cards\n";
            list += "/hit - To take oe more card\n";
            list += "/stand - To be ready to end\n";

            await bot.SendMessage(chatID, list);
        }
    }


}
