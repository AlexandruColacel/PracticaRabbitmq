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

        public override bool Equals(object? obj)
        {
            //Primero, saber si trabajamos con la misma clase
            if (obj is not DeviceParaCompraDTOs dto)
            {
                return false;
            }

            //Segundo, comprobar si es la misma instancia (optimización rápida)
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            //ultimo, comparar los campos
            return id.Equals(dto.id) &&
                   object.Equals(Name, dto.Name) &&
                   object.Equals(Brand, dto.Brand) &&
                   object.Equals(Model, dto.Model) &&
                   object.Equals(Color, dto.Color) &&
                   PriceForPurchase.Equals(dto.PriceForPurchase);

        }//Override de Equals

        public override int GetHashCode()
        {
            return HashCode.Combine(id, Name, Brand, Model, Color, PriceForPurchase);
        }

    }//De clas DeviceDTOs
}//De namespace
