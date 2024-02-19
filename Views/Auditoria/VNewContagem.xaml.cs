using AppGerenciamento.Classes.API.Auditoria;
using AppGerenciamento.Classes.API.Principal;
using AppGerenciamento.Classes.Globais;
using AppGerenciamento.Services.Auditoria;
using AppGerenciamento.Services.Principal;
using AppGerenciamento.Suporte;
using AppGerenciamento.Views.Principal;
using Microsoft.Maui;
using System.Globalization;
using System.Text;

namespace AppGerenciamento.Views;

public partial class VNewContagem : ContentPage
{
    #region 1- Variaveis
    APILocalAud apiLocal = new APILocalAud();
    APIEstoqueAud apiEstoque = new APIEstoqueAud();
    APIItensAud apiItens = new APIItensAud();
    APIEnviaArquivos aPIEnviaArquivos = new APIEnviaArquivos();

    APIErroLog error = new();
    ExceptionHandlingService _exceptionService = new();


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
        await _exceptionService.ReportError(ex);
    }

    private async Task MetodosIniciais()
    {
        try
        {
            imagensCacheSup.ApagaCacheImagens();

            if (index == 0)
            {
                btnFinalizar.IsVisible = true;
                camera = true;

                await CarregaItens();
                await CarregaLocal();
                await IdContagem();
            }
            else if (index == 2)
            {
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

    private async void Iniciais()
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
            await MetodoErroLog(ex);
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
                EstPrevistoClass est = await apiItens.EstoquePrevisto(item.Sku);

                string estAtualPre = est.estprev.ToString();

                if (index == 2)
                {
                    estAtualPre = item.EstoqueAtual.ToString();
                }

                card_itens.Add(new ItensClass
                {
                    Descricao = itens[0].Descricao,
                    Sku = item.Sku,
                    IdItem = (int)item.Iditem,
                    IdGrupo = item.Idgrupo,
                    IdCategoria = (int)item.Idcategoria,
                    IdSubgrupo = item.Idsubgrupo,
                    QuantidadeCont = CalculadoraVolume.CalculaVolumePopup(item.Quantidade.ToString(), itens[0].QuantidadeMed, itens[0].Volume.ToString()),
                    QuantidadeMed = itens[0].QuantidadeMed,
                    Volume = itens[0].Volume,
                    QuantidadeUn = item.Quantidade.ToString(),
                    estPrevUn = itens[0].Unidade + "/",
                    estPrevUnAp = itens[0].Unidade,
                    estUnP = estAtualPre,
                    estPrev = CalculadoraVolume.CalcularVolumeTotal(itens[0].Volume.ToString(), estAtualPre, itens[0].Unidade),
                    estAntigo = est.estantigo.ToString(),
                    vendaPeriodo = est.vendas.ToString(),
                    Unidade = itens[0].Unidade,
                    QtdFinalIten = CalculadoraVolume.QuantidadeFinalIten(itens[0].QuantidadeMed, itens[0].QuantidadeUn, itens[0].Volume.ToString()),
                    ColorAviso = itens[0].ColorAviso
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

    private async void CriaCardItem(ItensClass lista, EstPrevistoClass listaPrev)
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

            Color previsto = lista.ColorAviso;

            string _estPrev = "0";

            if (!string.IsNullOrEmpty(listaPrev.estprev.ToString()))
            {
                _estPrev = listaPrev.estprev.ToString();
            }

            string qtFinal = CalculadoraVolume.QuantidadeFinalIten(lista.QuantidadeMed, lista.QuantidadeUn, lista.Volume.ToString());

            card_itens.Add(new ItensClass
            {
                Descricao = lista.Descricao,
                Sku = lista.Sku,
                IdItem = lista.IdItem,
                QuantidadeCont = CalculadoraVolume.CalculaVolumePopup(lista.QuantidadeCont, lista.QuantidadeMed, lista.Volume.ToString()),
                QuantidadeMed = lista.QuantidadeMed,
                Volume = lista.Volume,
                QuantidadeUn = lista.QuantidadeUn,
                IdGrupo = lista.IdGrupo,
                IdCategoria = lista.IdCategoria,
                IdSubgrupo = lista.IdSubgrupo,
                estPrevUn = lista.Unidade + "/",
                estPrevUnAp = lista.Unidade,
                estUnP = listaPrev.estprev.ToString(),
                estPrev = CalculadoraVolume.CalcularVolumeTotal(lista.Volume.ToString(), _estPrev, lista.Unidade),
                estAntigo = listaPrev.estantigo.ToString(),
                vendaPeriodo = listaPrev.vendas.ToString(),
                Unidade = lista.Unidade,
                ColorAviso = previsto,
                QtdFinalIten = qtFinal
            });

            _listaCard.ItemsSource = card_itens.ToList();

            List<EstoquePreClass> pre = await VerificaIdLista();

            LocalClass local = (LocalClass)_listaLocal.SelectedItem;

            if (pre.Count > 0)
            {
                if (!pre.Any(x => x.Iditem == lista.IdItem))
                {
                    EstoquePreClass preNewNew = new EstoquePreClass();

                    preNewNew.Iditem = lista.IdItem;
                    preNewNew.Idgrupo = lista.IdGrupo;
                    preNewNew.Idcategoria = lista.IdCategoria;
                    preNewNew.Idsubgrupo = lista.IdSubgrupo;
                    preNewNew.Sku = lista.Sku;
                    preNewNew.Idlocal = local.IdLocal;
                    preNewNew.Usuario = InfoGlobal.usuario.ToUpper();
                    preNewNew.Quantidade = Convert.ToDecimal(qtFinal);
                    preNewNew.Datasave = DateTime.Now;
                    preNewNew.Idlista = _idLista;

                    await apiEstoque.AdicionaItensContPre(preNewNew);
                }
            }
            else
            {
                await apiEstoque.CriaPreviaContagemFull(AbasteceFull(local));

                EstoquePreClass preNewNew = new EstoquePreClass();

                preNewNew.Iditem = lista.IdItem;
                preNewNew.Idgrupo = lista.IdGrupo;
                preNewNew.Idcategoria = lista.IdCategoria;
                preNewNew.Idsubgrupo = lista.IdSubgrupo;
                preNewNew.Sku = lista.Sku;
                preNewNew.Idlocal = local.IdLocal;
                preNewNew.Usuario = InfoGlobal.usuario.ToUpper();
                preNewNew.Quantidade = Convert.ToDecimal(qtFinal);
                preNewNew.Datasave = DateTime.Now;
                preNewNew.Idlista = _idLista;

                await apiEstoque.AdicionaItensContPre(preNewNew);
            }
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

    private EstoqueClass AbasteceFull(LocalClass local)
    {
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

        EstoqueClass full = new EstoqueClass();

        full.IdLocal = local.IdLocal;
        full.IdGrupo = grupos;
        full.DataAbre = DateTime.Now;
        full.UserAbre = usuario;
        full.IdLista = _idLista;
        full.IdCategoriaLista = categoria;

        return full;
    }

    private EstoqueClass AbasteceFullFinaliza(LocalClass local)
    {
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

        EstoqueClass full = new EstoqueClass();

        full.IdLocal = local.IdLocal;
        full.IdGrupo = grupos;
        full.DataFecha = DateTime.Now;
        full.UserFecha = usuario;
        full.IdLista = _idLista;
        full.Finalizado = "S";
        full.IdCategoriaLista = categoria;

        return full;
    }

    private async Task<bool> AbasteceFinalizaItens()
    {
        List<SelectEstoqueAtual> newEst = new List<SelectEstoqueAtual>();

        foreach (var item in card_itens)
        {
            newEst.Add(new SelectEstoqueAtual
            {
                IdItem = item.IdItem,
                IdLista = _idLista,
                Atual = Convert.ToDecimal(item.estPrev),
                Sku = item.Sku
            });
        }

        return await apiEstoque.AttHistorico(newEst);
    }

    private async Task<List<EstoquePreClass>> VerificaIdLista()
    {
        List<EstoquePreClass> lista = await apiEstoque.ContagemFastId(_idLista);

        if (lista != null && lista.Count > 0)
        {
            return lista;
        }

        return new List<EstoquePreClass>();
    }

    private ItensClass AtuializaPropItens(ItensClass propAtual, List<ItensClass> propNova)
    {
        ItensClass novoItem = new ItensClass();

        foreach (var item in propNova)
        {
            propAtual.Volume = item.Volume;
            propAtual.Unidade = item.Unidade;
            propAtual.Peso = item.Peso;
            propAtual.Preco = item.Preco;

            novoItem.QuantidadeUn = item.QuantidadeUn;
            novoItem.QuantidadeCont = item.QuantidadeCont;
            novoItem.QuantidadeMed = item.QuantidadeMed;
            novoItem.QtdFinalIten = item.QtdFinalIten;
            novoItem.estPrev = item.estPrev;
            novoItem.estUnP = item.estUnP;
            novoItem.estPrevUn = item.estPrevUn;
            novoItem.estPrevUnAp = item.estPrevUnAp;
            novoItem.IdItem = item.IdItem;
            novoItem.IdLocal = item.IdLocal;
            novoItem.SkuCb = item.SkuCb;
            novoItem.CodItemCb = item.CodItemCb;
            novoItem.Descricao = item.Descricao;
            novoItem.estAntigo = item.estAntigo;
            novoItem.vendaPeriodo = item.vendaPeriodo;
            novoItem.Preco = item.Preco;
            novoItem.IdCategoria = item.IdCategoria;
            novoItem.IdGrupo = item.IdGrupo;
            novoItem.IdSubgrupo = item.IdSubgrupo;
            novoItem.Sku = item.Sku;
            novoItem.Volume = item.Volume;
            novoItem.Peso = item.Peso;
            novoItem.Ativo = item.Ativo;
            novoItem.CadastradoEm = item.CadastradoEm;
            novoItem.CadastradoPor = item.CadastradoPor;
            novoItem.AtualizadoEm = item.AtualizadoEm;
            novoItem.AtualizadoPor = item.AtualizadoPor;
            novoItem.IdReceita = item.IdReceita;
            novoItem.Codbarra = item.Codbarra;
            novoItem.InclusoReserva = item.InclusoReserva;
            novoItem.PrecoReserva = item.PrecoReserva;
            novoItem.LimiteReserva = item.LimiteReserva;
            novoItem.DescricaoCozinha = item.DescricaoCozinha;
            novoItem.RefeicaoCompleta = item.RefeicaoCompleta;
            novoItem.ComplementoRefeicao = item.ComplementoRefeicao;
            novoItem.Cozinha = item.Cozinha;
            novoItem.BarPiscina = item.BarPiscina;
            novoItem.Drink = item.Drink;
            novoItem.Ean = item.Ean;
            novoItem.Unidade = item.Unidade;
            novoItem.IdFt = item.IdFt;
        }

        return novoItem;
    }

    #endregion

    #region 4- Eventos de controle
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
                if (sbItens.Text.Length > 2)
                {
                    frItens.IsVisible = true;
                    _listaItem.ItemsSource = filtroItens;
                }
                else if (sbItens.Text.Length < 1)
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

    private async void btnFinalizar_Clicked(object sender, EventArgs e)
    {
        try
        {
            LocalClass local = (LocalClass)_listaLocal.SelectedItem;

            btnFinalizar.IsEnabled = false;

            if (await DisplayAlert("AVISO", "Deseja finalizar a contagem?", "Sim", "Não"))
            {
                stackPrincipal.IsVisible = false;
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;

                if (!await aPIEnviaArquivos.SalvaImagens())
                {
                    await DisplayAlert("Erro", "Erro ao salvar imagens!", "Ok");
                }

                if (!await apiEstoque.AtualizaContagemFull(AbasteceFullFinaliza(local)))
                {
                    await DisplayAlert("Erro", "Erro ao finalizar contagem!", "Ok");
                }

                if (!await AbasteceFinalizaItens())
                {
                    await DisplayAlert("Erro", "Erro ao finalizar contagem!", "Ok");
                }
                else
                {
                    await DisplayAlert("Aviso", "Contagem finalizada com sucesso!", "Ok");
                    await Navigation.PushModalAsync(new VContagemAberta());
                }

                stackPrincipal.IsVisible = true;
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
            }

            btnFinalizar.IsEnabled = true;
        }
        catch (Exception ex)
        {
            await MetodoErroLog(ex);
            return;
        }
    }

    private async void _listaItem_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        try
        {
            _listaLocal.Unfocus();

            ItensClass lista = (ItensClass)_listaItem.SelectedItem;

            sbItens.Text = lista.Descricao;
            frItens.IsVisible = false;

            List<EstoquePreClass> pre = await VerificaIdLista();

            List<ItensClass> itemAtt = await apiItens.ListaItensId(lista.IdItem);

            bool existe = pre.Where(x => x.Iditem == lista.IdItem).Any();

            if (!existe)
            {
                if (itemAtt[0].Volume <= 0.00M)
                {
                    await DisplayAlert("Aviso", "Este item está com o volume zerado. Por favor, atualize-o para adicioná-lo à contagem.", "Ok");
                    sbItens.Text = string.Empty;
                    return;
                }
                else
                {
                    ItensClass novaClassIten = AtuializaPropItens(lista, itemAtt);

                    pupQuantidade.IsVisible = true;

                    if (itemAtt[0].Unidade.ToString().ToLower() == "un")
                    {
                        lblMedidaPup.IsVisible = false;
                        quantidadeEntryMed.IsVisible = false;
                    }
                    else
                    {
                        lblMedidaPup.IsVisible = true;
                        quantidadeEntryMed.IsVisible = true;
                        lblMedidaPup.Text = itemAtt[0].Unidade.ToString().ToLower();
                    }

                    quantidadeEntry.Focus();

                    // Aguarda até que pupQuantidade.IsVisible seja false
                    while (pupQuantidade.IsVisible)
                    {
                        // Espera um curto período de tempo para evitar consumo excessivo de CPU
                        await Task.Delay(100);
                    }

                    int quantidadeUn = 0;

                    if (!string.IsNullOrEmpty(quantidadeEntry.Text))
                    {
                        quantidadeUn = Convert.ToInt32(quantidadeEntry.Text);

                        if (quantidadeUn < 0)
                        {
                            quantidadeUn = 0;
                        }
                    }

                    double quantidadeMed = 0;

                    if (!string.IsNullOrEmpty(quantidadeEntryMed.Text))
                    {
                        quantidadeMed = Convert.ToDouble(quantidadeEntryMed.Text.Replace('.', ','));

                        if (quantidadeMed < 0)
                        {
                            quantidadeMed = 0;
                        }
                    }

                    novaClassIten.QuantidadeCont = quantidadeUn.ToString();
                    novaClassIten.QuantidadeMed = quantidadeMed.ToString();
                    novaClassIten.QuantidadeUn = quantidadeUn.ToString();

                    EstPrevistoClass estprev = await apiItens.EstoquePrevisto(novaClassIten.Sku);

                    CriaCardItem(novaClassIten, estprev);

                    quantidadeEntry.Text = string.Empty;
                    quantidadeEntryMed.Text = string.Empty;

                    quantidadeEntry.Unfocus();
                    quantidadeEntryMed.Unfocus();
                }
            }
            else
            {
                await DisplayAlert("Aviso", "Este item já está presente na contagem atual!", "Ok");
            }

            sbItens.Text = string.Empty;
        }
        catch (Exception ex)
        {
            await MetodoErroLog(ex);
            return;
        }
    }

    private void OKButton_Clicked(object sender, EventArgs e)
    {
        pupQuantidade.IsVisible = false;
    }

    private async void SwipeItem_Clicked(object sender, EventArgs e)
    {
        // Obtenha o item associado ao SwipeItem clicado
        if (sender is SwipeItem swipeItem && swipeItem.CommandParameter is ItensClass item)
        {
            try
            {
                card_itens.Remove(item);
                lista_itens.Remove(item);

                // Atualize a fonte de dados da sua CollectionView após a exclusão
                _listaCard.ItemsSource = card_itens.ToList();

                List<EstoquePreClass> listaatual = await apiEstoque.ListaConstagensPorId(_idLista);

                if (listaatual.Count > 0)
                {
                    int indexToRemove = listaatual.FindIndex(x => x.Sku.Contains(item.Sku));

                    if (indexToRemove != -1)
                    {
                        int idLista = listaatual[indexToRemove].Id;
                        string sku = listaatual[indexToRemove].Sku;

                        await apiEstoque.DeletarContagemFast(idLista, sku);

                        listaatual.RemoveAt(indexToRemove);
                    }
                }
            }
            catch (Exception ex)
            {
                await MetodoErroLog(ex);
                return;
            }
        }
    }

    private async void OnFrameTapped(object sender, TappedEventArgs e)
    {
        var frameView = sender as Frame;
        ItensClass selecionado = frameView?.BindingContext as ItensClass;

        if (selecionado != null)
        {
            pupQuantidade.IsVisible = true;

            lblMedidaPup.Text = selecionado.Unidade.ToString().ToLower();

            quantidadeEntry.Text = selecionado.QuantidadeUn;
            quantidadeEntryMed.Text = selecionado.QuantidadeMed;

            // Aguarda até que pupQuantidade.IsVisible seja false
            while (pupQuantidade.IsVisible)
            {
                // Espera um curto período de tempo para evitar consumo excessivo de CPU
                await Task.Delay(100);
            }

            selecionado.QuantidadeCont = CalculadoraVolume.CalculaVolumePopup(quantidadeEntry.Text, quantidadeEntryMed.Text, selecionado.Volume.ToString());
            selecionado.QuantidadeUn = quantidadeEntry.Text;
            selecionado.QtdFinalIten = CalculadoraVolume.QuantidadeFinalIten(quantidadeEntryMed.Text, quantidadeEntry.Text, selecionado.Volume.ToString());

            int id = await apiEstoque.ListaIdPreItens(selecionado.IdItem, _idLista);

            EstoquePreClass preNew = new EstoquePreClass();
            preNew.Id = id;
            preNew.Quantidade = Convert.ToDecimal(selecionado.QtdFinalIten);
            await apiEstoque.AtualizaItensContPre(preNew);

            quantidadeEntry.Text = string.Empty;
            quantidadeEntryMed.Text = string.Empty;
        }
    }
    #endregion
}