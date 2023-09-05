using AppPousadaPeNaTerra.Classes.API.Auditoria;
using AppPousadaPeNaTerra.Classes.API.Principal;
using AppPousadaPeNaTerra.Classes.Globais;
using AppPousadaPeNaTerra.Services.Principal;
using Newtonsoft.Json;
using System.Text;

namespace AppPousadaPeNaTerra.Services.Auditoria
{
    public class APIEstoqueAud
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
        private readonly HttpClient _httpClient;

        public APIEstoqueAud()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> CriaContagemFull(List<EstoqueClass> lista)
        {
            try
            {
                string uri = InfoGlobal.apiEstoque + "/Estoque/criar-contagem";

                using (var cliente = new HttpClient())
                {
                    var contagem = new EstoqueClass();
                    contagem.IdCategoria = lista[0].IdCategoria;
                    contagem.IdLocal = lista[0].IdLocal;
                    contagem.IdGrupo = lista[0].IdGrupo;
                    contagem.DataAbre = lista[0].DataAbre;
                    contagem.UserAbre = lista[0].UserAbre;
                    contagem.UserFecha = lista[0].UserFecha;
                    contagem.DataFecha = lista[0].DataFecha;
                    contagem.IdLista = lista[0].IdLista;
                    contagem.Finalizado = lista[0].Finalizado;
                    contagem.IdCategoriaLista = lista[0].IdCategoriaLista;

                    string json = JsonConvert.SerializeObject(contagem);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var resposta = await cliente.PostAsync(uri, content);

                    if (resposta.IsSuccessStatusCode) { return true; } else { return false; }
                }
            }

            catch (Exception ex)
            {
                await MetodoErroLog(ex);
                return false;
            }
        }

