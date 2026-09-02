using MySql.Data.MySqlClient;
using RechercheLivres.Model;

namespace RechercheLivres.Service.SQLFetch
{
    public class MembreService
    {
        //Helper de connexion SQL
        private readonly OutilSQL _outilSQL;

        //Constructeur
        public MembreService()
        {
            _outilSQL = new OutilSQL();
            _outilSQL.InitialiserConnexion();
        }

        /// <summary>
        /// Call la procédure stocké "GetTousMembre" et désérialiser son résultat en objet C#
        /// </summary>
        /// <returns>List d'objets "Membre"</returns>
        public List<Membre> GetTousMembres()
        {
            List<Membre> membres = [];

            using MySqlDataReader resultat =
                _outilSQL.ExecuterProcedureLecture("GetTousMembres");

            while (resultat.Read())
            {
                Membre membre = new Membre
                {
                    IdMembre = resultat.GetInt32("IdMembre"),
                    Nom = resultat.GetString("Nom"),
                    Email = resultat.GetString("Email"),

                    Telephone = resultat.IsDBNull(
                        resultat.GetOrdinal("Telephone"))
                        ? null
                        : resultat.GetString("Telephone")
                };

                membres.Add(membre);
            }

            return membres;
        }
    }
}