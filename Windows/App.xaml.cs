using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RuhiBrowser
{
    public partial class App : Application
    {
        private const string MutexName = "RuhiBrowser_SingleInstance_Mutex_2026";
        private const string PipeName = "RuhiBrowser_IpcPipe_2026";
        private static Mutex? _instanceMutex;
        private static CancellationTokenSource? _pipeCts;

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                try { File.AppendAllText("D:\\RUHI Apps\\RUHI Browser\\Windows\\crash.log", $"[AppDomain] {DateTime.Now}: {e.ExceptionObject}\n"); } catch { }
            };
            DispatcherUnhandledException += (s, e) =>
            {
                try { File.AppendAllText("D:\\RUHI Apps\\RUHI Browser\\Windows\\crash.log", $"[Dispatcher] {DateTime.Now}: {e.Exception}\n"); } catch { }
            };
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                try { File.AppendAllText("D:\\RUHI Apps\\RUHI Browser\\Windows\\crash.log", $"[TaskScheduler] {DateTime.Now}: {e.Exception}\n"); } catch { }
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            bool createdNew;
            try
            {
                _instanceMutex = new Mutex(true, MutexName, out createdNew);
            }
            catch (AbandonedMutexException)
            {
                createdNew = true;
            }
            catch
            {
                createdNew = true;
            }

            if (!createdNew)
            {
                // Another instance of RuhiBrowser is already running
                string targetArg = e.Args.Length > 0 ? e.Args[0] : string.Empty;
                bool sent = SendArgsToRunningInstance(targetArg);
                if (sent)
                {
                    // Existing instance received the message and will bring itself to front
                    Shutdown();
                    return;
                }
                // If named pipe was not reachable, the other instance is dead or ghosted.
                // Do not shutdown; proceed to open normally!
            }

            base.OnStartup(e);
            StartIpcServer();
        }

        private static bool SendArgsToRunningInstance(string arg)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                client.Connect(500);
                using var writer = new StreamWriter(client, Encoding.UTF8) { AutoFlush = true };
                writer.WriteLine(arg ?? string.Empty);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void StartIpcServer()
        {
            _pipeCts = new CancellationTokenSource();
            var token = _pipeCts.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        using var server = new NamedPipeServerStream(
                            PipeName,
                            PipeDirection.In,
                            1,
                            PipeTransmissionMode.Byte,
                            PipeOptions.Asynchronous
                        );
                        await server.WaitForConnectionAsync(token);
                        using var reader = new StreamReader(server, Encoding.UTF8);
                        string? line = await reader.ReadLineAsync();

                        _ = Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (MainWindow is MainWindow mainWin)
                            {
                                if (mainWin.WindowState == WindowState.Minimized)
                                {
                                    mainWin.WindowState = WindowState.Normal;
                                }
                                mainWin.Activate();
                                mainWin.Focus();
                                mainWin.HandleExternalUrl(line);
                            }
                        }));
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch
                    {
                        try { await Task.Delay(200, token); } catch { }
                    }
                }
            }, token);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                _pipeCts?.Cancel();
                _instanceMutex?.ReleaseMutex();
                _instanceMutex?.Dispose();
            }
            catch { }
            base.OnExit(e);
        }
    }
}
