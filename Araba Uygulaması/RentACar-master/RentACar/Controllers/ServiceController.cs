using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RentACar.Models;
using RentACar.ViewModel;

namespace RentACar.Controllers
{
    public class ServiceController : ApiController
    {
        RentACarDbEntities db = new RentACarDbEntities();
        ResultModel result = new ResultModel();

        #region Car

        

        [HttpGet]
        [Route("api/listcar")]
        public List<CarModel> CarList()
        {
            List<CarModel> cars = db.Car.Select(car => new CarModel() 
            { 
                carId= car.carId,
                dailyPrice = car.dailyPrice,
                description = car.description,
                modelName = car.modelName,
                modelYear = car.modelYear,
                carPhoto = car.carPhoto,
                customerCount = car.Rental.Count()
            }).ToList();

            return cars;
        }

        [HttpGet]
        [Route("api/carbyid/{carId}")]
        public CarModel carById(string carId)
        {
            CarModel car = db.Car.Where(c => c.carId == carId).Select(x => new CarModel() 
            {
                carId = x.carId,
                dailyPrice = x.dailyPrice,
                description = x.description,
                modelName = x.modelName,
                modelYear = x.modelYear,
                carPhoto = x.carPhoto,
                customerCount = x.Rental.Count()
            }).SingleOrDefault();

            return car;
        }

        [HttpPost]
        [Route("api/addcar")]
        public ResultModel AddCar(CarModel car)
        {
            if (db.Car.Count(x => x.modelName == car.modelName) > 0)
            {
                result.success = false;
                result.message = "Bu araç zaten eklenmiş.";
                return result;
            }

            Car newCar = new Car();
            newCar.carId = Guid.NewGuid().ToString();
            newCar.dailyPrice = car.dailyPrice;
            newCar.description = car.description;
            newCar.modelName = car.modelName;
            newCar.modelYear = car.modelYear;
            newCar.carPhoto = car.carPhoto;
            db.Car.Add(newCar);
            db.SaveChanges();

            result.success = true;
            result.message = "Araç eklendi.";


            return result;
        }

        [HttpPut]
        [Route("api/updatecar")]
        public ResultModel UpdateCar(CarModel model)
        {
            Car car = db.Car.Where(s => s.carId == model.carId).SingleOrDefault();

            if (car == null)
            {
                result.success = false;
                result.message = "Araç bulunamadı.";
                return result;
            }

            car.carId = model.carId;
            car.dailyPrice = model.dailyPrice;
            car.description = model.description;
            car.modelName = model.modelName;
            car.modelYear = model.modelYear;
            car.carPhoto = model.carPhoto;

            db.SaveChanges();

            result.success = true;
            result.message = "Araç güncellendi.";

            return result;
        }

        [HttpDelete]
        [Route("api/deletecar/{carId}")]
        public ResultModel DeleteCar(string carId)
        {
            Car car = db.Car.Where(s => s.carId == carId).SingleOrDefault();

            if (car == null)
            {
                result.success = false;
                result.message = "Araç bulunamadı.";
                return result;
            }

            if (db.Rental.Count(s=>s.recordCarId == carId) > 0 )
            {
                result.success = false;
                result.message = "Araç bir kullanıcıya kiralandığı için silinemez.";
                return result;
            }

            db.Car.Remove(car);
            db.SaveChanges();

            result.success = true;
            result.message = "Araç silindi.";
            return result;
        }

