using MySql.Data.MySqlClient;
using RechercheLivres.Model;

namespace RechercheLivres.Service.SQLFetch
{
    public class LivreService
    {
        //Helper connexion SQL
        private readonly OutilSQL _outilSQL;

        //Constructeur
        public LivreService()
        {
            _outilSQL = new OutilSQL();
            _outilSQL.InitialiserConnexion();
        }

        /// <summary>
        /// Call la procédure stocké "GetTousLivres" et désérialiser son résultat en objet C#
        /// </summary>
        /// <returns>List d'objets "Livre"</returns>
        public List<Livre> GetTousLivres()
        {
            List<Livre> livres = [];

            using MySqlDataReader resultat =
                _outilSQL.ExecuterProcedureLecture("GetTousLivres");

            while (resultat.Read())
            {
                Livre livre = new Livre
                {
                    IdLivre = resultat.GetInt32("IdLivre"),

                    Titre = resultat.GetString("Titre"),

                    DatePublication =
                        resultat.GetDateTime("DatePublication"),

                    ISBN = resultat.GetString("ISBN"),

                    NbExemplaires =
                        resultat.GetInt32("NbExemplaires"),

                    Auteur = resultat.GetString("Auteur"),

                    Genre = resultat.GetString("Genre")
                };

                livres.Add(livre);
            }

            return livres;
        }
    }
}