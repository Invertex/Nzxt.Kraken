using Nzxt.Kraken.Core;
using System;
using System.Collections.Generic;
using System.CommandLine.Parser;
using System.CommandLine.Parser.Parameters;
using System.Linq;
using System.Runtime.InteropServices;

namespace Nzxt.Kraken.Controller
{
    public static class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetCommandLine();

        private static void GetCommandLine(out string commandLine)
        {
            commandLine = Marshal.PtrToStringUni(GetCommandLine());
        }

        public static int Main(string[] args)
        {
            var color = default(Color);
            var fan = default(byte?);
            var pump = default(byte?);
            var monitor = default(bool);
            GetParameters(out color, out fan, out pump, out monitor);
            return Main(color, fan, pump, monitor);
        }

#pragma warning disable 0028
        public static int Main(Color color, byte? fan, byte? pump, bool monitor)
#pragma warning restore 0028
        {
            try
            {
                using (var device = new Device())
                {
                    var manager = new Manager(device);
                    manager.Start();
                    if (color != null)
                    {
                        manager.SetLightingColor(
                            color.Red,
                            color.Green,
                            color.Blue
                        );
                    }
                    if (fan.HasValue)
                    {
                        manager.SetFanSpeed(fan.Value);
                    }
                    if (pump.HasValue)
                    {
                        manager.SetPumpSpeed(pump.Value);
                    }
                    do
                    {
                        manager.Read(true);
                        Console.WriteLine("Device = {0}, Fan Speed = {1}, Pump Speed = {2}", manager.Id, manager.FanSpeed, manager.PumpSpeed);
                        if (Console.KeyAvailable)
                        {
                            break;
                        }
                    } while (monitor);
                }
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
            return 0;
        }

        private static void GetParameters(out Color color, out byte? fan, out byte? pump, out bool monitor)
        {
            color = default(Color);
            fan = default(byte?);
            pump = default(byte?);
            monitor = default(bool);
            var commandLine = default(string);
            GetCommandLine(out commandLine);
            if (string.IsNullOrEmpty(commandLine))
            {
                return;
            }
            var arguments = commandLine.Split(new[] { "Nzxt.Kraken.Controller.exe", "\"" }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            if (string.IsNullOrEmpty(arguments))
            {
                return;
            }
            var parser = new CommandLineParser();
            var parameters = parser.Parse(arguments);
            var red = GetParameter(parameters.Parameters, "r");
            var green = GetParameter(parameters.Parameters, "g");
            var blue = GetParameter(parameters.Parameters, "b");
            if (red.HasValue && green.HasValue && blue.HasValue)
            {
                color = new Color()
                {
                    Red = red.Value,
                    Green = green.Value,
                    Blue = blue.Value
                };
            }
            fan = GetParameter(parameters.Parameters, "fan");
            pump = GetParameter(parameters.Parameters, "pump");
            monitor = parameters.Parameters.ContainsKey("monitor");
        }

        private static byte? GetParameter(IDictionary<string, Parameter> parameters, string name)
        {
            var parameter = default(Parameter);
            if (!parameters.TryGetValue(name, out parameter))
            {
                return null;
            }
            if (parameter.Kind != ParameterKind.Number)
            {
                return null;
            }
            return Convert.ToByte((parameter as NumberParameter).Value);
        }

        public class Color
        {
            public byte Red { get; set; }

            public byte Green { get; set; }

            public byte Blue { get; set; }
        }
    }
}
