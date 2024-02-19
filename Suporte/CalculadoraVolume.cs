namespace AppGerenciamento.Suporte
{
    public class CalculadoraVolume
    {
        public static string CalcularVolumeTotal(string volumePorUnidadeStr, string quantidadeTotalStr, string unidadeMedida)
        {
            // Verificar se os parâmetros não são nulos
            if (volumePorUnidadeStr == null || quantidadeTotalStr == null || unidadeMedida == null)
                return "0";

            double volumePorUnidade, volumeTotal;
            double quantidadeTotal;

            // Tentar converter as strings para os tipos necessários
            if (!double.TryParse(volumePorUnidadeStr, out volumePorUnidade) || !double.TryParse(quantidadeTotalStr, out quantidadeTotal))
                return "0";

            // Verificar se a unidade de medida é reconhecida
            switch (unidadeMedida)
            {
                case "ml":
                    volumeTotal = volumePorUnidade * quantidadeTotal;
                    return volumeTotal.ToString();
                case "lt":
                    volumeTotal = volumePorUnidade * quantidadeTotal;
                    return volumeTotal.ToString();
                case "kg":
                    // Considerando que 1 kg de água é aproximadamente igual a 1 litro
                    volumeTotal = volumePorUnidade * quantidadeTotal;
                    return volumeTotal.ToString();
                case "un":
                    // Não há conversão direta, portanto, volume total é igual ao volume por unidade
                    volumeTotal = quantidadeTotal;
                    return volumeTotal.ToString();
                default:
                    return "un";
            }
        }

        public static string CalculaVolumePopup(string un, string unMed, string volUn)
        {
            double result = 0;

            if (!string.IsNullOrEmpty(un) && !string.IsNullOrEmpty(volUn))
            {
                double _un = Convert.ToDouble(un.Replace(".",","));
                double _volUn = Convert.ToDouble(volUn);

                if (!string.IsNullOrEmpty(unMed))
                {
                    double _unMed = Convert.ToDouble(unMed);
                    result = _un * _volUn + _unMed;
                }
                else
                {
                    result = _un * _volUn;
                }
            }
            else if (!string.IsNullOrEmpty(unMed))
            {
                result = Convert.ToDouble(unMed);
            }

            return result.ToString();
        }

        public static string QuantidadeFinalIten(string unMed, string unQtd, string volUn)
        {
            // Verifica se os parâmetros não são nulos e define valores padrão como 0 caso sejam
            double quantidadeMedida = double.TryParse(unMed, out double med) ? med : 0;
            double quantidadeTotal = 0;
            double volumeIten = double.TryParse(volUn, out double vol) ? vol : 0;

            // Verifica se unQtd é um número válido e diferente de zero antes de calcular quantidadeTotal
            if (double.TryParse(unQtd, out double qtd) && qtd != 0)
            {
                quantidadeTotal = volumeIten * qtd;
            }

            double quantidadeFinal;

            // Verifica se volumeIten é zero para evitar divisão por zero
            if (volumeIten != 0)
            {
                quantidadeFinal = (quantidadeTotal + quantidadeMedida) / volumeIten;
            }
            else
            {
                quantidadeFinal = 0;
            }

            // Verifica se o resultado é NaN ou Infinito
            if (double.IsNaN(quantidadeFinal) || double.IsInfinity(quantidadeFinal))
            {
                quantidadeFinal = 0;
            }

            // Limita o valor retornado a duas casas decimais
            return quantidadeFinal.ToString("N2");
        }
    }
}
