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
                case "/play": await PlayBlackJack(bot, chatId, userId, userName, token); break;
                case "/join": await Join(bot, chatId, userId, userName, token); break;
                case "/start": await Start(bot, chatId, token); break;
                case "/hit": await Hit(bot, chatId, userId, userName, token); break;
                case "/stand": await Stand(bot, chatId, userId, userName, token); break;
                case "/show": await Show(bot, chatId, userId, userName, token); break;
                case "/abort": await Abort(bot, chatId, token); break;
                case "/help@vochyK_bot": await ShowCommands(bot, chatId); break;
                case "/play@vochyK_bot": await PlayBlackJack(bot, chatId, userId, userName, token); break;
                case "/join@vochyK_bot": await Join(bot, chatId, userId, userName, token); break;
                case "/start@vochyK_bot": await Start(bot, chatId, token); break;
                case "/hit@vochyK_bot": await Hit(bot, chatId, userId, userName, token); break;
                case "/stand@vochyK_bot": await Stand(bot, chatId, userId, userName, token); break;
                case "/show@vochyK_bot": await Show(bot, chatId, userId, userName, token); break;
                case "/abort@vochyK_bot": await Abort(bot, chatId, token); break;
            }
        }

        static async Task Hit(ITelegramBotClient bot, long chatId, long userId, string userName, CancellationToken token)
        {
            if (!ActiveGames.ContainsKey(chatId) || !ActiveGames[chatId].IsStarted)
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

            await Show(bot, chatId, userId, userName, token);
        }

        static async Task Stand(ITelegramBotClient bot, long chatId, long userId, string userName, CancellationToken token)
        {
            if (!ActiveGames.ContainsKey(chatId) || !ActiveGames[chatId].IsStarted)
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

        static async Task Show(ITelegramBotClient bot, long chatId, long userId, string userName, CancellationToken token)
        {
            if (!ActiveGames.ContainsKey(chatId) || !ActiveGames[chatId].IsStarted)
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

        // To actually play the game
        static async Task Start(ITelegramBotClient bot, long chatId, CancellationToken token)
        {
            if (!ActiveGames.ContainsKey(chatId))
            {
                await bot.SendMessage(chatId, "You haven`t used /play comand!");
                return;
            }

            if (ActiveGames[chatId].IsStarted)
            {
                await bot.SendMessage(chatId, "The game is LIVE!");
                return;
            }
            ActiveGames[chatId].IsStarted = true;

            var dc = ActiveGames[chatId].Start();
            await bot.SendMessage(chatId, "Players got their cards");

            foreach(var pl in ActiveGames[chatId].Players)
            {
                await Show(bot, chatId, pl.Id, pl.Name, token);
            }

            await bot.SendMessage(chatId, $"Dealer got {dc}");

            // Organized not to block the main thread
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
                var ls = ActiveGames[chatId].Players;

                foreach(var el in ls)
                {
                    await bot.SendMessage(chatId, $"{el.Name} got {el.PrintCards()}, with total of {el.ShowValue()}");
                }
                await bot.SendMessage(chatId, $"Dealer got {ActiveGames[chatId].Dealer.PrintCards()}, with total of {ActiveGames[chatId].Dealer.ShowValue()}");

                if (winner.Id == -1)
                    await bot.SendMessage(chatId, $"Unfortunately, diller won");
                else
                    await bot.SendMessage(chatId, $"Congratulations to winner: {winner}");
                ActiveGames.Remove(chatId);
            });
        }

        static async Task Join(ITelegramBotClient bot, long chatId, long userId, string userName, CancellationToken token)
        {
            if (!ActiveGames.ContainsKey(chatId))
            {
                await bot.SendMessage(chatId, "You haven`t started the game");
                return;
            }

            ActiveGames[chatId].AddPlayer(new Player(userId, userName));
        }

        // To start the game
        static async Task PlayBlackJack(ITelegramBotClient bot, long chatId, long userId, string userName, CancellationToken token)
        {
            if(ActiveGames.ContainsKey(chatId)) await bot.SendMessage(chatId, "You are already playing the game");

            var game = new GameSession();
            ActiveGames[chatId] = game;

            await Join(bot, chatId, userId, userName, token);
            
            await bot.SendMessage(chatId, "Waiting for players!");
        }

        static async Task Abort(ITelegramBotClient bot, long chatId, CancellationToken token)
        {
            if (!ActiveGames.ContainsKey(chatId))
            {
                await bot.SendMessage(chatId, "You haven`t started the game!");
                return;
            }

            ActiveGames.Remove(chatId);
            await bot.SendMessage(chatId, "Game has been aborted");
        }

        static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
        {
            Console.WriteLine($"Помилка: {exception.Message}");
            return Task.CompletedTask;
        }

        #region SendMenu(unused)
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

        #endregion

        static async Task ShowCommands(ITelegramBotClient bot, long chatID)
        {
            string list = "";
            list += "/play - Starts a queue for players for BJ game\n";
            list += "/start - Starts a the BJ game\n";
            list += "/join - To join the game\n";
            list += "/show - The bot will send you your cards\n";
            list += "/hit - To take oe more card\n";
            list += "/stand - To be ready to end\n";
            list += "/abort - To abort the game\n";

            await bot.SendMessage(chatID, list);
        }
    }


}
