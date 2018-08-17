using System;
using System.ServiceModel.Channels;
using NLog;

namespace LSOmni.Service
{
    public class JsonContentTypeMapper : WebContentTypeMapper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /*
        MUST user this binding for JsonContentTypeMapper to work
         
        <customBinding>
        <binding name="RawReceiveCapable">
          <!-- Provide the fully qualified name of the WebContentTypeMapper  -->
          <webMessageEncoding webContentTypeMapperType="LSOmni.Service.JsonContentTypeMapper, LSRetail.Mobile.WebService.Loyalty.Service,
                  Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" />
          <httpTransport manualAddressing="true" maxReceivedMessageSize="524288000" transferMode="Streamed" />
          <!-- maxReceivedMessageSize is 500 Mb -->
        </binding>
        </customBinding>
         */

        public override WebContentFormat GetMessageFormatForContentType(string contentType)
        {
            try
            {
                WebMessageEncodingBindingElement x = new WebMessageEncodingBindingElement();

                // I only binded the LoyaltyJson service to this content type so only expecting Json... 
                return WebContentFormat.Json;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "GetMessageFormatForContentType error " + contentType);
                return WebContentFormat.Default;
            }
        }
    }
}
