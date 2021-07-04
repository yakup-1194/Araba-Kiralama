using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RentACar.ViewModel
{
    public class RentalModel
    {
        public string rentalId { get; set; }
        public string recordCarId { get; set; }
        public string recordCustomerId { get; set; }
        public System.DateTime rentDate { get; set; }
        public System.DateTime returnDate { get; set; }

        public CustomerModel customerInformation { get; set; }
        public CarModel carInformation { get; set; }

    }
}