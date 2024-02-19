using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppGerenciamento.Classes.API.Auditoria
{
    public class EstPrevistoClass
    {
        public string? sku { get; set; }
        public Decimal? estantigo { get; set; }
        public int? vendas { get; set; }
        public Decimal? estprev { get; set; }
    }
}
