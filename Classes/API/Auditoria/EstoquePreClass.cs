using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppPousadaPeNaTerra.Classes.API.Auditoria
{
    public class EstoquePreClass
    {
        public int Id { get; set; }
        public int? Iditem { get; set; }
        public int? Idgrupo { get; set; }
        public int? Idcategoria { get; set; }
        public int? Idsubgrupo { get; set; }
        public string Sku { get; set; }
        public string Idlocal { get; set; }
        public string Usuario { get; set; }
        public string? Quantidade { get; set; }
        public DateTime? Datasave { get; set; }
        public int Idlista { get; set; }
        public string? Finaliza { get; set; }
        public Decimal EstoqueAtual { get; set; }
    }
}
