namespace RechercheLivres.Model
{
    /// <summary>
    /// Classe object dans laquelle l'objet SQL se "désérialise" en C#
    /// </summary>
    public class Livre
    {
        public int IdLivre { get; set; }

        public string Titre { get; set; } = string.Empty;

        public DateTime DatePublication { get; set; }

        public string ISBN { get; set; } = string.Empty;

        public int NbExemplaires { get; set; }

        public string Auteur { get; set; } = string.Empty;

        public string Genre { get; set; } = string.Empty;
    }
}