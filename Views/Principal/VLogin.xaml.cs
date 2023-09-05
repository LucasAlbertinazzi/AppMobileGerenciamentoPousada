using AppPousadaPeNaTerra.Classes.API.Principal;
using AppPousadaPeNaTerra.Classes.Globais;
using AppPousadaPeNaTerra.Services.Principal;
using System.Diagnostics;

namespace AppPousadaPeNaTerra.Views;

public partial class VLogin : ContentPage
{
    #region 1- VARIAVEIS
    APIUser aPIUser = new APIUser();
    APIVersaoApp versaoApp = new APIVersaoApp();
    APIErroLog error = new();
    #endregion

    #region 2 - METODOS CONSTRUTORES
    public VLogin()
    {
        InitializeComponent();
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        try
        {
            await Inicializa();
            MetodosIniciais();
        }
        catch (Exception ex)
        {
            await MetodoErroLog(ex);
            return;
        }
    }
    #endregion

    #region 3- METODOS
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

    private void MetodosIniciais()
    {
        try
        {
            //Redimensionamento de logo
            logoSize.HeightRequest = GetIconSizeForDevice();

            //Exibe label com versão do APP
            ExibeVersao();
        }
        catch (Exception ex)
        {
            MetodoErroLog(ex);
            return;
        }

    }

    private async Task AuthenticateSavedCredentials()
    {
        try
        {
            btnEntrar.IsVisible = false;
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var savedUsername = await SecureStorage.GetAsync("Username");
            var savedPassword = await SecureStorage.GetAsync("Password");

            UsernameEntry.Text = savedUsername;
            PasswordEntry.Text = savedPassword;

            var user = new Login
            {
                usuario = savedUsername,
                senha = savedPassword
            };

            if (await aPIUser.ValidaUser(user))
            {
                // Definir a nova página principal após o login
                await Application.Current.MainPage.Navigation.PushAsync(new VMenuPrincipal());
            }

            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            btnEntrar.IsVisible = true;
        }
        catch (Exception ex)
        {
            await MetodoErroLog(ex);
            return;
        }
    }

    private bool CheckSavedCredentials()
    {
        try
        {
            return !string.IsNullOrEmpty(SecureStorage.GetAsync("Username").Result) &&
               !string.IsNullOrEmpty(SecureStorage.GetAsync("Password").Result);
        }
        catch (Exception ex)
        {
            MetodoErroLog(ex);
            return false;
        }

    }

    private double GetIconSizeForDevice()
    {
        try
        {
            double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;

            // Ajuste o valor de acordo com suas preferências
            double iconSize = screenWidth / 2;

            return iconSize;
        }
        catch (Exception ex)
        {
            MetodoErroLog(ex);
            return 0;
        }

    }

    private void ExibeVersao()
    {
        try
        {
            lblInfoDev.Text = "Pousada Pé na Terra @Todos os direitos reservados";
            lblInfoDevVersao.Text = $"Versão {AppInfo.Version}";
        }
        catch (Exception ex)
        {
            MetodoErroLog(ex);
            return;
        }

    }

    private async Task Inicializa()
    {
        try
        {
            // Desabilita o menu lateral
            App.Current.MainPage.SetValue(Shell.FlyoutBehaviorProperty, FlyoutBehavior.Disabled);
            ShowPasswordButton.Source = "eyeclose.svg";

            await VerificaVersao();

            // Verifica se as credenciais estão salvas
            if (CheckSavedCredentials())
            {
                // Autentica automaticamente
                await AuthenticateSavedCredentials();
            }
        }
        catch (Exception ex)
        {
            await MetodoErroLog(ex);
            return;
        }
    }

    private async Task VerificaVersao()
    {
        try
        {
            if (!await versaoApp.VerificarVersaoInstalada())
            {
                if (Debugger.IsAttached)
                {
                    await versaoApp.SalvaVersao();
                }
                else
                {
                    // Exibir mensagem de atualização
                    await Application.Current.MainPage.DisplayAlert("Atualização Disponível", "Uma nova versão do aplicativo está disponível. Por favor, atualize para continuar.", "OK");

                    // Abrir a URL de atualização
                    await Launcher.OpenAsync("http://192.168.85.3:25434/");
                }
            }
        }
        catch (Exception ex)
        {
            await MetodoErroLog(ex);
            return;
        }

    }
    #endregion

    #region 4- EVENTOS DE CONTROLE
    private void OnShowPasswordButtonClicked(object sender, EventArgs e)
    {
        try
        {
            if (PasswordEntry.IsPassword)
            {
                PasswordEntry.IsPassword = false;
                ShowPasswordButton.Source = "eyeopen.svg";
            }
            else
            {
                PasswordEntry.IsPassword = true;
                ShowPasswordButton.Source = "eyeclose.svg";
            }
        }
        catch (Exception ex)
        {
            MetodoErroLog(ex);
            return;

        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            btnEntrar.IsVisible = false;
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            var user = new Login
            {
                usuario = UsernameEntry.Text,
                senha = PasswordEntry.Text
            };

            if (CredentialsSwitch.IsToggled)
            {
                // Limpar usuário e senha armazenados
                SecureStorage.Remove("Username");
                SecureStorage.Remove("Password");
                SecureStorage.RemoveAll();

                InfoGlobal.ClearData();

                if (await aPIUser.ValidaUser(user))
                {
                    // Definir a nova página principal após o login
                    await Application.Current.MainPage.Navigation.PushAsync(new VMenuPrincipal());
                }
                else
                {
                    await DisplayAlert("Erro", "Credenciais inválidas", "OK");
                }
            }
            else
            {
                if (await aPIUser.ValidaUser(user))
                {
                    // Armazenar usuário e senha
                    await SecureStorage.SetAsync("Username", user.usuario);
                    await SecureStorage.SetAsync("Password", user.senha);

                    // Definir a nova página principal após o login
                    await Application.Current.MainPage.Navigation.PushAsync(new VMenuPrincipal());
                }
                else
                {
                    await DisplayAlert("Erro", "Credenciais inválidas", "OK");
                }
            }

            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            btnEntrar.IsVisible = true;
        }
        catch (Exception ex)
        {
            await MetodoErroLog(ex);
            return;
        }
    }
    #endregion
}