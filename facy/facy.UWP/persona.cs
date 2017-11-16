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
        Glasses glasses;
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
            SubirDB();
        }

        private async void SubirDB()
        {
            IFirebaseClient client = new FirebaseClient(config);
            var todo = new Todo // se crea un objeto de la clase Todo solo es un set and get
            {
                name = "Execute PUSH", //Contenido del push
                priority = 2
            };
            PushResponse response = await client.PushAsync("todos/push", todo); //(path,datos)
        }


    }
}
