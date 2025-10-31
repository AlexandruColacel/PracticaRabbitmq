namespace AppForSEII2526.API.DTOs.DeviceDTO
{
    public class DeviceParaCompraDTOs
    {
        public DeviceParaCompraDTOs(int Id, string nombre, string marca, string modelo, string color, double precio)
        {
            id = Id;
            Name = nombre;
            Brand = marca;
            Model = modelo;
            Color = color;
            PriceForPurchase = precio;
        }
        //Revisar si es necesario añadir Required o StringLength
        public int id { get; set; }
        public string Name { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public string Color { get; set; }

        public double PriceForPurchase { get; set; }

    }//De clas DeviceDTOs
}//De namespace
