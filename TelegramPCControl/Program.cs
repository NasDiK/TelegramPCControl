using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramPCControl
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var botClient = new TelegramBotClient(""); //TODO TOKEN

            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };
            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await botClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();
            Console.WriteLine("Закончил принимать");
        }

        static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
        
            if (update.Message is not { } message)
                return;
     
            if (message.Text is not { } messageText)
                return;


            var chatId = message.Chat.Id;

            Console.WriteLine($"{DateTime.Now.ToLocalTime()} Received a '{messageText}' message in chat {chatId}.");

            var command = messageText.Split(' ');
            var outputMessage = "";
            try
            {
                if (command.Length == 1 || command.Length == 2 || command.Length == 3)
                {
                    switch (command[0])
                    {
                        case "/volume":

                            switch (command[1])
                            {
                                case "show":
                                    {
                                        var result = "";
                                        ProcessStartInfo psiOpt = new ProcessStartInfo(@"cmd", @$"/C cd {Environment.CurrentDirectory+"/SetVol"} && setvol report");
                                        // скрываем окно запущенного процесса
                                        psiOpt.WindowStyle = ProcessWindowStyle.Hidden;
                                        psiOpt.RedirectStandardOutput = true;
                                        psiOpt.UseShellExecute = false;
                                        psiOpt.CreateNoWindow = true;
                                        // запускаем процесс
                                        Process procCommand = Process.Start(psiOpt);
                                        // получаем ответ запущенного процесса
                                        StreamReader srIncoming = procCommand.StandardOutput;
                                        // выводим результат
                                        result += srIncoming.ReadToEnd();
                                        // закрываем процесс
                                        procCommand.WaitForExit();
                                        outputMessage = $"Выполнено: {messageText}. Результат: {result}";
                                    }
                                    break;
                                case "set":
                                    try
                                    {
                                        ProcessStartInfo psiOpt = new ProcessStartInfo(@"cmd", $"/C cd {Environment.CurrentDirectory + "/SetVol"} & setvol {command[2]}");
                                        // скрываем окно запущенного процесса
                                        psiOpt.WindowStyle = ProcessWindowStyle.Hidden;
                                        psiOpt.RedirectStandardOutput = true;
                                        psiOpt.UseShellExecute = false;
                                        psiOpt.CreateNoWindow = true;
                                        // запускаем процесс
                                        Process procCommand = Process.Start(psiOpt);
                                        procCommand?.WaitForExit();
                                        outputMessage = $"Выполнено: {messageText}.";
                                    }
                                    catch
                                    {
                                        Console.WriteLine("Ошибка выполнения команды");
                                    }
                                    break;
                                default:
                                    outputMessage = $"Неизвестный аргумент";
                                    break;

                            }

                            break;
                        case "/help":
                            outputMessage = @"
/help - список доступных комманд
/volume show - текущий звук
/volume set [0-100] - установить громкость
/shutdown -s - полное выключение
/shutdown -с - отмена отключения
";
                            break;
                        case "/start":
                            outputMessage = @"
/help - список доступных комманд
/volume show - текущий звук
/volume set [0-100] - установить громкость
/shutdown -s - полное выключение
/shutdown -с - отмена отключения
";
                            break;
                        case $"/shutdown":
                            switch (command[1])
                            {
                                case "-s":
                                    try
                                    {
                                        outputMessage = $"Выполнено: {messageText}. Компьютер выключится менее чем через 1 минуту";
                                        ProcessStartInfo psiOpt = new ProcessStartInfo(@"cmd", $"/C shutdown -s");
                                        // скрываем окно запущенного процесса
                                        psiOpt.WindowStyle = ProcessWindowStyle.Hidden;
                                        psiOpt.RedirectStandardOutput = true;
                                        psiOpt.UseShellExecute = false;
                                        psiOpt.CreateNoWindow = true;
                                        // запускаем процесс
                                        Process procCommand = Process.Start(psiOpt);
                                        procCommand?.WaitForExit();
                                    }
                                    catch
                                    {
                                        Console.WriteLine("Ошибка выполнения команды");
                                    }
                                    break;
                                case "-c":
                                    try
                                    {
                                        outputMessage = $"Выполнено: {messageText}. Запланированное отключение отменено";
                                        ProcessStartInfo psiOpt = new ProcessStartInfo(@"cmd", $"/C shutdown/a");
                                        // скрываем окно запущенного процесса
                                        psiOpt.WindowStyle = ProcessWindowStyle.Hidden;
                                        psiOpt.RedirectStandardOutput = true;
                                        psiOpt.UseShellExecute = false;
                                        psiOpt.CreateNoWindow = true;
                                        // запускаем процесс
                                        Process procCommand = Process.Start(psiOpt);
                                        procCommand?.WaitForExit();
                                    }
                                    catch
                                    {
                                        Console.WriteLine("Ошибка выполнения команды");
                                    }
                                    break;
                                default:
                                    break;
                            } 
                            break;
                        default:
                            outputMessage = $"Неизвестная команда: {messageText}";
                            break;
                    }
                }

                else
                {
                    outputMessage = $"Незнаемо такой команды: {messageText}";
                }
            }
            catch (Exception ex) { outputMessage = $"{ex.Message}. Иначе говоря: что-то ты не то написал дядя...все работало, я проверял.."; }
            if (outputMessage != "")
            {
                Message sentMessage = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: outputMessage,
                        cancellationToken: cancellationToken);
            }


        }

        static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
