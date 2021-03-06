﻿using Microsoft.Extensions.Logging;
using SharpDeck;
using SharpDeck.Events.Received;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FlightStreamDeck.Logics
{
    public class NumpadParams
    {
        public NumpadParams(string type, string min, string max, string mask, string regex, bool dependant)
        {
            Type = type;
            MinPattern = min;
            MaxPattern = max;
            Mask = mask;
            Regex = regex;
            Dependant = dependant;
        }

        public string Type { get; }
        public string MinPattern { get; }
        public string MaxPattern { get; }
        public string _Value { get; set; } = "";
        public string Mask { get; set; } = "xxx.xx";
        public string Regex { get; set; } = string.Empty;
        public bool Dependant { get; }

        public bool IsXPDR { 
            get {
                return Type == "XPDR";
            }
        }

        public string ValueUnpadded { get {
                return _Value;
            } 
        }

        public string Value { 
            get {
                return this._Value.PadRight(this.MinPattern.Length, '0');
            }
            set {
                _Value = value;
            }
        }
    }

    public class DeckLogic
    {
        public static NumpadParams NumpadParams { get; set; }
        public static TaskCompletionSource<(string value, bool swap)> NumpadTcs { get; set; }

        private readonly ILoggerFactory loggerFactory;
        private readonly IServiceProvider serviceProvider;

        public DeckLogic(ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            this.loggerFactory = loggerFactory;
            this.serviceProvider = serviceProvider;
        }

        public void Initialize()
        {
            var args = Environment.GetCommandLineArgs();
            loggerFactory.CreateLogger<DeckLogic>().LogInformation("Initialize with args: {args}", string.Join("|", args));

            var plugin = StreamDeckPlugin.Create(args[1..], Assembly.GetAssembly(GetType())).WithServiceProvider(serviceProvider);
            
            Task.Run(() =>
            {
                plugin.Run(); // continuously listens until the connection closes
            });
        }
    }
}
