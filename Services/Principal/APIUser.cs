using AppPousadaPeNaTerra.Classes.API.Principal;
using AppPousadaPeNaTerra.Classes.Globais;
using Newtonsoft.Json;
using System.Text;

namespace AppPousadaPeNaTerra.Services.Principal
{
    public class APIUser
    {
        #region 1- LOG
        APIErroLog error = new();

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

        #region 2- API
        private HttpClient _httpClient;

        public APIUser()
        {
            _httpClient = new HttpClient() { Timeout = new TimeSpan(0, 0, 2) };
        }

        public async Task<bool> ValidaUser(Login login)
        {
            try
            {
                string json = JsonConvert.SerializeObject(login);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                string uri = InfoGlobal.apiApp + "/Usuario/verifica-login";
                HttpResponseMessage response = await _httpClient.PostAsync(uri, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    UsuarioClass resposta = JsonConvert.DeserializeObject<UsuarioClass>(responseContent);

                    InfoGlobal.statusCode = true;
                    InfoGlobal.usuario = resposta.Nome;
                    InfoGlobal.funcao = resposta.IdFuncao.ToString();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                await MetodoErroLog(ex);
                return false;
            }
        }
        #endregion
    }
}
