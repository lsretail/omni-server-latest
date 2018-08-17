using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Configuration;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace LSOmni.Service
{
    /// <summary>
    /// Service fault behavior
    /// </summary>
    public class ServiceFaultBehavior : BehaviorExtensionElement, IEndpointBehavior
    {
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint,
                    EndpointDispatcher endpointDispatcher)
        {
            ServiceFaultMessageInspector inspector = new ServiceFaultMessageInspector();
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(inspector);
        }

        // The following methods are stubs and not relevant. 
        public void AddBindingParameters(ServiceEndpoint endpoint,
               BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint,
                    ClientRuntime clientRuntime)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        public override System.Type BehaviorType
        {
            get { return typeof(ServiceFaultBehavior); }
        }

        protected override object CreateBehavior()
        {
            return new ServiceFaultBehavior();
        }
    }

    /// <summary>
    /// Change reponse code from HTTP 500 to HTTP 200 works with 
    /// </summary>
    public class ServiceFaultMessageInspector : IDispatchMessageInspector
    {
        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            if (reply.IsFault)
            {
                HttpResponseMessageProperty property = new HttpResponseMessageProperty();
                // Here the response code is changed to 200.
                property.StatusCode = System.Net.HttpStatusCode.OK;

                reply.Properties[HttpResponseMessageProperty.Name] = property;
            }
        }

        public object AfterReceiveRequest(ref Message request,
        IClientChannel channel, InstanceContext instanceContext)
        {
            // Do nothing to the incoming message.
            return null;
        }
    }
    
 

}