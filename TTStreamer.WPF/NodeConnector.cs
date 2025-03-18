// NodeConnector.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace TTStreamer.WPF
{
    public class NodeConnector : IDisposable
    {
        private Process _process;
        private StreamWriter _writer;
        private StreamReader _reader;

        public NodeConnector()
        {
            _process = new Process();
            _process.StartInfo.FileName = "node";
            _process.StartInfo.Arguments = "tiktok-connector.js";
            _process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory(); // Убедитесь, что файл находится в текущей директории
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;
            _process.StartInfo.CreateNoWindow = true;
            _process.Start();
            _writer = _process.StandardInput;
            _reader = _process.StandardOutput;
        }

        public async Task<string> SendCommandAsync(string command, Dictionary<string, object> parameters)
        {
            var request = new
            {
                Command = command,
                Parameters = parameters
            };
            string jsonRequest = JsonSerializer.Serialize(request);
            _writer.WriteLine(jsonRequest);
            _writer.Flush();
            string response = await _reader.ReadLineAsync();
            return response;
        }

        public void Dispose()
        {
            _writer?.Close();
            _reader?.Close();
            _process?.Kill();
            _process?.Dispose();
        }
    }
}