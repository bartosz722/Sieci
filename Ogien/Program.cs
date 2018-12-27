﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;

namespace Ogien
{
    class Program
    {
        const int serverSendsPort = 9876; // serwer wysyła, klient odbiera
        //const int serverReceivePort = 9877; // serwer odbiera, klient wysyła
        static int bufferSize = 0;
        static bool randomData = false;
        static byte[] writeBuffer;
        readonly static TimeSpan statusPrintInterval = TimeSpan.FromSeconds(1);

        static void Main(string[] args)
        {
            Console.WriteLine("Ogień!");
            ConfigureApp();

            if (args.Length == 1 && args[0] == "client-in") { 
                ClientMode(true);
            }
            else if (args.Length == 1 && args[0] == "client-out") {
                //ClientMode(true);
                throw new NotImplementedException();
            }
            else if (args.Length == 0) {
                ServerMode();
            }
            else {
                Console.WriteLine("Possible arguments: client-in, client-out, no arguments");
            }
        }

        static void ConfigureApp()
        {
            var buffSizeString = ConfigurationManager.AppSettings["BufferSize"];
            Console.WriteLine("Buffer size: " + buffSizeString);
            int mul = 1;
            if (buffSizeString.EndsWith("k")) {
                mul = 1024;
                buffSizeString = buffSizeString.Substring(0, buffSizeString.Length - 1);
            }
            else if (buffSizeString.EndsWith("M")) {
                mul = 1024 * 1024;
                buffSizeString = buffSizeString.Substring(0, buffSizeString.Length - 1);
            }
            bufferSize = int.Parse(buffSizeString) * mul;
            if (bufferSize > 1024*1024*1024) {
                throw new InvalidOperationException("Buffer size is too big: " + bufferSize);
            }
                        
            if (ConfigurationManager.AppSettings["RandomData"] != null) {
                randomData = true;                
            }
            Console.WriteLine("Random data: " + randomData);
        }
               
        static void ServerMode()
        {
            writeBuffer = Utils.CreateBuffer(bufferSize, randomData);
            TcpListener server = new TcpListener(IPAddress.Any, serverSendsPort);
            server.Start();

            while (true) {
                Console.WriteLine("Wait for connection...");
                using (TcpClient client = server.AcceptTcpClient()) {
                    Console.WriteLine("Client connected from " + client.Client.RemoteEndPoint);
                    var stream = client.GetStream();
                    Console.WriteLine("Send test data");
                    SendTestData(stream);
                }
            }
        }

        static void ClientMode(bool modeIn)
        {
            Console.WriteLine("Connect...");
            var client = new TcpClient("localhost", serverSendsPort);
            Console.WriteLine("Connected");
            ReceiveTestData(client.GetStream());
        }

        /// <summary>
        /// Wysyła dane aż do zamknięcia połączenia, potem funkcja się kończy.
        /// </summary>
        static void SendTestData(NetworkStream stream)
        {
            long bytesWritten = 0;
            var startTime = DateTime.Now;
            var lastStatusPrint = startTime;

            while (true) {                
                try {
                    stream.Write(writeBuffer, 0, writeBuffer.Length);
                    bytesWritten += writeBuffer.Length;
                    PrintStatus(ref lastStatusPrint, bytesWritten);
                }
                catch (IOException e) {
                    Console.WriteLine("Write error: " + e.Message);
                    break;
                }
            }

            PrintSummary(startTime, bytesWritten);
        }

        /// <summary>
        /// Wysyła dane aż do zamknięcia połączenia, potem funkcja się kończy.
        /// </summary>
        static void ReceiveTestData(NetworkStream stream)
        {
            var buf = Utils.CreateBuffer(bufferSize, randomData);
            long readCount = 0;
            var startTime = DateTime.Now;
            var lastStatusPrint = startTime;

            while (true) {                
                try {
                    readCount += stream.Read(buf, 0, buf.Length);
                    PrintStatus(ref lastStatusPrint, readCount);
                }
                catch (IOException e) {
                    Console.WriteLine("Read error: " + e.Message);
                    break;
                }                
            }

            PrintSummary(startTime, readCount);
        }

        static void PrintStatus(ref DateTime lastStatusPrint, long bytesWritten)
        {
            var now = DateTime.Now;
            if (now - lastStatusPrint >= statusPrintInterval) {
                Console.WriteLine("Bytes transferred: " + Utils.FormatByteCount(bytesWritten));
                lastStatusPrint = now;                
            }            
        }

        static void PrintSummary(DateTime startTime, long bytesWritten)
        {
            var duration = DateTime.Now - startTime;
            Console.WriteLine("Bytes transferred: " + Utils.FormatByteCount(bytesWritten));
            Console.WriteLine("Duration: " + duration);
            double rate = bytesWritten / duration.TotalSeconds;
            Console.WriteLine(
                "Average transfer rate: {0}/s", 
                Utils.FormatByteCount((long)Math.Round(rate)));
        }
    }
}