using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RentACar.ViewModel
{
    public class CarModel
    {
        
        public string carId { get; set; }
        public string modelName { get; set; }
        public string modelYear { get; set; }
        public int dailyPrice { get; set; }
        public string description { get; set; }
        public int customerCount { get; set; }
        public string carPhoto { get; set; }
    }
}