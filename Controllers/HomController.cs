﻿using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using WebApplication2.Models;
using System.Threading.Tasks;

namespace WebApplication2.Controllers
{

    public class HomController : Controller
    {
        string EndpointUrl;
        private string PrimaryKey;
        private DocumentClient client;
        private BMSEntities3 db = new BMSEntities3();
        string cacheConnectionstring = System.Configuration.ConfigurationManager.AppSettings["RedisCacheKey"];
     
        // GET: Hom
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(HttpPostedFileBase uploadFile)
        {
            foreach (string file in Request.Files)
            {
                uploadFile = Request.Files[file];
            }
            // Container Name - picture
            BlobManager BlobManagerObj = new BlobManager("picture");
            //string FileAbsoluteUri = BlobManagerObj.UploadFile(uploadFile);
            return RedirectToAction("GetBlob");
        }

        public ActionResult GetBlob()
        {
            // Container Name - picture
            BlobManager BlobManagerObj = new BlobManager("picture");
            List<string> fileList = BlobManagerObj.BlobList();
            
            return View(fileList);

        }

        [HttpPost]
        public ActionResult GetBlob(int Selectseat, string InputEmail1)
        {
            EndpointUrl = "https://zenab.documents.azure.com:443/";
            PrimaryKey = "fgu3OlUmlxO3EktrQzyllj4s6HQZKMf9y9jszSUDS80G9cu2VkFs8jNqNLGzj9gX1qVNERgurJU3uR5XIIEj4g==";
            client = new DocumentClient(new Uri(EndpointUrl), PrimaryKey);
            client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri("1", "bmsCosmos"), InputEmail1);
            if (Selectseat <= 5)
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var senderEmail = new MailAddress("ekn.user.dp@gmail.com", "Book My Show");
                        var receiverEmail = new MailAddress(InputEmail1, "Receiver");
                        var password = "User@123";
                        var sub = "Book My Show Ticket Confirmed";
                        var body = "Your ticket is confirmed";
                        var smtp = new SmtpClient
                        {
                            Host = "smtp.gmail.com",
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            Credentials = new NetworkCredential(senderEmail.Address, password)
                        };
                        using (var mess = new MailMessage(senderEmail, receiverEmail)
                        {
                            Subject = sub,
                            Body = body
                        })
                        {
                            smtp.Send(mess);
                        }
                        ViewBag.Message = "email sent successfully";
                        return RedirectToAction("GetBlob");
                    }
                }
                catch (Exception)
                {
                    ViewBag.Error = "Some Error mail not sent";

                }

                return RedirectToAction("GetBlob");

            }
            else
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        var senderEmail = new MailAddress("ekn.user.dp@gmail.com", "Book My Show");
                        var receiverEmail = new MailAddress(InputEmail1, "Receiver");
                        var password = "User@123";
                        var sub = "Book My Show Ticket Not Confirmed";
                        var body = "Your ticket is not confirmed because the number of ticketes you booked are" +
                        " exceding the limits. You can book only 5 Tickets at a time. Try Again";
                        var smtp = new SmtpClient
                        {
                            Host = "smtp.gmail.com",
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            Credentials = new System.Net.NetworkCredential(senderEmail.Address, password)
                        };
                        using (var mess = new MailMessage(senderEmail, receiverEmail)
                        {
                            Subject = sub,
                            Body = body
                        })
                        {
                            smtp.Send(mess);
                        }
                        return RedirectToAction("GetBlob");
                    }
                }
                catch (Exception)
                {
                    ViewBag.Error = "Some Error mail not sent";

                }
                return RedirectToAction("GetBlob");
            }
        }
        // GET: Moviesdesc
        public ActionResult Moviedesc()
        {
            var connect = ConnectionMultiplexer.Connect(cacheConnectionstring);
            //mydbcontext = new DebendraContext();
            IDatabase Rediscache = connect.GetDatabase();
            if (string.IsNullOrEmpty(Rediscache.StringGet("Moviedesc")))
            {
                var liemp = db.Moviedescs.ToList();
                var emplist = JsonConvert.SerializeObject(liemp);

                Rediscache.StringSet("Moviedesc", emplist, TimeSpan.FromSeconds(30));
                Console.WriteLine("Redis");
                return View(liemp);
            }
            else
            {

                var detail = JsonConvert.DeserializeObject<List<Moviedesc>>(Rediscache.StringGet("Moviedesc"));
                Console.WriteLine("from db");
                return View(detail);

            }
            //return View(db.Moviedescs.ToList());
        }
        public async Task<ActionResult> BookingHistoryModel()
        {
            await client.CreateDatabaseIfNotExistsAsync(new Database { Id = "1" });

            await client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri("bmsCosmos"),
            new DocumentCollection { Id = "1" });


            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };


            IQueryable<HomController> employeeQuery = this.client.CreateDocumentQuery<HomController>(
            UriFactory.CreateDocumentCollectionUri("1", "bmsCosmos"), queryOptions);

            return View();
        }
        //public ActionResult Delete(string uri)
        //{
        // // Container Name - picture
        // BlobManager BlobManagerObj = new BlobManager("picture");
        // BlobManagerObj.DeleteBlob(uri);
        // return RedirectToAction("Get");
        //}
    }
}