using System;
using System.IO;
using Microsoft.Azure.Documents;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace FnBlobCopy
{
    [StorageAccount("BlobConnection")]
    public class FnCopyFromBlob
    {
        private readonly IConfiguration _configuration;

        public FnCopyFromBlob(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [FunctionName("CopyFromBlob")]
        public  async Task Run([BlobTrigger("sourceblobmanpower/{name}")]Stream myBlob,
            //[Blob("destblobmanpower/{name}",FileAccess.Write)] Stream outputStream,
            string name, ILogger log)
        {
            try
            {
                log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
                //var jsonData= DeserializeFromStreamCSV(myBlob);
                //var result = JsonConvert.DeserializeObject<string>(jsonData);
                List<PersonInfo> lst = new List<PersonInfo>();
                log.LogInformation($"CSV File");
                if (myBlob.Length > 0)
                {
                    using (var reader = new StreamReader(myBlob))
                    {
                        var headers = await reader.ReadLineAsync();

                        var startLineNumber = 1;
                        var currentLine = await reader.ReadLineAsync();
                        while (currentLine != null)
                        {
                            currentLine = await reader.ReadLineAsync();
                            await AddLineToTable(currentLine, log, lst, name);
                            startLineNumber++;
                        }

                    }
                }
                //
                var client = new ServiceBusClient(_configuration["AzureServiceBus"].ToString());
                var sender = client.CreateSender("manpowersourcequeue");
                string strserialize = JsonConvert.SerializeObject(lst);
                var msg = new ServiceBusMessage(strserialize.ToString());
                await sender.SendMessageAsync(msg);
                //IQueueClient queueClient = new QueueClient()_configuration["AzureServiceBus"].ToString(), _configuration["QueueName"].ToString());
                //var orderJSON = JsonConvert.SerializeObject(myBlob);
                //var orderMessage = new Message(Encoding.UTF8.GetBytes(orderJSON))
                //{
                //    MessageId = Guid.NewGuid().ToString(),
                //    ContentType = "application/json"
                //};
                // await queueClient.SendAsync(orderMessage).ConfigureAwait(false);
                //outputStream = myBlob;
            }
            catch (Exception ex)
            {

                log.LogError(ex.Message);
            }
            
        }
        public static object DeserializeFromStream(Stream stream)
        {
            var serializer = new JsonSerializer();

            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize(jsonTextReader);
            }
        }
        private static async Task AddLineToTable(string line, ILogger log,List<PersonInfo> lst,String name)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                log.LogInformation("The Current Record is empty");
                return;
            }

            var columns = line.Split(',');
            var person = new PersonInfo()
            {
                id = Convert.ToInt32(columns[0]),
                Name = columns[1],
                Sname = Convert.ToString(columns[2]),
                email = columns[3],
                email2 = columns[4],
                profession = Convert.ToString(columns[5]),
                FileName= name


            };
            lst.Add(person);


        }
        public static string DeserializeFromStreamCSV(Stream myBlob)
        {
            String line;
            if (myBlob.Length > 0)
            {
                using (var reader = new StreamReader(myBlob))
                {
                     line = reader.ReadLine();
                    
                }
            }
            var csv = new List<string[]>();

            //var lines = line;

            //foreach (string line in lines)
            //    csv.Add(line.Split(','));

            //var properties = lines[0].Split(',');

            //var listObjResult = new List<Dictionary<string, string>>();

            //for (int i = 1; i < lines.Length; i++)
            //{
            //    var objResult = new Dictionary<string, string>();
            //    for (int j = 0; j < properties.Length; j++)
            //        objResult.Add(properties[j], csv[i][j]);

            //    listObjResult.Add(objResult);
            //}

            //return JsonConvert.SerializeObject(listObjResult);
            return "";
        }
    }
}