        [HttpPost]
        [Route("api/updatecarphoto")]
        public ResultModel UpdateCarPhoto(CarPhotoModel model)
        {
            Car car = db.Car.Where(s => s.carId == model.carId).SingleOrDefault();
            if (car == null)
            {
                result.success = false;
                result.message = "Araç bulunamadı.";
                return result;
            }

            if (car.carPhoto != "placeholder.png")
            {
                string path = System.Web.Hosting.HostingEnvironment.MapPath("~/Files/"+car.carPhoto);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            string data = model.photoData;
            string base64 = data.Substring(data.IndexOf(',')+1);
            base64 = base64.Trim('\0');
            byte[] imagebytes = Convert.FromBase64String(base64);
            string fileName = car.carId + model.photoExtension.Replace("image/",".");

            using (var ms = new MemoryStream(imagebytes,0,imagebytes.Length))
            {
                Image image = Image.FromStream(ms,true);
                image.Save(System.Web.Hosting.HostingEnvironment.MapPath("~/Files/" + fileName));
            }
            car.carPhoto = fileName;
            db.SaveChanges();

            result.success = true;
            result.message = "Fotoğraf güncellendi.";
            
            return result;
        }
        #endregion

        #region Customer

        [HttpGet]
        [Route("api/listcustomer")]
        public List<CustomerModel> listCustomer()
        {
            List<CustomerModel> customers = db.Customer.Select(x => new CustomerModel()

            {
                customerId = x.customerId,
                customerBirthDate = x.customerBirthDate,
                customerMail = x.customerMail,
                customerName = x.customerName,
                customerCarCount = x.Rental.Count(),
                customerPhoto = x.customerPhoto,
                customerPassword = x.customerPassword,
                customerUsername = x.customerUsername,
                Admin = x.Admin


            }).ToList();
            return customers;
        }

        [HttpGet]
        [Route("api/customerbyid/{customerId}")]
        public CustomerModel customerById(string customerId)
        {
            CustomerModel customer = db.Customer.Where(s=>s.customerId==customerId).Select(x => new CustomerModel()

            {
                customerId = x.customerId,
                customerBirthDate = x.customerBirthDate,
                customerMail = x.customerMail,
                customerName = x.customerName,
                customerCarCount = x.Rental.Count(),
                customerPhoto = x.customerPhoto,
                customerUsername = x.customerUsername,
                customerPassword = x.customerPassword,
                Admin = x.Admin

            }).SingleOrDefault();
            return customer;
        }

        [HttpPost]
        [Route("api/addcustomer")]
        public ResultModel addCustomer(CustomerModel model)
        {
            if (db.Customer.Count(s=>s.customerMail == model.customerMail) > 0)
            {
                result.success = false;
                result.message = "Bu kullanıcı zaten sisteme kayıtlıdır.";
                return result;
            }

            Customer newCustomer = new Customer();
            newCustomer.customerId = Guid.NewGuid().ToString();
            newCustomer.customerBirthDate = model.customerBirthDate;
            newCustomer.customerMail = model.customerMail;
            newCustomer.customerName = model.customerName;
            newCustomer.customerPhoto = model.customerPhoto;
            newCustomer.customerUsername = model.customerUsername;
            newCustomer.customerPassword = model.customerPassword;
            newCustomer.Admin = model.Admin;

            db.Customer.Add(newCustomer);
            db.SaveChanges();

            result.success = true;
            result.message = "Kullanıcı eklendi.";
            return result;
        }

        [HttpPut]
        [Route("api/updatecustomer")]
        public ResultModel UpdateCustomer(CustomerModel model)
        {
            Customer customer = db.Customer.Where(s => s.customerId == model.customerId).SingleOrDefault();
            if (customer == null)
            {
                result.success = false;
                result.message = "Kullanıcı bulunamadı.";
                return result;
            }


            customer.customerBirthDate = model.customerBirthDate;
            customer.customerMail = model.customerMail;
            customer.customerName = model.customerName;
            customer.customerPhoto = model.customerPhoto;
            customer.customerPassword = model.customerPassword;
            customer.customerUsername = model.customerUsername;
            customer.Admin = model.Admin;

            db.SaveChanges();

            result.success = true;
            result.message = "Kullanıcı düzenlendi.";
            return result;
        }

        [HttpDelete]
        [Route("api/deletecustomer{customerId}")]
        public ResultModel DeleteCustomer(string customerId)
        {
            Customer customer = db.Customer.Where(s => s.customerId == customerId).SingleOrDefault();
            if (customer == null)
            {
                result.success = false;
                result.message = "Kullanıcı bulunamadı.";
                return result;
            }

            if (db.Rental.Count(s=>s.recordCustomerId==customerId) > 0 )
            {
                result.success = false;
                result.message = "Bu kullanıcı araç kiraladığı için şu an silemezsiniz.";
                return result;
            }

            db.Customer.Remove(customer);
            db.SaveChanges();

            result.success = true;
            result.message = "Kullanıcı silindi.";
            return result;
        }

        [HttpPost]
        [Route("api/updatecustomerphoto")]
        public ResultModel UpdateCustomerPhoto(CustomerPhotoModel model)
        {
            Customer customer = db.Customer.Where(s => s.customerId == model.customerId).SingleOrDefault();
            if(customer == null)
            {
                result.success = false;
                result.message = "Müşteri Bulunamadı.";
                return result;
            }

            if(customer.customerPhoto != "placeholder.png")
            {
                string path = System.Web.Hosting.HostingEnvironment.MapPath("~/Files/" + customer.customerPhoto);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            string data = model.photoData;
            string base64 = data.Substring(data.IndexOf(',') + 1);
            base64 = base64.Trim('\0');
            byte[] imgbytes = Convert.FromBase64String(base64);
            string fileName = customer.customerName + model.photoExtension.Replace("image/",".");
            using (var ms= new MemoryStream(imgbytes,0,imgbytes.Length))
            {
                Image img = Image.FromStream(ms,true);
                img.Save(System.Web.Hosting.HostingEnvironment.MapPath("~/Files/" + fileName));
            }
            customer.customerPhoto = fileName;
            db.SaveChanges();

            result.success = true;
            result.message = "Fotoğraf güncellendi.";
            return result;

        }
        #endregion

        #region RentalRecord
        [HttpGet]
        [Route("api/carsofcustomer/{customerId}")]
        public List<RentalModel> listCustomerRent(string customerId)
        {
            List<RentalModel> records = db.Rental.Where(x => x.recordCustomerId == customerId)
                .Select(x => new RentalModel()
                {
                    rentalId = x.rentalId,
                    recordCustomerId = x.recordCustomerId,
                    recordCarId = x.recordCarId,
                    rentDate = x.rentDate,
                    returnDate = x.returnDate

                }).ToList();

            foreach (var record in records)
            {
                record.customerInformation = customerById(record.recordCustomerId);
                record.carInformation = carById(record.recordCarId);
            }
            return records;
        }

        [HttpGet]
        [Route("api/customersofcar/{carId}")]
        public List<RentalModel> listCarRent(string carId)
        {
            List<RentalModel> records = db.Rental.Where(x => x.recordCarId == carId)
                .Select(x => new RentalModel()
                {
                    rentalId = x.rentalId,
                    recordCustomerId = x.recordCustomerId,
                    recordCarId = x.recordCarId,
                    rentDate = x.rentDate,
                    returnDate = x.returnDate

                }).ToList();

            foreach (var record in records)
            {
                record.customerInformation = customerById(record.recordCustomerId);
                record.carInformation = carById(record.recordCarId);
            }
            return records;
        }

        [HttpPost]
        [Route("api/addrental")]
        public ResultModel addRental(RentalModel model)
        {
            if (db.Rental.Count(x => x.recordCarId == model.recordCarId && x.recordCustomerId == model.recordCustomerId) > 0)
            {
                result.success = false;
                result.message = "Bu araç bu kullanıcıya zaten kiralanmış.";
                return result;
            }

            Rental newRental = new Rental();
            newRental.rentalId = Guid.NewGuid().ToString();
            newRental.recordCarId = model.recordCarId;
            newRental.recordCustomerId = model.recordCustomerId;
            db.Rental.Add(newRental);
            db.SaveChanges();

            result.success = true;
            result.message = "Araç başarıyla kiralandı.";
            return result;
        }

        [HttpDelete]
        [Route("api/deleteRental{rentalId}")]
        public ResultModel deleteRental(string rentalId)
        {
            Rental rental = db.Rental.Where(x => x.rentalId == rentalId).SingleOrDefault();

            if (rental == null)
            {
                result.success = false;
                result.message = "Bu kayıt zaten silinmiş.";
                return result;
            }

            db.Rental.Remove(rental);
            db.SaveChanges();

            result.success = true;
            result.message = "Kayıt başarıyla silindi.";
            return result;
        }
        #endregion
    }
}
