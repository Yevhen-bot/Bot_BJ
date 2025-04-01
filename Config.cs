using System;
using System.IO;
using Microsoft.Extensions.Configuration;

class Config
{
    public static string GetBotToken()
    {
        var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();

        return config["BotToken"];
    }
}