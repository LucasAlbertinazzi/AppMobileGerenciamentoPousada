using AppPousadaPeNaTerra.Classes.API.Auditoria;
using AppPousadaPeNaTerra.Classes.API.Principal;
using AppPousadaPeNaTerra.Classes.Globais;
using AppPousadaPeNaTerra.Services.Auditoria;
using AppPousadaPeNaTerra.Services.Principal;
using AppPousadaPeNaTerra.Suporte;
using AppPousadaPeNaTerra.Views.Principal;
using System.Globalization;
using System.Text;

namespace AppPousadaPeNaTerra.Views;

public partial class VNewContagem : ContentPage
{
    #region 1- Variaveis
    APILocalAud apiLocal = new APILocalAud();
    APIEstoqueAud apiEstoque = new APIEstoqueAud();
    APIItensAud apiItens = new APIItensAud();
    APIEnviaArquivos aPIEnviaArquivos = new APIEnviaArquivos();

    APIErroLog error = new();

    List<LocalClass> lista_local = new List<LocalClass>();
    List<ItensClass> lista_itens = new List<ItensClass>();
    List<ItensClass> card_itens = new List<ItensClass>();

    ImagensCache imagensCacheSup = new ImagensCache();

    private bool useglobal = false;
    private int index = 0;
    private int _idLista;
    private string local;
    private string localFinaliza = string.Empty;

    private bool camera = false;
    #endregion

    #region 2- Construtores
    public VNewContagem()
    {
        InitializeComponent();
        Iniciais();
    }

