namespace RechercheLivres.Model
{
    /// <summary>
    /// Classe object dans laquelle l'objet SQL se "désérialise" en C#
    /// </summary>
    public class Emprunt
    {
        public int IdEmprunt { get; set; }

        public int IdLivre { get; set; }

        public string Titre { get; set; } = string.Empty;

        public int IdMembre { get; set; }

        public string NomMembre { get; set; } = string.Empty;

        public DateTime DateDebut { get; set; }

        public DateTime DateEcheance { get; set; }

        public DateTime? DateRetour { get; set; }
    }
}