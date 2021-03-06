﻿using System;
using System.IO;
using System.Reflection;
using BabylonExport.Exporters;

namespace BabylonExport
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    DisplayUsage();
                    return;
                }

                // Parsing arguments
                string input = "";
                string output = "";
                bool skinned = false;
                foreach (var arg in args)
                {
                    var order = arg.Substring(0, 3);

                    switch (order)
                    {
                        case "/i:":
                            input = arg.Substring(3);
                            break;
                        case "/o:":
                            output = arg.Substring(3);
                            break;
                        case "/sk":
                            skinned = true;
                            break;
                        default:
                            DisplayUsage();
                            return;
                    }
                }

                if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(output))
                {
                    DisplayUsage();
                    return;
                }
                var extension = Path.GetExtension(input).ToLower();
                var outputName = Path.Combine(output, Path.GetFileNameWithoutExtension(input) + ".babylon");
                if (!Directory.Exists(output))
                {
                    Directory.CreateDirectory(output);
                }

                // Browsing exporters
                foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    var interf = type.GetInterface("BabylonExport.IExporter");
                    if (interf != null)
                    {
                        var importer = (IExporter)Activator.CreateInstance(type);

                        if (!importer.SupportedExtensions.Contains(extension))
                        {
                            continue;
                        }

                        Console.WriteLine("Using " + type);

                        // Importation
                        try
                        {
                            importer.OnImportProgressChanged += progress =>
                                {
                                    Console.CursorLeft = 0;
                                    Console.Write("Generation....{0} %", progress);
                                };

                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("Generation of " + outputName + " started");
                            Console.WriteLine();
                            Console.ResetColor();
                            importer.GenerateBabylonFile(input, outputName, skinned);
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine();
                            Console.WriteLine();
                            Console.WriteLine("Generation of " + outputName + " successfull");
                            Console.ResetColor();
                        }
                        catch (Exception ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine();
                            Console.WriteLine(ex.Message);
                            Console.ResetColor();
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Fatal error encountered:");
                Console.WriteLine(ex.LoaderExceptions[0].Message);
                Console.ResetColor();
            } 
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Fatal error encountered:");
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            } 
        }

        static void DisplayUsage()
        {
            Console.WriteLine("Babylon Import usage: BabylonImport.exe /i:\"source file\" /o:\"output folder\" [/sk]");
        }
    }
}
