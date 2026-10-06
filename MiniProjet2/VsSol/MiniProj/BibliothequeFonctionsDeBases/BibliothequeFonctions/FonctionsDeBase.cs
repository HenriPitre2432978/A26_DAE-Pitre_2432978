using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace BibliothequeFonctionsDeBase;

public class FonctionsDeBase
{
    ////////////////
    #region Méthodes  communes aux projet DEVAPP



    #region textChecker

    /// <summary>
    /// Checks wether a string is the same as another, ignoring case and accents
    /// </summary>
    /// <param name="source">comparer</param>
    /// <param name="toCheck">compared</param>
    /// <returns>true if theyre the same</returns>
    public static bool ContainsInsensitive(string source, string toCheck)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(toCheck))
            return false;

        CompareInfo compareInfo = CultureInfo.GetCultureInfo("fr-FR").CompareInfo;
        return compareInfo.IndexOf(source, toCheck,
            CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
    }

    #endregion

    #region filenames

    /// <summary>
    /// Obtenir le nom de tous les fichiers se retrouvant dans le dossier, à partir du filepath de chacun des fichiers
    /// </summary>
    /// <param name="chemin">filepath</param>
    /// <param name="condition">type de fichier recherché. Format: "*.txt", "*.csv" ... </param>
    /// <returns>La liste des noms des fichiers</returns>
    public static List<string> GetTxtFilenames(string chemin, string? condition = null)
    {
        List<string> nomsFichiers = [];

        // Use filter if there is one, otherwise get all files
        List<string> cheminFichiers;
        if (!string.IsNullOrWhiteSpace(condition))
            cheminFichiers = [.. Directory.GetFiles(chemin, condition)];
        else
            cheminFichiers = [.. Directory.GetFiles(chemin)];

        // Extract just the file names (without full path)
        foreach (string fichier in cheminFichiers)
        {
            nomsFichiers.Add(Path.GetFileName(fichier));
        }

        return nomsFichiers;
    }


    #endregion

    #region System / Utility prints

    /// <summary>
    /// Shows an error message in red color in the console.
    /// </summary>
    /// <param name="errorCode">msg to show</param>
    public static void ShowError(string errorCode)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nErreur - {errorCode}\n");
        Console.ResetColor();
    }

    /// <summary>
    /// Outputs a list of choices in the console, each prefixed with its index number.
    /// </summary>
    public static void DisplayLineChoices(List<string> choices)
    {
        foreach (string choice in choices)
            AfficherTexteCentre($"{choices.IndexOf(choice) + 1} " + " - " + choice);
    }
    #endregion

    /// <summary>
    /// Vide le buffer des clavier pour éviter un bug du Readkey
    /// </summary>
    public static void FlushInput()
    {
        try
        {
            while (Console.KeyAvailable) Console.ReadKey(true);
        }
        catch (InvalidOperationException) { }//redirect empty
    }


    #region Menu system

    public static class Logger
    {
        private static string logFilePath = "log_activites.txt";

        public static void EnregistrerLog(string codeUtilisateur, string action)
        {
            LogActivite log = new LogActivite(codeUtilisateur, action, DateTime.Now);
            string ligne = $"{log.DateHeure:yyyy-MM-dd HH:mm:ss}\t{log.CodeUtilisateur}\t{log.Action}";
            File.AppendAllText(logFilePath, ligne + Environment.NewLine);
        }
    }





    public class MenuItem(string label, Action action = null, ConsoleColor color = ConsoleColor.White, List<FonctionsDeBase.MenuItem> subMenu = null)
    {
        public string Label { get; set; } = label;
        public Action Action { get; set; } = action;

        public List<MenuItem> SubMenu { get; set; } = subMenu;
        public ConsoleColor Color { get; set; } = color;

        public bool IsSubMenu => SubMenu != null && SubMenu.Count > 0;
    }



    public static bool ShowMenu(List<FonctionsDeBase.MenuItem> menuItems, string? codeUtilisateur, string title = "Menu", bool withRetour = true)
    {
        while (true)
        {

            Console.Clear();

            // CENTRER LE TITRE
            int row = Console.WindowHeight / 2 - (menuItems.Count / 2) - 2;
            AfficherTexteCentre(title, row);
            row += 2;

            // AFFICHER LES OPTIONS CENTRÉES
            if (withRetour)
            {
                AfficherTexteCentre("0. Retour", row++);
            }

            for (int i = 0; i < menuItems.Count; i++)
            {
                int affichage = withRetour ? i + 1 : i;
                Console.ForegroundColor = menuItems[i].Color;
                AfficherTexteCentre($"{affichage}. {menuItems[i].Label}", row++);
                Console.ResetColor();
            }

            // CENTRER LE PROMPT
            string prompt = "Selectionner une option : ";
            AfficherTexteCentre(prompt, row);
            int min = 0;
            int max = withRetour ? menuItems.Count : menuItems.Count - 1;
            CentrerCurseur();
            int choix = LireEntierMinMax("", min, max); // prompt déjà affiché

            if (withRetour && choix == 0)
                return true;

            var optionListe = menuItems[choix - (withRetour ? 1 : 0)];




            if (optionListe.IsSubMenu)
            {
                bool continuer = ShowMenu(optionListe.SubMenu, codeUtilisateur, optionListe.Label, withRetour: true);
                if (!continuer) return false;
            }
            else
            {

                optionListe.Action.Invoke();
                if (optionListe.Label.ToLower().Contains("déconnexion"))
                    return false;

                WaitKey();
            }
        }
    }

    #endregion

    #endregion
    ////////////////

    #region UNUSED (TO READD AFTER)

    #region security

    public class LogActivite
    {
        public string? CodeUtilisateur { get; set; }
        public string? Action { get; set; }
        public DateTime DateHeure { get; set; }


        public LogActivite() { }

        public LogActivite(string _code, string _action, DateTime _dateheure)
        {
            CodeUtilisateur = _code;
            Action = _action;
            DateHeure = _dateheure;
        }
    }

    #endregion#

    #region streamWriter

    /// <summary>
    /// Ajoute une ligne à la fin d'un fichier texte.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="textToWrite"></param>
    public static void AppendLineToFile(string filePath, string textToWrite)
    {
        try
        {
            // Open the file for appending. If the file doesn't exist, it will be created.
            using (StreamWriter writer = new StreamWriter(filePath, append: true))
            {
                writer.WriteLine(textToWrite);
            }
            Console.WriteLine($"Text appended successfully at {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    /// <summary>
    /// Supprime une ligne contenant un GUID spécifique dans un fichier texte.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="guid"></param>
    public static void DeleteLineWithGuid(string filePath, string guid)
    {
        try
        {
            // Read all lines from the file
            var lines = File.ReadAllLines(filePath).ToList();

            // Remove the line that contains the GUID
            var lineToDelete = lines.FirstOrDefault(line => line.Contains(guid));
            if (lineToDelete != null)
            {
                lines.Remove(lineToDelete);
                // Write the remaining lines back to the file
                File.WriteAllLines(filePath, lines);
                Console.WriteLine("Line with GUID deleted successfully.");
            }
            else
            {
                Console.WriteLine("GUID not found in the file.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    /// <summary>
    /// Supprime un fichier spécifique.
    /// </summary>
    /// <param name="filePath"></param>
    public static void DeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine("File deleted successfully.");
            }
            else
            {
                Console.WriteLine("File does not exist.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error deleting file: " + ex.Message);
        }
    }

    /// <summary>
    /// Crée un fichier à l'emplacement spécifié.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    public static void CreateFile(string filePath, string fileName = null)
    {
        try
        {
            // Determine the full path
            string fullPath = fileName == null
                ? filePath
                : Path.Combine(filePath, fileName);

            // Create directory if it doesn't exist
            string directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create the file if it doesn't exist
            if (!File.Exists(fullPath))
            {
                File.Create(fullPath).Dispose(); // Dispose to release file handle immediately
                Console.WriteLine($"File created: {fullPath}");
            }
            else
            {
                Console.WriteLine("File already exists.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error creating file: " + ex.Message);
        }
    }

    #endregion

    #region files functions

    public static bool IsValidFileName(string fileName, string? errorMsg = "Nom de fichier invalide")
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            AfficherTexteCentre(errorMsg);
            return false;
        }

        // Get the invalid characters from the system
        char[] invalidChars = Path.GetInvalidFileNameChars();

        // Check if any invalid character is present
        if (fileName.Any(c => invalidChars.Contains(c)))
        {
            AfficherTexteCentre(errorMsg);
            return false;
        }

        return true;
    }


    /// <summary>
    /// Verifies if a file does NOT exist in the specified directory.
    /// </summary>
    /// <param name="directoryPath">The full directory path (e.g. "C:\\MyFolder")</param>
    /// <param name="fileName">The desired file name (e.g. "document.txt")</param>
    /// <returns>True if the file does not exist; False if it exists.</returns>
    public static bool FileDoesNotExist(string directoryPath, string fileName)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
            AfficherTexteCentre($"Directory path cannot be null or empty., {nameof(directoryPath)}");

        if (string.IsNullOrWhiteSpace(fileName))
            AfficherTexteCentre($"File name cannot be null or empty., {nameof(fileName)}");

        string fullPath = Path.Combine(directoryPath, fileName);
        return !File.Exists(fullPath);
    }


    public static void AddDataToXml(string filePath, Dictionary<string, string> data)
    {
        XDocument doc;

        if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
        {
            // File exists and is not empty
            doc = XDocument.Load(filePath);
        }
        else
        {
            // File does not exist or is empty → create new XML with root
            doc = new XDocument(new XElement("Root"));
        }

        // Create new element for this data
        XElement newElement = new XElement("Record");
        foreach (var kvp in data)
        {
            newElement.Add(new XElement(kvp.Key, kvp.Value ?? string.Empty));
        }

        // Add and save
        doc.Root.Add(newElement);
        doc.Save(filePath);

        Console.WriteLine("Data added successfully!");
    }



    #endregion

    #region Encryption

    /// <summary>
    /// Encrypts a file using AES encryption with a random IV.
    /// </summary>
    /// <param name="inputFilePath"></param>
    /// <param name="outputFilePath"></param>
    /// <param name="password"></param>
    public static void EncryptFile(string inputFilePath, string outputFilePath, string password)
    {
        try
        {
            byte[] key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password));

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV(); // Random IV

                using (FileStream outFs = new FileStream(outputFilePath, FileMode.Create))
                {
                    // Write the IV at the beginning of the file
                    outFs.Write(aes.IV, 0, aes.IV.Length);

                    using (CryptoStream cryptoStream = new CryptoStream(outFs, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (FileStream inFs = new FileStream(inputFilePath, FileMode.Open))
                    {
                        inFs.CopyTo(cryptoStream);
                    }
                }
            }

            Console.WriteLine("File encrypted with random IV.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Encryption error: " + ex.Message);
        }
    }


    /// <summary>
    /// Decrypts a file using AES decryption with the IV embedded in the file.
    /// </summary>
    /// <param name="inputFilePath"></param>
    /// <param name="outputFilePath"></param>
    /// <param name="password"></param>
    public static void DecryptFile(string inputFilePath, string outputFilePath, string password)
    {
        try
        {
            byte[] key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password));

            using (FileStream inFs = new FileStream(inputFilePath, FileMode.Open))
            {
                byte[] iv = new byte[16];
                inFs.Read(iv, 0, iv.Length); // Read IV from the start of the file

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;

                    using (CryptoStream cryptoStream = new CryptoStream(inFs, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (FileStream outFs = new FileStream(outputFilePath, FileMode.Create))
                    {
                        cryptoStream.CopyTo(outFs);
                    }
                }
            }

            Console.WriteLine("File decrypted using embedded IV.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Decryption error: " + ex.Message);
        }
    }


    /// <summary>
    /// Gets a StreamReader for a decrypted file using AES decryption with the IV embedded in the file.
    /// </summary>
    /// <param name="encryptedFilePath"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static StreamReader GetDecryptedStreamReader(string encryptedFilePath, string password)
    {
        byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(password));

        FileStream encryptedFileStream = new FileStream(encryptedFilePath, FileMode.Open, FileAccess.Read);

        // Read IV from beginning of file
        byte[] iv = new byte[16];
        int bytesRead = encryptedFileStream.Read(iv, 0, iv.Length);
        if (bytesRead != iv.Length)
        {
            encryptedFileStream.Dispose();
            throw new Exception("Could not read IV from encrypted file.");
        }

        Aes aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        CryptoStream cryptoStream = new CryptoStream(encryptedFileStream, aes.CreateDecryptor(), CryptoStreamMode.Read);

        return new StreamReader(cryptoStream, Encoding.UTF8);
    }

    /// <summary>
    /// Decrypts encryptedFilePath (IV prepended) to a temp file and returns that temp file path.
    /// </summary>
    /// <param name="encryptedFilePath"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    // Decrypts encryptedFilePath (IV prepended) to a temp file and returns that temp file path.
    // Caller must delete the returned file when done.
    public static string DecryptToTempFile(string encryptedFilePath, string password)
    {
        //string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".tmp");
        string tempPath = Path.Combine(Path.GetTempPath(), "humans.csv");

        byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(password));

        using (FileStream inFs = new FileStream(encryptedFilePath, FileMode.Open, FileAccess.Read))
        {
            // Read IV (first 16 bytes)
            byte[] iv = new byte[16];
            int read = inFs.Read(iv, 0, iv.Length);
            if (read != iv.Length)
                throw new InvalidOperationException("Encrypted file missing IV or corrupt.");

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                // create temp file and write decrypted content to it
                using (FileStream outFs = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                using (CryptoStream cryptoStream = new CryptoStream(inFs, aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    cryptoStream.CopyTo(outFs);
                }
            }
        }

        return tempPath;
    }



    #endregion

    #endregion

    #region Affichage de texte

    /// <summary>
    /// Affiche une ligne de texte lettre par lettre.
    /// </summary>
    /// <param name="texte">Le texte à afficher.</param>
    /// <param name="couleur">La couleur console du texte, par défaut White.</param>
    public static void AfficherLigne(string texte, ConsoleColor couleur = ConsoleColor.White)
    {
        Console.ForegroundColor = couleur;
        Console.WriteLine(texte);
        Console.ResetColor();
    }

    public static void AfficherTexte(string texte, ConsoleColor couleur = ConsoleColor.White)
    {
        Console.ForegroundColor = couleur;
        Console.Write(texte);
        Console.ResetColor();
    }

    /// <summary>
    /// Affiche un texte lettre par lettre.
    /// </summary>
    /// <param name="texte">Le texte à afficher.</param>
    /// <param name="couleur">La couleur console du texte, par défaut White.</param>
    public static void AfficherTexteProgressif(string texte, ConsoleColor couleur = ConsoleColor.White)
    {
        Console.ForegroundColor = couleur;

        foreach (char lettre in texte)
        {
            Thread.Sleep(25);
            Console.Write(lettre);
        }

        Console.ResetColor();
    }

    public static void AfficherTexteProgressifCentre(string texte, bool cursorTop = true, ConsoleColor couleur = ConsoleColor.White, int SleepTime = 25)
    {
        Console.ForegroundColor = couleur;
        int consoleWidth = Console.WindowWidth;
        int startPosition = (consoleWidth - texte.Length) / 2;

        if (startPosition < 0) startPosition = 0;

        int currentPosition = startPosition;

        int line = Console.CursorTop;

        foreach (char lettre in texte)
        {
            if (cursorTop)
                Console.SetCursorPosition(currentPosition, line);
            else
                Console.SetCursorPosition(currentPosition, Console.WindowHeight / 2);
            Console.Write(lettre);
            currentPosition++;
            Thread.Sleep(SleepTime);
        }
        Console.WriteLine();
        Console.ResetColor();
    }



    /// <summary>
    /// Affiche un titre entre 2 lignes de n asterisques.
    /// </summary>
    /// <param name="titre">Le titre à afficher.</param>
    public static void AfficherTitre(string titre, int nbAsterisques = 30, int letterPause = 25, ConsoleColor couleur = ConsoleColor.Cyan, bool centre = true, bool consoleClear = true)
    {
        if (consoleClear) Console.Clear();

        // Déclaration des variables
        string asterisques = "".PadLeft(nbAsterisques, '*');

        // Centrage du titre


        // Affichage de l'entête
        Console.ResetColor();
        Console.ForegroundColor = couleur;

        if (centre)
        {
            AfficherTexteCentre(asterisques);
            AfficherTexteProgressifCentre(titre.ToUpper(), true, ConsoleColor.White, 50);
            Console.ForegroundColor = couleur;
            AfficherTexteCentre(asterisques);
        }
        else
        {
            Console.WriteLine(asterisques);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(titre.ToUpper());
            Console.ForegroundColor = couleur;
            Console.WriteLine(asterisques);
        }

        Console.WriteLine();
        Console.ResetColor();
    }


    /// <summary>
    /// Affiche un texte centré horizontalement à la ligne courante.
    /// </summary>
    public static void AfficherTexteCentre(string texte)
    {
        int col = (Console.WindowWidth - texte.Length) / 2;
        int row = Console.CursorTop; // ligne courante
        Console.SetCursorPosition(Math.Max(0, col), row);
        Console.WriteLine(texte);
        Console.ResetColor();
    }

    /// <summary>
    /// Affiche un texte centré horizontalement à la ligne courante ou à une ligne spécifiée.
    /// </summary>
    public static void AfficherTexteCentre(string texte, int? ligne = null)
    {
        int col = (Console.WindowWidth - texte.Length) / 2;
        int row = ligne ?? Console.CursorTop;
        Console.SetCursorPosition(Math.Max(0, col), Math.Max(0, row));
        Console.WriteLine(texte);
    }


    #endregion

    #region Lecture de texte


    public static string LireAdresseIP(string message)
    {
        string? ip;

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(message);
            Console.ResetColor();
            ip = Console.ReadLine().Trim() ?? "";

            if (IPAddress.TryParse(ip, out _))
                return ip!.Trim();

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Adresse IP invalide ! Recommencez.");
            Console.ResetColor();
        }
    }

    /// <summary>
    /// Lit un caractère dans la console.
    /// </summary>
    /// <returns>Le caractère lu dans la console.</returns>
    public static char LireCaractere()
    {
        return LireCaractere("");
    }

    /// <summary>
    /// Lit un caractère dans la console.
    /// </summary>
    /// <param name="message">Le message affiché avant de lire un caractère dans la console.</param>
    /// <returns>Le caractère lu dans la console.</returns>
    public static char LireCaractere(string message)
    {
        Console.Write(message);
        return Console.ReadKey().KeyChar;
        Console.WriteLine();

    }

    public static char LireChiffre(string message, int maxIncluded = 9)
    {
        ConsoleKeyInfo key;
        AfficherTexte("\n" + message, ConsoleColor.Yellow);
        key = Console.ReadKey();
        Console.WriteLine();

        while (!(key.KeyChar >= '0' && key.KeyChar <= maxIncluded.ToString()[0]))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Appuyez sur un numéro ! Recommencez : ");
            Console.ResetColor();
            key = Console.ReadKey();
            Console.WriteLine();
        }

        Console.WriteLine();
        return key.KeyChar;
    }

    /// <summary>
    /// Lit un texte dans la console.
    /// </summary>
    /// <param name="message">Le message affiché avant de lire le texte dans la console.</param>
    /// <param name="messageInvalide">Le message à afficher lorsque l'lue est invalide.</param>
    /// <param name="accepteVide">Accepte un texte vide. Par défaut VRAI.</param>
    /// <returns>Le texte lu dans la console.</returns>
    public static string LireTexte(string message, string messageInvalide = "", bool accepteVide = true)
    {
        string reponse;

        Console.Write(message);
        reponse = Console.ReadLine() ?? "";

        if (accepteVide == false)
        {
            while (reponse.Trim() == "")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(messageInvalide);
                Console.ResetColor();
                reponse = Console.ReadLine() ?? "";
            }
        }

        return reponse;
    }

    /// <summary>
    /// Lit un texte non vide dans la console.
    /// </summary>
    /// <returns>Le texte non vide lu dans la console.</returns>
    public static string LireTexteNonVide()
    {
        return LireTexteNonVide("");
    }

    /// <summary>
    /// Lit un texte non vide dans la console.
    /// </summary>
    /// <param name="message">Le message affiché avant de lire le texte non vide dans la console.</param>
    /// <returns>Le texte non vide lu dans la console.</returns>
    public static string LireTexteNonVide(string message)
    {
        string texte;

        Console.Write(message);
        texte = Console.ReadLine() ?? "";
        texte = texte.Trim();

        while (texte == "")
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Valeur obligatoire, recommencez : ");
            Console.ResetColor();
            texte = Console.ReadLine() ?? "";
        }

        return texte;
    }

    public static string LireTexteCentre(int ligne)
    {
        string texte = "";
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true); // true = n'affiche pas la touche

            if (key.Key == ConsoleKey.Backspace && texte.Length > 0)
            {
                texte = texte.Substring(0, texte.Length - 1);
            }
            else if (!char.IsControl(key.KeyChar))
            {
                texte += key.KeyChar;
            }

            // Efface la ligne
            Console.SetCursorPosition(0, ligne);
            Console.Write(new string(' ', Console.WindowWidth));

            // Affiche le texte centré
            int col = (Console.WindowWidth - texte.Length) / 2;
            Console.SetCursorPosition(Math.Max(0, col), ligne);
            Console.Write(texte);

        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine(); // passe à la ligne
        return texte;
    }


    #endregion

    #region string manipulation

    /// <summary>
    /// Convertit un tableau de chaînes de caractères en une seule chaîne de caractères, les champs étant séparés par des virgules.
    /// </summary>
    /// <param name="fieldLine"></param>
    /// <returns></returns>
    public static string FieldsToString(string[] fieldLine)
    {
        string joinedLine = string.Join(",", fieldLine).Trim();
        return joinedLine;

    }

    #endregion

    #region Lecture de double

    /// <summary>
    /// Lit un nombre réel (double) valide dans la console.
    /// </summary>
    /// <returns>Le nombre réel (double) lu dans la console.</returns>
    public static double LireDouble()
    {
        return LireDouble("");
    }

    /// <summary>
    /// Lit un nombre réel (double) valide dans la console.
    /// </summary>
    /// <param name="message">Le message affiché avant de lire le nombre réel dans la console.</param>
    /// <returns>Le nombre réel (double) lu dans la console.</returns>
    public static double LireDouble(string message)
    {
        return LireDouble(message, "Nombre réel invalide, recommencez : ");
    }

    /// <summary>
    /// Lit un nombre réel (double) valide dans la console.
    /// </summary>
    /// <param name="message">Le message affiché avant de lire le nombre réel dans la console.</param>
    /// <param name="messageInvalide">Le message à afficher lorsque l'lue est invalide.</param>
    /// <returns>Le nombre réel (double) lu dans la console.</returns>
    public static double LireDouble(string message, string messageInvalide)
    {
        bool valide;

        Console.Write(message);
        valide = double.TryParse(Console.ReadLine(), out double reel);

        while (valide == false)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(messageInvalide);
            Console.ResetColor();
            valide = double.TryParse(Console.ReadLine(), out reel);
        }

        return reel;
    }

    /// <summary>
    /// Lit un double valide dans la console.
    /// </summary>
    /// <param name="min">La valeur minimale valide.</param>
    /// <param name="max">La valeur maximale valide.</param>
    /// <returns>Le nombre réel (double) lu dans la console.</returns>
    public static double LireDoubleMinMax(double min, double max)
    {
        return LireDoubleMinMax("", min, max);
    }

    /// <summary>
    /// Lit un double valide dans la console.
    /// </summary>
    /// <param name="message">Le message affiché avant de lire l'entier dans la console.</param>
    /// <param name="min">La valeur minimale valide.</param>
    /// <param name="max">La valeur maximale valide.</param>
    /// <returns>Le nombre réel (double) lu dans la console.</returns>
    public static double LireDoubleMinMax(string message, double min, double max)
    {
        return LireDoubleMinMax(message, $"Cette valeur est invalide ({min} à {max}), recommencez : ", min, max);
    }

    /// <summary>
    /// Lit un double valide dans la console.
    /// </summary>
    /// <param name="message">Le message affiché avant de lire l'entier dans la console.</param>
    /// <param name="messageInvalide">Le message à afficher lorsque l'lue est invalide.</param>
    /// <param name="min">La valeur minimale valide.</param>
    /// <param name="max">La valeur maximale valide.</param>
    /// <returns>Le nombre réel (double) lu dans la console.</returns>
    public static double LireDoubleMinMax(string message, string messageInvalide, double min, double max)
    {
        double reel;
        bool valide;

        Console.Write(message);
        valide = double.TryParse(Console.ReadLine(), out reel);

        while (valide == false || reel < min || reel > max)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(messageInvalide);
            Console.ResetColor();
            valide = double.TryParse(Console.ReadLine(), out reel);
        }

        return reel;
    }

    /// <summary>
    /// Lit un double plus grand que zéro dans la console.
    /// </summary>
    /// <param name="message">Le message à afficher.</param>
    /// <returns>Le nombre réel (double) lu dans la console.</returns>
    public static double LireDoublePositif(string message)
    {
        return LireDoublePositif(message, "Cette valeur est invalide. Le nombre doit être plus grand que zéro. Recommencez :");
    }

    /// <summary>
    /// Lit un double plus grand que zéro dans la console.
    /// </summary>
    /// <param name="message">Le message à afficher.</param>
    /// <param name="messageInvalide">Le message à afficher lorsque l'lue est invalide.</param>
    /// <returns>Le nombre réel (double) lu dans la console.</returns>
    public static double LireDoublePositif(string message, string messageInvalide)
    {
        // Déclaration des variables
        double reel;
        bool valide;

        // Écriture du message à la console
        Console.Write(message);
        valide = double.TryParse(Console.ReadLine(), out reel);

        while (valide == false || reel <= 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(messageInvalide);
            Console.ResetColor();
            valide = double.TryParse(Console.ReadLine(), out reel);
        }

        return reel;
    }

    /// <summary>
    /// Lit un nombre de doubles valides dans la console.
    /// </summary>
    /// <param name="nbDoubles">Nombre de doubles à lire.</param>
    /// <returns>Le nombre réel (double) lu dans la console.</returns>
    public static List<double> LireDoubles(int nbDouble)
    {
        return LireDoubles(nbDouble);
    }

    /// <summary>
    /// Lit un nombre de doubles valides dans la console.
    /// </summary>
    /// <param name="message">Le message affiché avant de lire l'entier dans la console.</param>
    /// <param name="nbDoubles">Nombre de doubles à lire.</param>
    /// <returns>Les nombres réels (double) lus dans la console.</returns>
    public static List<double> LireDoubles(string message, int nbDoubles)
    {
        List<double> doubles = new List<double>();

        Console.WriteLine(message);

        for (double index = 1; index <= nbDoubles; index++)
        {
            double reel = LireDouble($"Entier #{index} : ");
            doubles.Add(reel);
        }

        return doubles;
    }


    /// <summary>
    /// Lit un nombre de doubles valides dans la console.
    /// </summary>
    /// <param name="message">Le message affiché avant de lire l'entier dans la console.</param>
    /// <param name="nbDoubles">Nombre de doubles à lire.</param>
    /// <returns>Les nombres réels (double) lus dans la console.</returns>
    public static List<double> LireDoublesMinMax(string message, int nbDoubles, double min, double max)
    {
        List<double> doubles = new List<double>();

        Console.WriteLine(message);

        for (double index = 1; index <= nbDoubles; index++)
        {
            double reel = LireDoubleMinMax($"Entier #{index} : ", min, max);
            doubles.Add(reel);
        }

        return doubles;
    }


    /// <summary>
    /// Lit un nombre de doubles valides dans la console.
    /// </summary>
    /// <param name="message">Le message affiché avant de lire l'entier dans la console.</param>
    /// <param name="nbDoubles">Nombre de doubles à lire.</param>
    /// <returns>Les nombres réels (double) lus dans la console.</returns>
    public static List<double> LireDoublesPositifs(string message, int nbDoubles)
    {
        List<double> doubles = new List<double>();

        Console.WriteLine(message);

        for (double index = 1; index <= nbDoubles; index++)
        {
            double reel = LireDoublePositif($"Entier #{index} : ");
            doubles.Add(reel);
        }

        return doubles;
    }

    #endregion

    #region LireEntier

    /// <summary>
    /// Lit un entier valide dans la console.
    /// </summary>
    /// <returns>L'entier lu dans la console.</returns>
    public static int LireEntier()
    {
        return LireEntier("");
    }

    /// <summary>
    /// Lit un entier valide dans la console.
    /// </summary>
    /// <param name="message">Le message affiché avant de lire l'entier dans la console.</param>
    /// <returns>L'entier lu dans la console.</returns>
    public static int LireEntier(string message)
    {
        return LireEntier(message, "Nombre entier invalide, recommencez : ");
    }

    /// <summary>
    /// Lit un entier valide dans la console.
    /// </summary>
    /// <param name="message">Le message affiché avant de lire l'entier dans la console.</param>
    /// <param name="messageInvalide">Le message à afficher lorsque l'lue est invalide.</param>
    /// <returns>L'entier lu dans la console.</returns>
    public static int LireEntier(string message, string messageInvalide)
    {
        int entier;
        bool valide;

        Console.Write(message);
        valide = int.TryParse(Console.ReadLine(), out entier);

        while (valide == false)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(messageInvalide);
            Console.ResetColor();
            valide = int.TryParse(Console.ReadLine(), out entier);
        }

        return entier;
    }

    /// <summary>
    /// Lit un entier valide dans la console.
    /// </summary>
    /// <param name="min">La valeur minimale valide.</param>
    /// <param name="max">La valeur maximale valide.</param>
    /// <returns>L'entier lu dans la console.</returns>
    public static int LireEntierMinMax(int min, int max)
    {
        return LireEntierMinMax("", min, max);
    }

    /// <summary>
    /// Lit un entier valide dans la console.
    /// </summary>
    /// <param name="message">Le message affiché avant de lire l'entier dans la console.</param>
    /// <param name="min">La valeur minimale valide.</param>
    /// <param name="max">La valeur maximale valide.</param>
    /// <returns>L'entier lu dans la console.</returns>
    public static int LireEntierMinMax(string message, int min, int max)
    {
        return LireEntierMinMax(message, $"Nombre entier invalide ({min} à {max}), recommencez : ", min, max);
    }

    /// <summary>
    /// Lit un entier valide dans la console.
    /// </summary>
    /// <param name="message">Le message affiché avant de lire l'entier dans la console.</param>
    /// <param name="messageInvalide">Le message à afficher lorsque l'lue est invalide.</param>
    /// <param name="min">La valeur minimale valide.</param>
    /// <param name="max">La valeur maximale valide.</param>
    /// <returns>L'entier lu dans la console.</returns>
    public static int LireEntierMinMax(string message, string messageInvalide, int min, int max)
    {
        int entier;
        bool valide;

        Console.Write(message);
        valide = int.TryParse(Console.ReadLine(), out entier);

        while (valide == false || entier < min || entier > max)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(messageInvalide);
            Console.ResetColor();
            valide = int.TryParse(Console.ReadLine(), out entier);
        }

        return entier;
    }

    /// <summary>
    /// Lit un nombre entier plus grand que zéro dans la console.
    /// </summary>
    /// <param name="message">Le message à afficher.</param>
    /// <returns>Le nombre entier lu dans la console.</returns>
    public static int LireEntierPositif(string message)
    {
        return LireEntierPositif(message, "Le nombre entier saisi est invalide. Il doit être plus grand que zéro. Recommencez :");
    }

    /// <summary>
    /// Lit un nombre entier plus grand que zéro dans la console.
    /// </summary>
    /// <param name="message">Le message à afficher.</param>
    /// <param name="messageInvalide">Le message à afficher lorsque l'lue est invalide.</param>
    /// <returns>Le nombre entier lu dans la console.</returns>
    public static int LireEntierPositif(string message, string messageInvalide)
    {
        // Déclaration des variables
        int entier;
        bool valide;

        // Écriture du message à la console
        Console.Write(message);
        valide = int.TryParse(Console.ReadLine(), out entier);

        while (valide == false || entier <= 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(messageInvalide);
            Console.ResetColor();
            valide = int.TryParse(Console.ReadLine(), out entier);
        }

        return entier;
    }

    /// <summary>
    /// Lit un entier valide dans la console.
    /// </summary>
    /// <param name="nbEntiers">Nombre d'entiers à lire.</param>
    /// <returns>L'entier lu dans la console.</returns>
    public static List<int> LireEntiers(int nbEntiers)
    {
        return LireEntiers(nbEntiers);
    }

    /// <summary>
    /// Lit un nombre d'entiers valides dans la console.
    /// </summary>
    /// <param name="message">Le message affiché avant de lire l'entier dans la console.</param>
    /// <param name="nbEntiers">Nombre d'entiers à lire.</param>
    /// <returns>L'entier lu dans la console.</returns>
    public static List<int> LireEntiers(string message, int nbEntiers)
    {
        List<int> entiers = new List<int>();

        Console.WriteLine(message);

        for (int index = 1; index <= nbEntiers; index++)
        {
            int entier = LireEntier($"Entier #{index} : ");
            entiers.Add(entier);
        }

        return entiers;
    }

    /// <summary>
    /// Lit un nombre d'entiers valides dans la console.
    /// </summary>
    /// <param name="message">Le message affiché avant de lire l'entier dans la console.</param>
    /// <param name="nbEntiers">Nombre d'entiers à lire.</param>
    /// <returns>L'entier lu dans la console.</returns>
    public static List<int> LireEntiersMinMax(string message, int nbEntiers, int min, int max)
    {
        List<int> entiers = new List<int>();

        Console.WriteLine(message);

        for (int index = 1; index <= nbEntiers; index++)
        {
            int entier = LireEntierMinMax($"Entier #{index} : ", min, max);
            entiers.Add(entier);
        }

        return entiers;
    }


    /// <summary>
    /// Lit un nombre d'entiers valides dans la console.
    /// </summary>
    /// <param name="message">Le message affiché avant de lire l'entier dans la console.</param>
    /// <param name="nbEntiers">Nombre d'entiers à lire.</param>
    /// <returns>L'entier lu dans la console.</returns>
    public static List<int> LireEntiersPositifs(string message, int nbEntiers)
    {
        List<int> entiers = new List<int>();

        Console.WriteLine(message);

        for (int index = 1; index <= nbEntiers; index++)
        {
            int entier = LireEntierPositif($"Entier #{index} : ");
            entiers.Add(entier);
        }

        return entiers;
    }


    #endregion

    #region Aesthetic

    /// <summary>
    /// Cammoufle le texte affiché à la console à la fin de l'excéution.
    /// </summary>
    public static void MasquerTexte()
    {
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Black;
    }

    /// <summary>
    /// Affiche un titre encadré dans la console.
    /// </summary>
    /// <param name="title"></param>
    public static void DisplayTitle(string title)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=====================================");
        Console.WriteLine($"   {title}");
        Console.WriteLine("=====================================\n");
        Console.ResetColor();
    }


    /// <summary>
    /// affiche un effet de chargement dans la console.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="c"></param>
    public static void LoadingEffect(string message, ConsoleColor c)
    {
        Console.ForegroundColor = c;
        Console.Write($"\n{message}");
        for (int i = 0; i < 3; i++)
        {
            Thread.Sleep(500);
            Console.Write(".");
        }
        Console.ResetColor();
        Console.WriteLine("\n");
    }

    /// <summary>
    /// Attend la pression d'une touche pour continuer.
    /// </summary>
    public static void WaitKey(bool? isCentered = false)
    {
        if (isCentered is true)
        {
            AfficherTexteProgressifCentre("\nAppuyez sur une touche pour continuer...", true, ConsoleColor.DarkGray);
        }

        else
        {
            AfficherTexteProgressif("\nAppuyez sur une touche pour continuer...", ConsoleColor.DarkGray);
        }
        Console.ReadKey();
        Console.WriteLine();
    }

    #endregion

    #region Regex

    /// <summary>
    ///  Vérifie si le format du numéro de téléphone est valide.
    /// </summary>
    /// <param name="phone"></param>
    /// <returns></returns>
    public static bool IsValidPhone(string phone)
    {
        return !Regex.IsMatch(phone, @"^0\d{9}$"); // French phone format
    }


    #endregion

    #region DataScraper

    public static string ObtenirLocalisation()
    {
        try
        {
            using (var client = new HttpClient())
            {
                var result = client.GetStringAsync("https://ipinfo.io/json").Result;
                var doc = JsonDocument.Parse(result);
                string ville = doc.RootElement.GetProperty("city").GetString() ?? "Inconnue";
                string region = doc.RootElement.GetProperty("region").GetString() ?? "";
                string pays = doc.RootElement.GetProperty("country").GetString() ?? "";
                return $"{ville}, {region}, {pays}";
            }
        }
        catch
        {
            return "Localisation non disponible";
        }
    }

    #endregion

    #region CursorPosition
    /// <summary>
    /// Centre le curseur horizontalement sur une ligne donnée (ou la ligne courante si null).
    /// </summary>
    public static void CentrerCurseur(int? ligne = null)
    {
        int row = ligne ?? Console.CursorTop;            // ligne courante par défaut
        int col = Console.WindowWidth / 2;               // milieu de la console
        Console.SetCursorPosition(Math.Max(0, col), row);
    }

    #endregion


}



