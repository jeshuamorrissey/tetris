using System;
using Gum.DataTypes;
using Gum.Wireframe;
using GumRuntime;
using Microsoft.Xna.Framework;
using MonoGameGum.Forms;
using MonoGameGum.Forms.Controls;
using MonoGameGum.GueDeriving;
using RenderingLibrary;
using Tetris;


public class OptionsScreen
{
    private GraphicalUiElement Root;
    
    CheckBox EnableSecondBoardCheckbox;
    Slider ChangeBaseSpeedSlider;
    TextRuntime ChangeBaseSpeedSliderText;
    Button BackButton;

    public OptionsScreen(GumProjectSave gumProject, Action<bool> onSetMultiboardMode, Action<double> onSetInitialSpeed, Action onBack)
    {
        Root = gumProject.Screens.Find(item => item.Name == "Options").ToGraphicalUiElement(SystemManagers.Default, addToManagers: false);
        EnableSecondBoardCheckbox = Root.GetFrameworkElementByName<CheckBox>("EnableSecondBoardCheckbox");
        ChangeBaseSpeedSlider = Root.GetFrameworkElementByName<Slider>("ChangeBaseSpeedSlider");
        ChangeBaseSpeedSliderText = (TextRuntime)Root.GetGraphicalUiElementByName("BaseSpeedSliderText");
        BackButton = Root.GetFrameworkElementByName<Button>("BackButton");
        
        BackButton.Click += (_, _) => { onBack(); };
        EnableSecondBoardCheckbox.Checked += (_, _) => { onSetMultiboardMode(true); };
        EnableSecondBoardCheckbox.Unchecked += (_, _) => { onSetMultiboardMode(false); };
        ChangeBaseSpeedSlider.ValueChanged += (_, _) => { 
            ChangeBaseSpeedSliderText.Text = string.Format(Localization.Strings.OptionsChangeBaseSpeedSlider, ChangeBaseSpeedSlider.Value);
            onSetInitialSpeed(ChangeBaseSpeedSlider.Value);
        };

        ChangeBaseSpeedSlider.Minimum = 1;
        ChangeBaseSpeedSlider.Maximum = 10;
        ChangeBaseSpeedSlider.IsSnapToTickEnabled = true;
        ChangeBaseSpeedSlider.TicksFrequency = 1;

        Localization.RegisterLanguageChangeCallback(strings =>
        {
            EnableSecondBoardCheckbox.Text = strings.OptionsEnableSecondBoardCheckbox;
            ChangeBaseSpeedSliderText.Text = string.Format(strings.OptionsChangeBaseSpeedSlider, ChangeBaseSpeedSlider.Value);
            BackButton.Text = strings.OptionsBackButton;
        });
    }

    public void Show()
    {
        State.GumRoot = Root;
    }
}