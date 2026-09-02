using MySql.Data.MySqlClient;
using System.Data;

namespace RechercheLivres.Service.SQLFetch
{
    internal class OutilSQL
    {
        public MySqlConnection? LaConnexion { get; set; } = null;

        #region Connexion

        public void InitialiserConnexion()
        {
            //Identifiants
            string connectionString ="server=sql.decinfo-cchic.ca;" +"port=33306;" +"uid=dev-2432978;" +"pwd=DONNEZ-MOI 100% SVP;" +"database=a26_e80_tp1_2432978";

            LaConnexion = new MySqlConnection(connectionString);
        }

        private void VerifierConnexionBD()
        {
            if (LaConnexion == null)
            {
                throw new Exception(
                    "La connexion est nulle, veuillez initialiser la connexion."
                );
            }
        }

        #endregion

        #region Procédures stockées
        /// <summary>
        /// Obtenir les resultats d'une procédure stockée créée au préalable dans la BD.
        /// </summary>
        /// <param name="nomProcedure">Nom de la procédure stockée désirée</param>
        /// <returns>Le SQL  result en format SQLData</returns>
        public MySqlDataReader ExecuterProcedureLecture(
            string nomProcedure)
        {
            VerifierConnexionBD();

            LaConnexion!.Open();

            MySqlCommand commande =
                new MySqlCommand(nomProcedure, LaConnexion);

            commande.CommandType = CommandType.StoredProcedure;

            return commande.ExecuteReader(
                CommandBehavior.CloseConnection
            );
        }

        #endregion
    }
}