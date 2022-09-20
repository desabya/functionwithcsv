using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Email.Clients;

namespace FnBlobCopy
{
   
    internal  class SendEmail
    {
        private readonly IConfiguration _configuration;
        public SendEmail(IConfiguration configuration)
        {
            _configuration = configuration;
        }


       
    }
}
