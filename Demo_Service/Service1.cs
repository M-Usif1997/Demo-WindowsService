using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Demo_Service
{
    public partial class Service1 : ServiceBase
    {
        private Timer timer;
        private HttpClient client;


        public Service1()
        {
            timer = new Timer();
            InitializeComponent();

        }
        protected override void OnStart(string[] args)
        {
            
            WriteToFile("OnStart: " + DateTime.Now);
            timer = new Timer();
            timer.Interval = 10000; // 10 seconds
            timer.Elapsed += async (sender, e) => await OnElapsedTimeAsync();
            timer.Start();

            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = ValidateCertificate;

            client = new HttpClient(handler);
            client.BaseAddress = new Uri("https://localhost:44328/");

        }
        protected override void OnStop()
        {
            timer.Stop();
            timer.Dispose();
            client.Dispose();
            WriteToFile("Service is stopped at " + DateTime.Now);
           
        }
        private async Task OnElapsedTimeAsync()
        {
            try
            {

              
                HttpResponseMessage response =  await client.GetAsync("api/values");
                WriteToFile("API called with status code: " + response + " - " + DateTime.Now);
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    WriteToFile("API response: " + result + " - " + DateTime.Now);
                }
                else
                {
                    WriteToFile("API call failed with status code: " + response.StatusCode + " - " + DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    WriteToFile("An error occurred: " + ex.InnerException.Message + " - " + DateTime.Now);
                }
                WriteToFile("An error occurred: " + ex.Message + " - " + DateTime.Now);
            }
          
            //WriteToFile("Service is recall at " + DateTime.Now);
        }



        private bool ValidateCertificate(HttpRequestMessage request, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // Add your custom certificate validation logic here
            // For this example, we're ignoring all certificate validation errors
            return true;
        }

        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }

      
    }
}

