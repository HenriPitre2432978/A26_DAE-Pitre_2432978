namespace RechercheLivres.Model
{
    /// <summary>
    /// Classe object dans laquelle l'objet SQL se "désérialise" en C#
    /// </summary>
    public class Membre
    {
        public int IdMembre { get; set; }

        public string Nom { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Telephone { get; set; }
    }
}