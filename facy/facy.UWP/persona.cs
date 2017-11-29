using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Face.Contract;
using FireSharp.Interfaces;
using FireSharp.Config;
using FireSharp;
using FireSharp.Response;

namespace facy.UWP
{
    class persona
    {
        double age;
        EmotionScores emotion;
        string gender;
        string View;
        double[] emociones;
        string emocionSuperior;
        String mesDB;
        String diaDB;
        Glasses glasses;
        DateTime thisDay = DateTime.Today;
        string query;
        double Anger, Contempt, Disgust, Fear, Happiness, Neutral, Sadness, Surprise;
        string rangoEdad;
        int totalCountEmocion;
        int totalCountLentes;
        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "brrxdTuuvZYociZt8FVWU7GN4fCakbhEVx7O5UPi",
            BasePath = "https://facy-7edfb.firebaseio.com/"
        };

        public persona(double age, string gender, Glasses glasses, EmotionScores emotion)
        {
            this.age = age;
            this.gender = gender;
            this.glasses = glasses;
            this.emotion = emotion;
            query = "cliente/" + thisDay.Year + "/" + thisDay.Month + "/" + thisDay.Day;
            //ActualizarFechaDB();
            Iniciar();
            
           
            
        }
        public async void Iniciar()
        {
           await categorizarDatos();
           await GetData(query);
        }

       
      
        private async Task categorizarDatos()
        {
            rangoEdad = age > 0 && age <= 15 ? "Edad 0-15" :
                       age > 15 && age <= 22 ? "Edad 16-22" :
                       age > 22 && age <= 30 ? "Edad 23-30" :
                       age > 30 && age <= 40 ? "Edad 31-40" :
                       age > 40 && age <= 50 ? "Edad 41-50" :
                       age > 50 && age <= 100 ? "Edad 50 o mas":null;

            emociones = new double[8];
            emociones[0] = Convert.ToDouble(emotion.Anger);
            emociones[1] = Convert.ToDouble(emotion.Contempt);
            emociones[2] = Convert.ToDouble(emotion.Disgust);
            emociones[3] = Convert.ToDouble(emotion.Fear);
            emociones[4] = Convert.ToDouble(emotion.Happiness);
            emociones[5] = Convert.ToDouble(emotion.Neutral);
            emociones[6] = Convert.ToDouble(emotion.Sadness);
            emociones[7] = Convert.ToDouble(emotion.Surprise);

            int indiceMayor1 = 0;
            for(int i = 0; i < emociones.Length; i++)
            {
                if (i + 1 <8)
                {
                    indiceMayor1 = emociones[indiceMayor1] >= emociones[i] ? indiceMayor1 : i ;
                }
            }

            emocionSuperior = emocionGrande(indiceMayor1);


        }

        public string emocionGrande(int indiceMayor)
        {
            string emocionMayor="";

            switch (indiceMayor)
            {
                case 0:
                    emocionMayor = "Anger";
                    break;
                case 1:
                    emocionMayor = "Contempt";
                    break;
                case 2:
                    emocionMayor = "Disgust";
                    break;
                case 3:
                    emocionMayor = "Fear";
                    break;
                case 4:
                    emocionMayor = "Happiness";
                    break;
                case 5:
                    emocionMayor = "Neutral";
                    break;
                case 6:
                    emocionMayor = "Sadness";
                    break;
                case 7:
                    emocionMayor = "Surprise";
                    break;

            }
            return emocionMayor;
        }
        
        private async Task GetData(string query)
        {
            IFirebaseClient client = new FirebaseClient(config);

            FirebaseResponse response = await client.GetAsync(query+"/views/viewCount");
            int totalViews;
           View = response.ResultAs<string>();
            totalViews = View!=null ?Convert.ToInt32(View.ToString()):0;

            var todo = new newView

            {
                viewCount = totalViews + 1
            };
            FirebaseResponse response2 = await client.UpdateAsync(query+"/views",todo);

            FirebaseResponse response3 = await client.GetAsync(query + "/" + gender + "/" + rangoEdad + "/" + emocionSuperior + "/total");
            string countEmocion =response3.ResultAs<string>();
            totalCountEmocion = countEmocion != null ? Convert.ToInt32(countEmocion) : 0;
            var todo2 = new newEmocion

            {
                total = totalCountEmocion + 1
            };
            FirebaseResponse response4 = await client.UpdateAsync(query + "/" + gender + "/" + rangoEdad + "/" + emocionSuperior, todo2);

            FirebaseResponse response5 = await client.GetAsync(query + "/" + gender + "/" + rangoEdad + "/" + glasses + "/total");
            string countLentes = response5.ResultAs<string>();
            totalCountLentes = countLentes != null ? Convert.ToInt32(countEmocion) : 0;
             var todo3 = new anteojos

            {
                total = totalCountLentes + 1
            };
            FirebaseResponse response6= await client.UpdateAsync(query + "/" + gender + "/" + rangoEdad + "/" + glasses, todo3);
        }

        

        private async void ActualizarFechaDB()
        {
            IFirebaseClient client = new FirebaseClient(config);
            String f = Convert.ToString(thisDay);

            var todo = new Todo // se crea un objeto de la clase Todo solo es un set and get
            {
                fechaDB =f,
            };
            FirebaseResponse response = await client.UpdateAsync("FechaActual/fechaDB", todo);
        }




    }
}
