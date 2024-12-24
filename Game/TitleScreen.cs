using System;
using Gum.DataTypes;
using Gum.Wireframe;
using GumRuntime;
using MonoGameGum.Forms;
using MonoGameGum.Forms.Controls;
using RenderingLibrary;
using Tetris;

public class TitleScreen
{
    public GraphicalUiElement Root;
    private OptionsScreen OptionsScreen;
    private Button NewGameButton, OptionsButton, QuitButton, LanguageButton;



    public TitleScreen(GumProjectSave gumProject, Action onStartNewGame, Action onExit)
    {
        Root = gumProject.Screens.Find(item => item.Name == "Title").ToGraphicalUiElement(SystemManagers.Default, addToManagers: false);
        NewGameButton = Root.GetFrameworkElementByName<Button>("NewGameButton");
        OptionsButton = Root.GetFrameworkElementByName<Button>("OptionsButton");
        QuitButton = Root.GetFrameworkElementByName<Button>("QuitButton");
        LanguageButton = Root.GetFrameworkElementByName<Button>("LanguageButton");

        NewGameButton.Click += (_, _) => { onStartNewGame(); };
        OptionsButton.Click += (_, _) => { OptionsScreen.Show(); };
        QuitButton.Click += (_, _) => { onExit(); };
        LanguageButton.Click += (_, _) =>
        {
            if (Localization.CurrentLanguage == "en-US")
            {
                Localization.CurrentLanguage = "id-ID";
            }
            else
            {
                Localization.CurrentLanguage = "en-US";
            }
        };

        Localization.RegisterLanguageChangeCallback(strings =>
        {
            NewGameButton.Text = strings.TitleNewGameButton;
            OptionsButton.Text = strings.TitleOptionsButton;
            QuitButton.Text = strings.TitleQuitButton;
            LanguageButton.Text = strings.LanguageName;
        });

        OptionsScreen = new OptionsScreen(
            gumProject: gumProject,
            onSetMultiboardMode: val =>
            {
                Config.ShowSecondBoard = val;
            },
            onSetInitialSpeed: val =>
            {
                Config.TetronimoDefaultBaseVerticalSpeedBlocksPerSecond = val;
            },
            onBack: Show
        );
    }

    public void Show()
    {
        State.GumRoot = Root;
    }
}