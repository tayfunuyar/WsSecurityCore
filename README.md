# WsSecurityCore
Ws Security Soap Request .Net Core


An example of use:

```csharp
/// Inject SoapService to your class 

  
   var request = new WsSoapRequest
        {
            SoapUrl = "",
            SoapActionName = "SoapActionName", // Example : OrderListAsync
            SoapActionUrl = "SoapActionUrl", // Example :  "http://tempuri.org/IService/OrderList"
            Parameters = new Dictionary<string, object>
                {
                      {"YourParameter1","YourParameterValue1"},
                      {"YourParameter2", "YourParameterValue2"},
                      //....
                },
            Username = "ServiceUsername",
            Password = "ServicePassword",
            GetOnlyBody = false, // If set true soap response xmlNodeList
        };

  var orderResponse = soapService.SendSOAPRequest(request);
  // Control Soap Response Result Here and Convert to your class to use XmlSerializer
}

```
If you have a any question please contact me or open issue. 
# Nuget Package 
https://www.nuget.org/packages/WsSecurityCore/
