﻿using AppPousadaPeNaTerra.Classes.API.Principal;
using AppPousadaPeNaTerra.Classes.Globais;
using AppPousadaPeNaTerra.Services.Principal;
using AppPousadaPeNaTerra.Views;

namespace AppPousadaPeNaTerra;

public partial class AppShell : Shell
{
    #region 1- VARIAVEIS
    APIErroLog error = new();
    #endregion

    #region 2 - METODOS CONSTRUTORES
    public AppShell()
    {
        InitializeComponent();
    }

    private void Shell_Loaded(object sender, EventArgs e)
    {
        try
        {
            double dist = DefineEspaco();
            btnSettings.Margin = new Thickness(0, dist, 0, 0);
        }
        catch (Exception ex)
        {
            MetodoErroLog(ex);
            return;
        }
    }

    #endregion

    #region 3- METODOS
    private double DefineEspaco()
    {
        try
        {
            double screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
            double espaconHeight = screenHeight / 1.5;

            return espaconHeight;
        }
        catch (Exception ex)
        {
            MetodoErroLog(ex);
            return 0;
        }
    }

    private async Task MetodoErroLog(Exception ex)
    {
        var erroLog = new ErrorLogClass
        {
            Erro = ex.Message, // Obtém a mensagem de erro
            Metodo = ex.TargetSite.Name, // Obtém o nome do método que gerou o erro
            Dispositivo = DeviceInfo.Model, // Obtém o nome do dispositivo em execução
            Versao = DeviceInfo.Version.ToString(), // Obtém a versão do dispostivo
            Plataforma = DeviceInfo.Platform.ToString(), // Obtém o sistema operacional do dispostivo
            TelaClasse = GetType().FullName, // Obtém o nome da tela/classe
            Data = DateTime.Now,
        };

        await error.LogErro(erroLog);
    }

    #endregion

    #region 4- EVENTOS DE CONTROLE
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        try
        {
            // Limpar usuário e senha armazenados
            SecureStorage.Remove("Username");
            SecureStorage.Remove("Password");
            SecureStorage.RemoveAll();

            await Application.Current.MainPage.Navigation.PushAsync(new VLogin());
        }
        catch (Exception ex)
        {
            await MetodoErroLog(ex);
            return;
        }
    }

    private async void OnSettingsCliked(object sender, EventArgs e)
    {
        try
        {

        }
        catch (Exception ex)
        {
            await MetodoErroLog(ex);
            return;
        }
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        try
        {
            if (InfoGlobal.isMenuOpen)
            {
                await Application.Current.MainPage.Navigation.PushAsync(new VMenuPrincipal());
            }
            else
            {
                Shell.Current.FlyoutIsPresented = false;
            }
        }
        catch (Exception ex)
        {
            await MetodoErroLog(ex);
            return;
        }
    }
    #endregion
}
