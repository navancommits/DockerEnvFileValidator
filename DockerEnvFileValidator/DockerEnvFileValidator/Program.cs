using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DockerEnvFileValidator
{
    class Program
    {
        static string _dockerPath;
        static List<string> EnvList = new List<string>();
        static List<string> EnvVarsinEnvFile = new List<string>();
        static IEnumerable<string> MissingEnvVars = new List<string>();

        static void Main(string[] args)
        {
            _dockerPath = args[0];

            //_dockerPath = @"C:\Projects\Helix.Examples\examples\helix-basic-tds-consolidated";
            //_dockerPath = @"C:\Projects\Helix.Examples\examples\helix-basic-aspnetcore";

            if (string.IsNullOrWhiteSpace(_dockerPath))
            {
                Console.WriteLine("Docker Folder Path with .env file Required!");
                return;
            }

            if (!Directory.GetFiles(_dockerPath, ".env").Any())
            {
                Console.WriteLine(".env file missing, creating in the base directory!");

                File.Create(_dockerPath + ".env");
            }

            var usedEnvVars=SearchDirectoryforEnvVars(_dockerPath);

            if (usedEnvVars.Any())
            {
                Console.WriteLine();
                Console.WriteLine("Here are the env vars used in the docker compose/override files:");

                foreach (var envvar in usedEnvVars)
                {
                    Console.WriteLine(envvar);
                }
            }

            var preExistingEnvVars=GetEnvVarsfromExistingEnvFile(_dockerPath + "\\.env");

            MissingEnvVars = usedEnvVars.Except(preExistingEnvVars);
            if (MissingEnvVars.Any())
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Here are the missing vars in the .env file:");

                foreach (var missingvar in MissingEnvVars)
                {
                    Console.WriteLine(missingvar);
                }

                UpdateEnvFile(_dockerPath + "\\.env");
            }
            else
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("All good! The .env file has all the required env variables!!!!");
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Done!");

            Console.ReadLine();
        }

        private static void UpdateEnvFile(string filePath)
        {
            var concatLines = Environment.NewLine;

            using (var writer = File.AppendText(filePath))
            {
                foreach (var envar in MissingEnvVars)
                {
                    concatLines += Environment.NewLine + envar + "=";
                }

                writer.WriteLine(concatLines);
            }

            Console.WriteLine();
            Console.ForegroundColor=ConsoleColor.Green;
            Console.WriteLine("Updated Env File successfully - " + filePath);
        }


        public static char[] GetChars(string s)
        {
            char[] charList = s.ToArray();

            return charList;
        }

        private static bool StartBrace(char letter, int letterIndex)
        {
            if (letter != '{') return false;

            return true;
        }

        private static bool EndBrace(char letter, int letterIndex)
        {
            if (letter != '}') return false;

            return true;
        }

        private static List<string> SearchDirectoryforEnvVars(string dockerPath)
        {
            List<string> envVarList = new List<string>();

            var fileList = Directory.GetFiles(dockerPath, "*.yml").Where(f => f.Contains("docker-compose"));

            if (!fileList.Any())
            {
                Console.WriteLine("No Files in the Path!");
                return null;
            }

            foreach (var filePath in fileList)
            {
                envVarList = SearchEnvVarinFile(filePath);

                if (envVarList == null || !envVarList.Any())
                {
                    Console.WriteLine("No Env variables detected!");
                    return null;
                }
            }

            return envVarList;
        }

        private static List<string> GetEnvVarsfromExistingEnvFile(string filePath)
        {
            string envVarName = string.Empty;

            using (var input = File.OpenText(filePath))
            {
                string currline;
                while (null != (currline = input.ReadLine()))
                {
                    envVarName= string.Empty;

                    if (currline.Contains("=")) envVarName = currline.Split('=')[0];

                    if (!EnvVarsinEnvFile.Contains(envVarName) && !string.IsNullOrWhiteSpace(envVarName)) EnvVarsinEnvFile.Add(envVarName);
                }

            }

            Console.WriteLine();
            Console.WriteLine("Finished Searching Existing Env File: " + filePath);

            return EnvVarsinEnvFile;
        }


        private static List<string> SearchEnvVarinFile(string filePath)
        {
            int startBraceIndex = -1;
            int endBraceIndex = -1;
            string envVarName = string.Empty;

            using (var input = File.OpenText(filePath))
            {
                string currline;
                while (null != (currline = input.ReadLine()))
                {
                    var charList= GetChars(currline);

                    for (int i = 0; i < charList.Length; i++)
                    {
                        if (StartBrace(charList[i], i)) { startBraceIndex = i; }
                        if(startBraceIndex > -1) if (EndBrace(charList[i], i)) { endBraceIndex = i; }

                        if (startBraceIndex > -1 && endBraceIndex > -1)
                        {
                            envVarName = currline.Substring(startBraceIndex+1, endBraceIndex - startBraceIndex-1);

                            if (envVarName.Contains(":")) envVarName = envVarName.Split(':')[0];

                            if (!EnvList.Contains(envVarName)) EnvList.Add(envVarName);

                            startBraceIndex = -1;
                            endBraceIndex = -1;
                            envVarName = string.Empty;
                        }
                    }
                }

            }

            Console.WriteLine("Finished Searching File: " + filePath);
            return EnvList;
        }

    }
}
