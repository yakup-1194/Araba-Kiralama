using RentACar.Models;
using RentACar.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RentACar.Auth
{
    public class CustomerService
    {
        RentACarDbEntities db = new RentACarDbEntities();

        public CustomerModel customerLogin(string username,string password)
        {
            CustomerModel customer = db.Customer.Where(s => s.customerUsername == username && s.customerPassword == password).Select(x => 
            new CustomerModel() {
                 customerMail = x.customerMail,
                 customerName = x.customerName,
                 customerBirthDate = x.customerBirthDate,
                 customerId = x.customerId,
                 customerPassword = x.customerPassword,
                 customerPhoto = x.customerPhoto,
                 customerCarCount = x.Rental.Count(),
                 customerUsername = x.customerUsername,
                 Admin = x.Admin
                    
            }).SingleOrDefault();
            return customer;
        }
    }
}