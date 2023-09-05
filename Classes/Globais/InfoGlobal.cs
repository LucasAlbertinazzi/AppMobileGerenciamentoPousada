namespace AppPousadaPeNaTerra.Classes.Globais
{
    public static class InfoGlobal
    {
        public static string LastVersao;

        public static string usuario = string.Empty;

        public static string senha = string.Empty;

        public static string funcao = string.Empty;

        public static bool statusCode;

        public static bool isMenuOpen;

        public static int IdItemCamera = 0;

        public static List<string> listaImagens = new List<string>();

        //public static string apiApp = "http://192.168.10.94:5000/api";
        //public static string apiEstoque = "http://192.168.10.94:5005/api";

        public static string apiApp = "http://192.168.85.3:6565/api";
        public static string apiEstoque = "http://192.168.85.3:6566/api";
        public static void ClearData()
        {
            usuario = string.Empty;
            senha = string.Empty;
            funcao = string.Empty;
            statusCode = false;
        }
    }
}
