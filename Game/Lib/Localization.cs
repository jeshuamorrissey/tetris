using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Content.ContentReaders;

namespace Tetris;

public class Strings
{
    public string LanguageName { get; set; }
    public string TitleNewGameButton { get; set; }
    public string TitleOptionsButton { get; set; }
    public string TitleQuitButton { get; set; }
    public string OptionsEnableSecondBoardCheckbox { get; set; }
    public string OptionsChangeBaseSpeedSlider { get; set; }
    public string OptionsBackButton { get; set; }
    public string GameScore { get; set; }
    public string GameSpeed { get; set; }
    public string GameYouLose { get; set; }
}

public class StringsReader : JsonContentTypeReader<Strings> { }

public class Localization
{
    private static string _currentLanguage = "en-US";
    public static string CurrentLanguage
    {
        get { return _currentLanguage; }
        set
        {
            _currentLanguage = value;
            foreach (var callback in LanguageChangeCallbacks)
            {
                callback(Strings);
            }
        }
    }

    private static Dictionary<string, Strings> StringsByLanguageCode = [];
    private static List<Action<Strings>> LanguageChangeCallbacks = [];

    public static void Initialize(ContentManager content)
    {
        var localizationFiles = Directory.GetFiles(Path.Join([content.RootDirectory, "strings"]));
        foreach (var localizationFile in localizationFiles)
        {
            var languageCode = Path.GetFileNameWithoutExtension(localizationFile);
            StringsByLanguageCode[languageCode] = content.Load<Strings>($"strings/{languageCode}");
        }
    }

    public static Strings Strings { get { return StringsByLanguageCode[CurrentLanguage]; } }

    public static void RegisterLanguageChangeCallback(Action<Strings> onLanguageChange)
    {
        LanguageChangeCallbacks.Add(onLanguageChange);
        onLanguageChange(Strings);
    }
}