using MySql.Data.MySqlClient;

namespace RechercheLivres.Service
{
    internal class OutilSQL
    {
        public MySqlConnection? LaConnexion { get; set; } = null;

        #region Connexion et Vérification du serveur SQL

        /// <summary>
        /// Vérifie que l'user veut utiliser les credentials de base
        /// </summary>
        public void InitialiserConnexion()
        {
            //Se connecter à une base de données dans un serveur à l'aide de l'uid et du pwd du compte SQL
            string connectionString = "server=sql.decinfo-cchic.ca;port=33306;uid=dev-2432978;pwd=DONNEZ-MOI 100% SVP;database=a26_e80_tp1_2432978";

            LaConnexion = new MySqlConnection(connectionString);
        }

        /// <summary>
        /// Vérifie que la connexion contient des paramètres et est bel est bien instanciée.
        /// </summary>
        /// <exception cref="Exception">Exception lancée si la connexion = "null"</exception>
        private void VerifierConnexionBD()
        {
            if (LaConnexion == null)
            {
                throw new Exception("La connexion est nulle, veuillez Initialiser la Connexion avec les paramètres d'entrée requis."); //Throw exception = envoyer code d'erreur
            }
        }

        #endregion

        #region Appel des procédures stockées

        /// <summary>
        /// Appelle la procédure du même nom, gère ses exceptions possibles et formatte sa valeurs de retour
        /// </summary>
        /// <param name="id">Identifiant numérique de l'enseignant</param>
        /// <returns>Une string formattée des détails de l'enseignant. Retourne 9 retours de ligne.</returns>
        public string AppelerProcedureGetEnsByID(int id)
        {
            VerifierConnexionBD();
            string resultatFormatted = "!ERREUR! Une exception a empêché un résultat valide.";
            try //Code qui se lance en étant sujet à une exception/erreur
            {
                if (LaConnexion != null) LaConnexion.Open();

                MySqlCommand laCommande = new MySqlCommand("GetEnsById", LaConnexion);

                laCommande.CommandType = System.Data.CommandType.StoredProcedure;

                //Fournir les paramètres selon la procédure stockée appelée
                laCommande.Parameters.AddWithValue("@idEnseignantActuel", id);

                MySqlDataReader resultatRequete = laCommande.ExecuteReader();  //Pour exécuter des procédures qui retournent une ou plusieurs lignes (Select)

                //Si la requete est lisible, utiliser les paramètres
                if (resultatRequete.Read())
                {
                    //Déclarer les valeurs pouvant être "null" 
                    string ens_courriel = "Non spécifié", ens_telephone = "Non spécifié", ens_echelon = "Non spécifié", ens_date_retraite = "Non spécifié";

                    //Enregistrer les résultats de la procédure et les enregistrer dans une string formattée de retour
                    string ens_id = resultatRequete.GetInt32("tp3_ens_id").ToString();
                    string ens_nom = resultatRequete.GetString("tp3_ens_nom");
                    string ens_prenom = resultatRequete.GetString("tp3_ens_prenom");
                    string ens_discipline = resultatRequete.GetString("tp3_ens_discipline");
                    if (!valeurEstNull(resultatRequete, "tp3_ens_courriel")) ens_courriel = resultatRequete.GetString("tp3_ens_courriel");
                    if (!valeurEstNull(resultatRequete, "tp3_ens_telephone")) ens_telephone = resultatRequete.GetString("tp3_ens_telephone");
                    string ens_anciennete = resultatRequete.GetInt32("tp3_ens_anciennete").ToString();
                    if (!valeurEstNull(resultatRequete, "tp3_ens_echelon_salaire")) ens_echelon = resultatRequete.GetInt32("tp3_ens_echelon_salaire").ToString();
                    string ens_date_embauche = resultatRequete.GetDateTime("tp3_ens_date_embauche").ToString("dd MMMM yyyy");
                    if (!valeurEstNull(resultatRequete, "tp3_ens_date_retraite")) ens_date_retraite = resultatRequete.GetDateTime("tp3_ens_date_retraite").ToString("dd MMMM yyyy");

                    resultatFormatted =
                            $"\n|ID :\t\t\t" + ens_id +
                            $"\n|Nom, Prénom :\t\t" + ens_nom + ", " + ens_prenom +
                            $"\n|Discipline :\t\t" + ens_discipline +
                            $"\n|Courriel :\t\t" + ens_courriel +
                            $"\n|Téléphone :\t\t" + ens_telephone +
                            $"\n|Ancienneté :\t\t" + ens_anciennete +
                            $"\n|Échelon salarial :\t" + ens_echelon +
                            $"\n|Date Embauche :\t" + ens_date_embauche +
                            $"\n|Date Retraite :\t" + ens_date_retraite;
                }
                else resultatFormatted = " Aucun enseignant trouvé avec cet identifiant. ";
            }
            catch (MySqlException sqlExc) //Code lancé à la suite d'une erreur(exception) dans try ^^
            {
                Console.WriteLine(sqlExc.ToString());
            }
            finally //Code qui se lance dans tous les cas, à la fin du code
            {
                if (LaConnexion != null)
                {
                    LaConnexion.Close();
                }
            }
            return resultatFormatted;
        }

