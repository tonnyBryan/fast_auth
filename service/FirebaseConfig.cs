using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace fast_auth.service
{
    public class FirebaseConfig
    {
        public static void InitializeFirebase()
        {
            string pathToJson = "firebase.json";

            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(pathToJson)
            });

            Console.WriteLine("Firebase a été initialisé avec succès !");
        }
    }
}
