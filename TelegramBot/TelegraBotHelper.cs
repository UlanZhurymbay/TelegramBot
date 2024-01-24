using System;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    internal class TelegraBotHelper
    {
        private const string Product = "Товары";
        private const string Basket = "Корзина";
        private const string Start = "/start";
        private string _token;
        private TelegramBotClient _client;
        private Dictionary<long, UserState> _clientStates = new Dictionary<long, UserState>();
        private CancellationTokenSource _srcToken = new();
        internal TelegraBotHelper(string token)
        {
            this._token = token;
        }

        internal void StartReceiving()
        {
            _client = new TelegramBotClient(_token);

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new()
            {
                ThrowPendingUpdates = false,
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
            };

            _client.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: _srcToken.Token
            );
        }

        //Любой сообщение приходит сюда
        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            switch (update.Type)
            {
                //только сообщение от клиента
                case UpdateType.Message:
                    var text = update.Message.Text;
                    await Console.Out.WriteLineAsync(text);
                    var chatId = update.Message.Chat.Id;


                    {
                        switch (text)
                        {
                            case Start:
                                // опять передаем клавиатуру в параметр replyMarkup
                                await botClient.SendTextMessageAsync(
                                                                  chatId,
                                                                  "Добро пожаловать наш мазазин!",
                                                                  replyMarkup: GetButtons()); 
                                break;
                            case Product:


                                await _client.SendPhotoAsync(
                                    update.Message.Chat.Id,
                                    InputFile.FromUri("https://proza.ru/pics/2020/10/31/819.jpg"),
                                    caption: "Помидоры",
                                    replyMarkup: GetInlineButton(1, "Помидоры", 0));

                                await _client.SendPhotoAsync(
                                    update.Message.Chat.Id,
                                    InputFile.FromUri("https://prosad.ru/wp-content/uploads/loaded/ba5d82d833a4b90083f7b362e7.jpg"),
                                    caption: "Капуста",
                                    replyMarkup: GetInlineButton(2, "Капуста", 0));

                                break;
                            default:
                                await botClient.SendTextMessageAsync(
                                                                  chatId,
                                                                  "Чтобы начать нажмите '/start'");
                                break;
                        }
                    }
                    break;

                //для кнопки
                case UpdateType.CallbackQuery:
                    switch (update.CallbackQuery.Data)
                    {
                        case "1":
                            var msg1 = _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Заказ {update.CallbackQuery.Data} принят", replyMarkup: GetButtons()).Result;
                            break;
                        case "2":
                            var msg2 = _client.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, $"Заказ {update.CallbackQuery.Data} принят", replyMarkup: GetButtons()).Result;
                            break;

                    }
                    break;
                default:
                    Console.WriteLine(update.Type + " Not ipmlemented!");
                    break;
            }
        }

        //ошибка
        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception.Message);
            return Task.CompletedTask;
        }


        private IReplyMarkup GetInlineButton(int id, string product, int counter)
        {
            return new InlineKeyboardMarkup(
                // keyboard
                new[]
                {
                    // first row
                    new[]
                    {
                        // first button in row
                         InlineKeyboardButton.WithCallbackData( "-", "decrement _counter"),
                        // second button in row
                        InlineKeyboardButton.WithCallbackData( $"{counter}"),
                        InlineKeyboardButton.WithCallbackData( "+", "increase_counter" )
                    },
                    // second row
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Заказать", id.ToString())
                    },

                });


        }


        private IReplyMarkup GetButtons()
        {
            return new ReplyKeyboardMarkup(
                keyboard: new List<List<KeyboardButton>>
                                {
                                    new List<KeyboardButton>
                                    {  
                                        new KeyboardButton(Product) , 
                                        new KeyboardButton(Basket),  
                                    }
                                })
            {
                ResizeKeyboard = true
            };
        }

        ~TelegraBotHelper()
        {
            _srcToken.Dispose();
        }
    }
}