        /// <summary>
        /// Apelle la procédure stockée du même nom et convertit le résultat en entier. 
        /// </summary>
        /// <param name="departementActuel">Le département sujet au comptage. Non sensible à la casse.</param>
        /// <returns>Le nombre d'enseignants dans le département spécifié. Retourne 0 si aucun enseignant et si la conversion n'a pas fonctionné(erreur). </returns>
        public int AppelerProcedureGetNbEnsByDept(string departementActuel)
        {
            VerifierConnexionBD();
            int resultatNb = 0;

            try //Code qui se lance en étant sujet à une exception/erreur
            {
                if (LaConnexion != null) LaConnexion.Open();

                MySqlCommand laCommande = new MySqlCommand("GetNbEnsByDept", LaConnexion);

                laCommande.CommandType = System.Data.CommandType.StoredProcedure;

                //Fournir les paramètres selon la procédure stockée appelée
                laCommande.Parameters.AddWithValue("@disciplineActuelle", departementActuel);

                object resultat = laCommande.ExecuteScalar();//Pour exécuter des procédures qui retournent une valeur(Count, Max...)
                resultatNb = Convert.ToInt32(resultat);
            }
            catch (MySqlException sqlExc) //Code lancé à la suite d'une erreur(exception) dans try ^^
            {
                Console.WriteLine(sqlExc.ToString());
            }
            finally //Code qui se lance dans tous les cas, à la fin du code
            {
                if (LaConnexion != null)
                {
                    LaConnexion.Close();
                }
            }
            return resultatNb;
        }

        /// <summary>
        /// Apelle la procédure stockée du même nom et convertit le résultat en entier. 
        /// </summary>
        /// <param name="departementActuel">Le département sujet au comptage. Non sensible à la casse.</param>
        /// <returns>Le nombre d'enseignants dans le département spécifié. Retourne 0 si aucun enseignant et si la conversion n'a pas fonctionné(erreur). </returns>
        public void AppelerProcedureSetNewEns(int id, string nom, string prenom, string discipline, string? courriel, string? telephone, int anciennete, int? echelon, DateTime date_embauche, DateTime? date_retraite)
        {
            VerifierConnexionBD();

            if (verifierEnsIdValide(id) == true) //Si l'id n'est pas déjà dans la base de données
            {
                try //Code qui se lance en étant sujet à une exception/erreur
                {

                    if (LaConnexion != null) LaConnexion.Open();

                    MySqlCommand laCommande = new MySqlCommand("SetNewEns", LaConnexion);

                    laCommande.CommandType = System.Data.CommandType.StoredProcedure;

                    //Fournir les paramètres selon la procédure stockée appelée
                    laCommande.Parameters.AddWithValue("@ens_id", id);
                    laCommande.Parameters.AddWithValue("@ens_nom", nom);
                    laCommande.Parameters.AddWithValue("@ens_prenom", prenom);
                    laCommande.Parameters.AddWithValue("@ens_discipline", discipline);
                    laCommande.Parameters.AddWithValue("@ens_courriel", courriel);
                    laCommande.Parameters.AddWithValue("@ens_telephone", telephone);
                    laCommande.Parameters.AddWithValue("@ens_anciennete", anciennete);
                    laCommande.Parameters.AddWithValue("@ens_echelon", echelon);
                    laCommande.Parameters.AddWithValue("@ens_date_embauche", date_embauche);
                    laCommande.Parameters.AddWithValue("@ens_date_retraite", date_retraite);

                    laCommande.ExecuteNonQuery();//Pour exécuter des procédures qui ne retournent rien (Insert, Update, Delete)

                    //Affichage de la confirmation
                    Console.WriteLine("");
                    FonctionsDeBase.AfficherEtoiles(60);
                    FonctionsDeBase.AfficherLigne($"{prenom} {nom} a été ajouté aux enseignants.", ConsoleColor.Green);
                    FonctionsDeBase.AfficherEtoiles(60);

                    //Affichage si désiré du nouvel enseignant
                    string choix = FonctionsDeBase.LireTexteNonVide("\nDésirez-vous voir le profil du nouvel enseignant ? (oui/non) : ").ToLower();
                    if (choix == "oui")
                    {
                        Console.WriteLine("");
                        string resultatEnsID = AppelerProcedureGetEnsByID(id);
                        FonctionsDeBase.AfficherTexteProgressif(resultatEnsID, ConsoleColor.Green, 15);
                    }
                }
                catch (MySqlException sqlExc) //Code lancé à la suite d'une erreur(exception) dans try ^^
                {
                    Console.WriteLine(sqlExc.ToString());
                }
                finally //Code qui se lance dans tous les cas, à la fin du code
                {
                    if (LaConnexion != null)
                    {
                        LaConnexion.Close();
                    }
                }
            }
            else FonctionsDeBase.AfficherTexteProgressif("!ERREUR! Ajout de l'enseignant impossible; l'identifiant du nouvel enseignant est déjà pris.");
        }

        #endregion

        #region Vérification des valeurs reçues

        private bool valeurEstNull(MySqlDataReader resultatRequete, string nomColonne)
        {
            int indexColonneVerif = resultatRequete.GetOrdinal(nomColonne);
            return valeurEstNull(resultatRequete, indexColonneVerif);
        }
        private bool valeurEstNull(MySqlDataReader resultatRequete, int indexColonne)
        {
            return resultatRequete.IsDBNull(indexColonne);
        }

        private bool verifierEnsIdValide(int id)
        {
            bool valide = false; ;
            if (id >= 0)
            {
                string retourEns = AppelerProcedureGetEnsByID(id);
                if (retourEns == " Aucun enseignant trouvé avec cet identifiant. ") valide = true;
            }
            return valide;
        }

        #endregion
    }
}