        public async Task<bool> CriaContagemFast(List<EstoquePreClass> lista)
        {
            try
            {
                string uri = InfoGlobal.apiEstoque + "/Estoque/criar-contagem-fast";


                using (var cliente = new HttpClient())
                {
                    List<EstoquePreClass> contagem = new List<EstoquePreClass>();

                    foreach (var item in lista)
                    {
                        contagem.Add(new EstoquePreClass
                        {
                            Iditem = item.Iditem,
                            Idgrupo = item.Idgrupo,
                            Idcategoria = item.Idcategoria,
                            Idsubgrupo = item.Idsubgrupo,
                            Idlocal = item.Idlocal,
                            Usuario = item.Usuario,
                            Quantidade = item.Quantidade,
                            Datasave = item.Datasave,
                            Sku = item.Sku,
                            Idlista = item.Idlista,
                            Finaliza = item.Finaliza
                        });
                    }

                    string json = JsonConvert.SerializeObject(contagem);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var resposta = await cliente.PostAsync(uri, content);

                    if (resposta.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            catch (Exception ex)
            {
                await MetodoErroLog(ex);
                return false;
            }
        }

        public async Task<List<EstoqueClass>> ListaContagens()
        {
            try
            {
                string uri = InfoGlobal.apiEstoque + "/Estoque/lista-contagens";

                using (var cliente = new HttpClient())
                {
                    var resposta = await cliente.GetStringAsync(uri);
                    return JsonConvert.DeserializeObject<EstoqueClass[]>(resposta).ToList();
                }
            }
            catch (Exception ex)
            {
                await MetodoErroLog(ex);
                return null;
            }
        }

        public async Task<List<EstoqueClass>> ListaConstagensFast(string status)
        {
            try
            {
                string uri = InfoGlobal.apiEstoque + "/Estoque/lista-contagem-fast?status=" + status;

                using (var cliente = new HttpClient())
                {
                    var resposta = await cliente.GetStringAsync(uri);
                    var retorno = JsonConvert.DeserializeObject<EstoqueClass[]>(resposta).ToList();
                    List<EstoqueClass> cont = new List<EstoqueClass>();
                    cont = retorno.ToList();
                    return cont;
                }
            }
            catch (Exception ex)
            {
                await MetodoErroLog(ex);
                return null;
            }
        }

        public async Task<List<EstoqueClass>> ContagensAbertas()
        {
            try
            {
                string uri = InfoGlobal.apiEstoque + "/Estoque/lista-contagem-aberta";

                using (var cliente = new HttpClient())
                {
                    var resposta = await cliente.GetStringAsync(uri);
                    var retorno = JsonConvert.DeserializeObject<EstoqueClass[]>(resposta).ToList();
                    List<EstoqueClass> cont = new List<EstoqueClass>();
                    cont = retorno.ToList();
                    return cont;
                }
            }
            catch (Exception ex)
            {
                await MetodoErroLog(ex);
                return null;
            }
        }

        public async Task<List<EstoqueClass>> ContagensFechadas(DataContagem dataContagem)
        {
            try
            {
                string json = JsonConvert.SerializeObject(dataContagem);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                string uri = InfoGlobal.apiEstoque + "/Estoque/lista-contagem-fechada";
                HttpResponseMessage response = await _httpClient.PostAsync(uri, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<EstoqueClass>>(responseContent);
                }

                return null;
            }
            catch (Exception ex)
            {
                await MetodoErroLog(ex);
                return null;
            }
        }

        public async Task<List<EstoquePreClass>> ListaConstagensPorId(int id)
        {
            try
            {
                string uri = InfoGlobal.apiEstoque + "/Estoque/lista-contagem-fast-id?idCard=" + id;

                using (var cliente = new HttpClient())
                {
                    var resposta = await cliente.GetStringAsync(uri);
                    var retorno = JsonConvert.DeserializeObject<EstoquePreClass[]>(resposta).ToList();
                    List<EstoquePreClass> cont = new List<EstoquePreClass>();
                    cont = retorno.ToList();
                    return cont;
                }
            }
            catch (Exception ex)
            {
                await MetodoErroLog(ex);
                return null;
            }
        }

        public async Task<List<EstoquePreClass>> ContagemFastId(int id)
        {
            try
            {
                string uri = InfoGlobal.apiEstoque + "/Estoque/lista-contagem-fast-id?idCard=" + id.ToString();

                using (var cliente = new HttpClient())
                {
                    var resposta = await cliente.GetStringAsync(uri);
                    var retorno = JsonConvert.DeserializeObject<EstoquePreClass[]>(resposta).ToList();
                    List<EstoquePreClass> cont = new List<EstoquePreClass>();
                    cont = retorno.ToList();
                    return cont;
                }
            }
            catch (Exception ex)
            {
                await MetodoErroLog(ex);
                return null;
            }
        }

        public async Task<int> UltimoIdLista()
        {
            try
            {
                string uri = InfoGlobal.apiEstoque + "/Estoque/id-ultima-lista";

                using (var cliente = new HttpClient())
                {
                    var resposta = await cliente.GetStringAsync(uri);

                    return int.Parse(resposta);
                }
            }
            catch (Exception ex)
            {
                await MetodoErroLog(ex);
                return 0;
            }
        }

        public async Task<bool> AttContagem(int id, string idlocal)
        {
            try
            {
                string uri = InfoGlobal.apiEstoque + "/Estoque/att-contagem?id=" + id + "&local=" + idlocal + "&user=" + InfoGlobal.usuario.ToUpper();

                using (var cliente = new HttpClient())
                {
                    var resposta = await cliente.PutAsync(uri, null);

                    if (resposta.IsSuccessStatusCode) { return true; } else { return false; }
                }
            }
            catch (Exception ex)
            {
                await MetodoErroLog(ex);
                return false;
            }
        }

        public async Task<bool> ExcluiLista(int id)
        {
            try
            {
                string uri = InfoGlobal.apiEstoque + "/Estoque/deleta-contagem?id=" + id + "&user=" + InfoGlobal.usuario.ToUpper();

                using (var cliente = new HttpClient())
                {
                    var resposta = await cliente.PutAsync(uri, null);

                    if (resposta.IsSuccessStatusCode) { return true; } else { return false; }
                }
            }
            catch (Exception ex)
            {
                await MetodoErroLog(ex);
                return false;
            }
        }

        public async Task<List<SelectEstoqueAtual>> HistoricoSelect(int idlista)
        {
            try
            {
                string uri = InfoGlobal.apiEstoque + "/Estoque/historico-estoque-atual?idCard=" + idlista + "";

                using (var cliente = new HttpClient())
                {
                    var resposta = await cliente.GetStringAsync(uri);
                    var retorno = JsonConvert.DeserializeObject<SelectEstoqueAtual[]>(resposta).ToList();
                    return retorno.ToList();
                }
            }
            catch (Exception ex)
            {
                await MetodoErroLog(ex);
                return null;
            }
        }

        public async Task<bool> AttHistorico(List<SelectEstoqueAtual> lista)
        {
            try
            {
                string uri = InfoGlobal.apiEstoque + "/Estoque/att-historico";

                using (var cliente = new HttpClient())
                {
                    List<SelectEstoqueAtual> estoque = new List<SelectEstoqueAtual>();

                    foreach (var item in lista)
                    {
                        estoque.Add(new SelectEstoqueAtual
                        {
                            EstoqueAtual = item.EstoqueAtual,
                            IdItem = item.IdItem,
                            IdLista = item.IdLista,
                            Sku = item.Sku
                        });
                    }

                    string json = JsonConvert.SerializeObject(estoque);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var resposta = await cliente.PostAsync(uri, content);

                    if (resposta.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
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
