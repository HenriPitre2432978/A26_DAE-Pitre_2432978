using MySql.Data.MySqlClient;
using RechercheLivres.Model;

namespace RechercheLivres.Service.SQLFetch
{
    public class EmpruntService
    {
        //Helper connexion SQL
        private readonly OutilSQL _outilSQL;

        //Constructeur
        public EmpruntService()
        {
            _outilSQL = new OutilSQL();
            _outilSQL.InitialiserConnexion();
        }

        /// <summary>
        /// Call la procédure stockée "GetTousEmprunts" et désérialiser son résultat en objet C#
        /// </summary>
        /// <returns>List d'objets "Emprunts"</returns>
        public List<Emprunt> GetTousEmprunts()
        {
            List<Emprunt> emprunts = [];

            using MySqlDataReader resultat =
                _outilSQL.ExecuterProcedureLecture("GetTousEmprunts");

            while (resultat.Read())
            {
                Emprunt emprunt = new Emprunt
                {
                    IdEmprunt = resultat.GetInt32("IdEmprunt"),

                    IdLivre = resultat.GetInt32("IdLivre"),

                    Titre = resultat.GetString("Titre"),

                    IdMembre = resultat.GetInt32("IdMembre"),

                    NomMembre = resultat.GetString("NomMembre"),

                    DateDebut = resultat.GetDateTime("DateDebut"),

                    DateEcheance = resultat.GetDateTime("DateEcheance"),

                    DateRetour = resultat.IsDBNull(
                        resultat.GetOrdinal("DateRetour"))
                        ? null
                        : resultat.GetDateTime("DateRetour")
                };

                emprunts.Add(emprunt);
            }

            return emprunts;
        }
    }
}