    public VNewContagem(int idlista, int _i, string _local)
    {
        InitializeComponent();

        try
        {
            Iniciais();
            index = _i;
            _idLista = idlista;
            local = _local;
            useglobal = true;
        }
        catch (Exception ex)
        {
            MetodoErroLog(ex);
            return;
        }
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        try
        {
            stackPrincipal.IsVisible = false;
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            await MetodosIniciais();

            stackPrincipal.IsVisible = true;
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
        catch (Exception ex)
        {
            await MetodoErroLog(ex);
            return;
        }
    }
    #endregion

    #region 3- Metodos
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

    private async Task MetodosIniciais()
    {
        try
        {
            imagensCacheSup.ApagaCacheImagens();

            if (index == 0)
            {
                btnSalvar.IsEnabled = true;
                btnSalvar.IsVisible = true;
                btnFinalizar.IsVisible = true;
                camera = true;

                await CarregaItens();
                await CarregaLocal();
                await IdContagem();
            }
            else if (index == 2)
            {
                btnSalvar.IsVisible = false;
                btnFinalizar.IsVisible = false;
                _listaLocal.IsEnabled = false;
                sbItens.IsReadOnly = true;
                frItens.IsEnabled = false;
                _listaItem.IsEnabled = false;
                camera = false;

                await CarregaListId(_idLista);
                _listaLocal.Title = localFinaliza;
            }
            else
            {
                btnSalvar.IsEnabled = false;
                btnSalvar.IsVisible = false;
                camera = true;

                await CarregaItens();
                await CarregaLocal();
                await CarregaListId(_idLista);
            }
        }
        catch (Exception ex)
        {
            await MetodoErroLog(ex);
            return;
        }
    }

    private void Iniciais()
    {
        try
        {
            _listaCard.HeightRequest = ResponsiveAuto.Height(1.9);
            _listaItem.MaximumHeightRequest = ResponsiveAuto.Height(3.8);
            BindingContext = this;
            App.Current.MainPage.SetValue(Shell.FlyoutBehaviorProperty, FlyoutBehavior.Flyout);
            NavigationPage.SetHasNavigationBar(this, false);
            InfoGlobal.isMenuOpen = true;
        }
        catch (Exception ex)
        {
            MetodoErroLog(ex);
            return;
        }
    }

    private async Task CarregaLocal()
    {
        try
        {
            List<LocalClass> lista_locais = await apiLocal.Locais();
            lista_local = new List<LocalClass>();

            foreach (var item in lista_locais)
            {
                if (item.Fisico == true && item.IdLocal != "ALL")
                {
                    lista_local.Add(new LocalClass { Local = item.Local, IdLocal = item.IdLocal });
                }
            }

            _listaLocal.ItemsSource = lista_local.OrderBy(x => x.Local).ToList();

            List<LocalClass> listaIndex = lista_local.OrderBy(x => x.Local).ToList();

            if (!string.IsNullOrEmpty(local))
            {
                _listaLocal.SelectedIndex = VerificaIndexLocal(local, listaIndex);
            }
        }
        catch (Exception ex)
        {
            await MetodoErroLog(ex);
            return;
        }
        
    }

    private int VerificaIndexLocal(string local, List<LocalClass> listaIndex)
    {
        try
        {
            string l = local.Replace("Local: \r\r", "");

            // Encontrar o índice do elemento que contém o valor "l" na propriedade "Local"
            int index = listaIndex.FindIndex(x => x.Local == l);

            return index;
        }
        catch (Exception ex)
        {
            MetodoErroLog(ex);
            return 0;
        }
    }

    private async Task IdContagem()
    {
        try
        {
            _idLista = await apiEstoque.UltimoIdLista();

            if (_idLista >= 0)
            {
                _idLista++;
            }

            else
            {
                _idLista = 0;
            }

            lastId.Text = "N° da contagem: " + _idLista.ToString();
        }
        catch (Exception ex)
        {
            await MetodoErroLog(ex);
            return;
        }
    }

    private async Task CarregaListId(int id)
    {
        try
        {
            List<EstoquePreClass> lista = await apiEstoque.ContagemFastId(id);

            var local = await apiLocal.Local(lista[0].Idlocal);

            localFinaliza = local[0].Local;

            foreach (var item in lista)
            {
                var itens = await apiItens.ListaItensId((int)item.Iditem);

                card_itens.Add(new ItensClass
                {
                    Descricao = itens[0].Descricao,
                    Sku = item.Sku,
                    IdItem = (int)item.Iditem,
                    QuantidadeCont = item.Quantidade,
                    IdGrupo = item.Idgrupo,
                    IdCategoria = (int)item.Idcategoria,
                    IdSubgrupo = item.Idsubgrupo
                });
            }

            _listaCard.ItemsSource = card_itens.ToList();
            lastId.Text = "N° da contagem: " + id.ToString();
        }
        catch (Exception ex)
        {
            await MetodoErroLog(ex);
            return;
        }
    }

    private async Task CarregaItens()
    {
        try
        {
            List<ItensClass> itens = await apiItens.ListaItens();
            lista_itens = new List<ItensClass>();

            foreach (var item in itens)
            {
                lista_itens.Add(new ItensClass
                {
                    Descricao = item.Descricao.ToUpper(),
                    IdItem = item.IdItem,
                    Sku = item.Sku,
                    Ativo = item.Ativo,
                    AtualizadoEm = item.AtualizadoEm,
                    AtualizadoPor = item.AtualizadoPor,
                    BarPiscina = item.BarPiscina,
                    CadastradoEm = item.CadastradoEm,
                    CadastradoPor = item.CadastradoPor,
                    Codbarra = item.Codbarra,
                    CodItemCb = item.CodItemCb,
                    ComplementoRefeicao = item.ComplementoRefeicao,
                    Cozinha = item.Cozinha,
                    DescricaoCozinha = item.DescricaoCozinha,
                    Drink = item.Drink,
                    Ean = item.Ean,
                    IdCategoria = item.IdCategoria,
                    IdFt = item.IdFt,
                    IdGrupo = item.IdGrupo,
                    IdReceita = item.IdReceita,
                    IdSubgrupo = item.IdSubgrupo,
                    Peso = item.Peso,
                    SkuCb = item.SkuCb,
                    Unidade = item.Unidade,
                    Volume = item.Volume
                });
            }

            _listaItem.ItemsSource = lista_itens;
        }
        catch (Exception ex)
        {
            await MetodoErroLog(ex);
            return;
        }
    }

    private void CriaCardItem(ItensClass lista)
    {
        try
        {
            if (card_itens != null && card_itens.Count > 0)
            {
                foreach (var item in card_itens)
                {
                    string x = item.QuantidadeCont;
                }
            }
            card_itens.Add(new ItensClass
            {
                Descricao = lista.Descricao,
                Sku = lista.Sku,
                IdItem = lista.IdItem,
                QuantidadeCont = "0",
                IdGrupo = lista.IdGrupo,
                IdCategoria = lista.IdCategoria,
                IdSubgrupo = lista.IdSubgrupo,
            });

            _listaCard.ItemsSource = card_itens.ToList();
        }
        catch (Exception ex)
        {
            MetodoErroLog(ex);
            return;
        }
    }

    private async void Aviso()
    {
        try
        {
            LocalClass lista = (LocalClass)_listaLocal.SelectedItem;

            if (index == 0)
            {
                if (card_itens.Count > 0 && lista != null)
                {
                    if (await DisplayAlert("AVISO", "Deseja salvar a contagem?", "Sim", "Não"))
                    {
                        btnSalvar_Clicked(null, null);
                    }
                }
            }

            await Application.Current.MainPage.Navigation.PushAsync(new VMenuPrincipal());
        }
        catch (Exception ex)
        {
            await MetodoErroLog(ex);
            return;
        }
    }

    public string RemoveDiacritics(string text)
    {
        try
        {
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString();
        }
        catch (Exception ex)
        {
            MetodoErroLog(ex);
            return null;
        }
    }

    #endregion

    #region 4- Eventos de controle
    private void Entry_Focused(object sender, FocusEventArgs e)
    {
        try
        {
            var entry = sender as Entry;
            if (entry != null)
            {
                if (entry.Text == "0")
                {
                    entry.Text = string.Empty;
                    entry.Focus();
                }
            }
        }
        catch (Exception ex)
        {
            MetodoErroLog(ex);
            return;
        }
    }

    private async void camera_Clicked(object sender, EventArgs e)
    {
        try
        {
            Button b = (Button)sender;

            if (b != null)
            {
                if (!string.IsNullOrWhiteSpace(b.Text))
                {
                    int aidItem = Convert.ToInt32(b.Text);

                    InfoGlobal.isMenuOpen = false;
                    await Application.Current.MainPage.Navigation.PushAsync(new VCamera(aidItem, _idLista, useglobal, camera));
                }
            }
        }
        catch (Exception ex)
        {
            await MetodoErroLog(ex);
            return;
        }
    }

    protected override bool OnBackButtonPressed()
    {
        Aviso();

        return true;
    }

    private void sbItens_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            _listaLocal.Unfocus();

            var normalizedSearchText = RemoveDiacritics(e.NewTextValue).ToLower();

            var filtroItens = lista_itens
                .Where(a => RemoveDiacritics(a.Descricao).ToLower().Contains(normalizedSearchText))
                .OrderBy(x => x.Descricao)
                .ToList();

            if (filtroItens.Count > 0)
            {
                if (sbItens.Text.Length > 3)
                {
                    frItens.IsVisible = true;
                    _listaItem.ItemsSource = filtroItens;
                }
                else if (sbItens.Text.Length < 2)
                {
                    frItens.IsVisible = false;
                    _listaItem.ItemsSource = lista_itens;
                }
            }
        }
        catch (Exception ex)
        {
            MetodoErroLog(ex);
            return;
        }
    }

    private async void btnSalvar_Clicked(object sender, EventArgs e)
    {
        try
        {
            LocalClass lista = (LocalClass)_listaLocal.SelectedItem;

            if (lista != null)
            {
                stackPrincipal.IsVisible = false;
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;

                if (!await aPIEnviaArquivos.SalvaImagens())
                {
                    await DisplayAlert("Erro", "Erro ao salvar imagens!", "Ok");

                    btnFinalizar.IsEnabled = true;
                    btnSalvar.IsEnabled = true;

                    stackPrincipal.IsVisible = true;
                    LoadingIndicator.IsVisible = false;
                    LoadingIndicator.IsRunning = false;

                    return;
                }

                btnFinalizar.IsEnabled = false;
                btnSalvar.IsEnabled = false;

                if (card_itens.Count > 0 && lista != null)
                {
                    List<EstoqueClass> listafinaliza = new List<EstoqueClass>();

                    string grupos = string.Empty;
                    string categoria = string.Empty;

                    for (int i = 0; i < card_itens.Count; i++)
                    {
                        grupos += card_itens[i].IdGrupo.ToString() + ",";
                        categoria += card_itens[i].IdCategoria.ToString() + ",";
                    }

                    grupos = grupos.TrimEnd(',');
                    categoria = categoria.TrimEnd(',');

                    foreach (var card in card_itens)
                    {
                        listafinaliza.Add(new EstoqueClass
                        {
                            IdLocal = lista.IdLocal,
                            IdGrupo = grupos,
                            DataAbre = DateTime.Now,
                            DataFecha = null,
                            UserAbre = InfoGlobal.usuario.ToUpper(),
                            UserFecha = null,
                            IdLista = _idLista,
                            Finalizado = null,
                            IdCategoriaLista = categoria
                        });
                    }

                    List<EstoquePreClass> save = new List<EstoquePreClass>();

                    foreach (var card in card_itens)
                    {
                        save.Add(new EstoquePreClass
                        {
                            Iditem = card.IdItem,
                            Idgrupo = card.IdGrupo,
                            Idcategoria = card.IdCategoria,
                            Idsubgrupo = card.IdSubgrupo,
                            Idlocal = lista.IdLocal,
                            Usuario = InfoGlobal.usuario.ToUpper(),
                            Quantidade = card.QuantidadeCont,
                            Datasave = DateTime.Now,
                            Sku = card.Sku,
                            Idlista = _idLista,
                            Finaliza = null
                        });
                    }

                    if (await apiEstoque.CriaContagemFull(listafinaliza) && await apiEstoque.CriaContagemFast(save))
                    {
                        List<SelectEstoqueAtual> nova = await apiEstoque.HistoricoSelect(save[0].Idlista);
                        if (await apiEstoque.AttHistorico(nova))
                        {
                            await DisplayAlert("Aviso", "Contagem salva com sucesso!", "Ok");

                            card_itens.Clear();
                            lista_local.Clear();

                            _listaCard.ItemsSource = null;

                            await CarregaItens();
                            await CarregaLocal();
                            await IdContagem();
                        }
                    }
                }

                btnFinalizar.IsEnabled = true;
                btnSalvar.IsEnabled = true;

                stackPrincipal.IsVisible = true;
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }

            else
            {
                await Application.Current.MainPage.DisplayAlert("Aviso", "Selecione um local para prosseguir!", "OK");
            }
        }
        catch (Exception ex)
        {
            await MetodoErroLog(ex);
            return;
        }
    }

    private async void btnFinalizar_Clicked(object sender, EventArgs e)
    {
        try
        {
            LocalClass lista = new LocalClass();
            var item = _listaLocal.SelectedItem;
            lista = (LocalClass)item;

            btnFinalizar.IsEnabled = false;
            btnSalvar.IsEnabled = false;

            if (card_itens.Count > 0 && lista != null)
            {
                if (await DisplayAlert("AVISO", "Deseja finalizar a contagem?", "Sim", "Não"))
                {
                    stackPrincipal.IsVisible = false;
                    LoadingIndicator.IsVisible = true;
                    LoadingIndicator.IsRunning = true;



                    if (!await aPIEnviaArquivos.SalvaImagens())
                    {
                        await DisplayAlert("Erro", "Erro ao salvar imagens!", "Ok");
                        return;
                    }

                    if (index == 1)
                    {
                        var listaatual = await apiEstoque.ListaConstagensPorId(_idLista);
                        List<EstoquePreClass> save = new List<EstoquePreClass>();

                        for (int i = 0; i < card_itens.Count; i++)
                        {
                            if (!listaatual.Where(x => x.Idlista == _idLista && x.Iditem == card_itens[i].IdItem).Any())
                            {
                                save.Add(new EstoquePreClass
                                {
                                    Iditem = card_itens[i].IdItem,
                                    Idgrupo = card_itens[i].IdGrupo,
                                    Idcategoria = card_itens[i].IdCategoria,
                                    Idsubgrupo = card_itens[i].IdSubgrupo,
                                    Idlocal = lista.IdLocal,
                                    Usuario = InfoGlobal.usuario.ToUpper(),
                                    Quantidade = card_itens[i].QuantidadeCont,
                                    Datasave = DateTime.Now,
                                    Sku = card_itens[i].Sku,
                                    Idlista = _idLista,
                                    Finaliza = null
                                });

                                await apiEstoque.CriaContagemFast(save);
                            }
                        }

                        if (await apiEstoque.AttContagem(_idLista, lista.IdLocal))
                        {
                            btnSalvar.IsEnabled = true;
                            btnSalvar.IsVisible = true;
                            index = 0;
                            await DisplayAlert("Aviso", "Contagem finalizada com sucesso!", "Ok");

                            card_itens.Clear();
                            lista_local.Clear();

                            _listaCard.ItemsSource = null;

                            await CarregaItens();
                            await CarregaLocal();
                            await IdContagem();
                        }
                    }
                    else
                    {
                        List<EstoqueClass> listafinaliza = new List<EstoqueClass>();

                        string grupos = string.Empty;
                        string categoria = string.Empty;

                        for (int i = 0; i < card_itens.Count; i++)
                        {
                            grupos += card_itens[i].IdGrupo.ToString() + ",";
                            categoria += card_itens[i].IdCategoria.ToString() + ",";
                        }

                        grupos = grupos.TrimEnd(',');
                        categoria = categoria.TrimEnd(',');

                        string usuario = InfoGlobal.usuario.ToUpper();

                        foreach (var card in card_itens)
                        {
                            listafinaliza.Add(new EstoqueClass
                            {
                                IdLocal = lista.IdLocal,
                                IdGrupo = grupos,
                                DataAbre = DateTime.Now,
                                DataFecha = DateTime.Now,
                                UserAbre = usuario,
                                UserFecha = usuario,
                                IdLista = _idLista,
                                Finalizado = "S",
                                IdCategoriaLista = categoria
                            });
                        }

                        List<EstoquePreClass> save = new List<EstoquePreClass>();

                        foreach (var card in card_itens)
                        {
                            save.Add(new EstoquePreClass
                            {
                                Iditem = card.IdItem,
                                Idgrupo = card.IdGrupo,
                                Idcategoria = card.IdCategoria,
                                Idsubgrupo = card.IdSubgrupo,
                                Idlocal = lista.IdLocal,
                                Usuario = usuario,
                                Quantidade = card.QuantidadeCont,
                                Datasave = DateTime.Now,
                                Sku = card.Sku,
                                Idlista = _idLista,
                                Finaliza = "S"
                            });
                        }

                        if (await apiEstoque.CriaContagemFull(listafinaliza) && await apiEstoque.CriaContagemFast(save))
                        {
                            await DisplayAlert("Aviso", "Contagem finalizada com sucesso!", "Ok");

                            card_itens.Clear();
                            lista_local.Clear();

                            _listaCard.ItemsSource = null;

                            await CarregaItens();
                            await CarregaLocal();
                            await IdContagem();
                        }
                    }

                    stackPrincipal.IsVisible = true;
                    LoadingIndicator.IsVisible = false;
                    LoadingIndicator.IsRunning = false;
                }
            }

            btnFinalizar.IsEnabled = true;
            btnSalvar.IsEnabled = true;
        }
        catch (Exception ex)
        {
            await MetodoErroLog(ex);
            return;
        }
    }

    private void _listaItem_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        try
        {
            _listaLocal.Unfocus();

            ItensClass lista = (ItensClass)_listaItem.SelectedItem;

            sbItens.Text = lista.Descricao;
            frItens.IsVisible = false;

            bool existe = card_itens.Where(x => x.Descricao == lista.Descricao).Any();

            if (!existe)
            {
                CriaCardItem(lista);
            }

            sbItens.Text = string.Empty;
        }
        catch (Exception ex)
        {
            MetodoErroLog(ex);
            return;
        }
    }

    private void Entry_Unfocused(object sender, FocusEventArgs e)
    {
        try
        {
            // Crie uma view invisível e dê o foco a ela para evitar que o teclado abra
            var dummyView = new Entry();
            dummyView.IsVisible = false;
            dummyView.IsReadOnly = false;
            dummyView.Focus();
        }
        catch (Exception ex)
        {
            MetodoErroLog(ex);
            return;
        }
    }

    protected override void OnAppearing()
    {
        try
        {
            App.Current.MainPage.SetValue(Shell.FlyoutBehaviorProperty, FlyoutBehavior.Flyout);
            NavigationPage.SetHasNavigationBar(this, false);
            InfoGlobal.isMenuOpen = true;

            base.OnAppearing();
        }
        catch (Exception ex)
        {
            MetodoErroLog(ex);
            return;
        }
    }

    #endregion
}