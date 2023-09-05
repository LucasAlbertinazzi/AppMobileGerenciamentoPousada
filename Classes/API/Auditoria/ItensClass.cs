namespace AppPousadaPeNaTerra.Classes.API.Auditoria
{
    public class ItensClass
    {
        public string QuantidadeCont { get; set; }

        public int IdItem { get; set; }

        public string IdLocal { get; set; }

        public int SkuCb { get; set; }

        public string CodItemCb { get; set; }

        public string Descricao { get; set; }

        public decimal? Preco { get; set; }

        public int IdCategoria { get; set; }

        public int? IdGrupo { get; set; }

        public int? IdSubgrupo { get; set; }

        public string Sku { get; set; }

        public decimal Volume { get; set; }

        public decimal Peso { get; set; }

        public bool? Ativo { get; set; }

        public DateTime CadastradoEm { get; set; }

        public int CadastradoPor { get; set; }

        public DateTime AtualizadoEm { get; set; }

        public int AtualizadoPor { get; set; }

        public int? IdReceita { get; set; }

        public string Codbarra { get; set; }

        public bool InclusoReserva { get; set; }

        public decimal PrecoReserva { get; set; }

        public int LimiteReserva { get; set; }

        public string DescricaoCozinha { get; set; }

        public bool RefeicaoCompleta { get; set; }

        public bool ComplementoRefeicao { get; set; }

        public bool Cozinha { get; set; }

        public bool BarPiscina { get; set; }

        public bool Drink { get; set; }

        public string Ean { get; set; }

        public string Unidade { get; set; }

        public long? IdFt { get; set; }
    }
}
