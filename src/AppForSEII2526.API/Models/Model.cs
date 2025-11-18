
using System;

namespace AppForSEII2526.API.Models
{

    public class Model
    {
        public Model() { 
        }
        public Model(String Name) {
            NameModel = Name;
        }

        public Model(string nameModel, List<Device> devices) : this(nameModel) {
            Devices = devices;
        }

        [Key]
        public int Id { get; set; }//Identificador único del modelo


        [StringLength(30, ErrorMessage = "El nombre del modelo no puede tener más de 30 caracteres")]
        public string NameModel { get; set; }//Nombre del modelo


        public List<Device> Devices { get; set; }//Relación uno a muchos con dispositivos
    }
}

