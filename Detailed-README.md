# .net-core-client

## CI status
![CI Status](https://ci.appveyor.com/api/projects/status/github/splitio/.net-core-client?branch=master&svg=true&passingText=master%20-%20ok&pendingText=master%20-%20running&failingText=master%20-%20failing)

![CI Status](https://ci.appveyor.com/api/projects/status/github/splitio/.net-core-client?branch=development&svg=true&passingText=development%20-%20ok&pendingText=development%20-%20running&failingText=development%20-%20failing)


## Installing Split SDK using Nuget

To install Splitio, run the following command in the Package Manager Console

```
Install-Package Splitio-net-core
```

## Write your code!

SDK Configuration options

```cs
    var configurations = new ConfigurationOptions();
    configurations.FeaturesRefreshRate = 30;
    configurations.SegmentsRefreshRate = 30;

```

Create the Split Client instance. 

```cs
var factory = new SplitFactory("API_KEY", configurations);
var sdk = factory.Client();
```

Checking if the key belongs to treatment 'on' in sample_feature. 

```cs
if (sdk.GetTreatment("key", "sample_feature") == "on") 
{
    //Code for enabled feature
} 
else 
{
    //Code for disabled feature
}
```

Using matching key and bucketing key

```cs
Key key = new Key("sample_matching_key", "sample_bucketing_key");

if (sdk.GetTreatment(key, "sample_feature") == "on") 
{
    //Code for enabled feature
} 
else 
{
    //Code for disabled feature
}
```

### Using Attributes in SDK

In order to Target Based on Custom Attributes, the SDK should be given an attributes dictionary. In the example below, we are rolling out a feature to users. The provided attributes - plan_type, registered_date, and deal_size - can be used in the Split Editor to decide whether to show the 'on' or 'off' treatment to this account.

```cs
var attributes = new Dictionary<string, object>();
attributes.Add("plan_type", "growth");
attributes.Add("registered_date", System.DateTime.UtcNow);
attributes.Add("deal_size", 1000);

string treatment = sdk.getTreatment("CUSTOMER_ID", "FEATURE_NAME", attributes);

if (treatment == "on") 
{
    // insert on code here
} 
else if (treatment == "off") 
{
    // insert off code here
} 
else 
{
    // insert control code here
}
```

In the attributes Dictionary used in this example, you can provide three types of attributes:

*String Literal Attributes*

String literal attributes capture the concept of a character Strings. The values for String literal attributes should be of type String. For instance, the value for attribute plan_type is a String. Such attributes can be used with the following matchers:

    in list

*Numeric Attributes*

Numeric attributes capture the concept of signed numbers. Their values can be of type long or int. For instance, the value for attribute deal_size is int. Negative numbers are allowed. Floating point numbers are not supported. Numeric attributes can be used with the following matchers:

    is =
    is >=
    is <=
    is between

*DateTime Attributes*

DateTime attributes capture the concept of a Date, with optional Time. DateTime should be expressed in UTC.

DateTime attributes can be used with the following matchers:

    is on
    is on or after
    is on or before
    is between

### FAQ On Attributes

*What happens if a Split uses an attribute whose value is not provided in code?*

For instance, take this Split definition:

if user.age <= 60 then split 100%:on
else
if user is in segment all then split 100%:off

If the value for age attribute is not provided in the attributes map passed to getTreatment call, then the matcher in the first condition user.age <= 60 will not match. The user will get the evaluation of the second condition: if user is in segment all then split 100%:off which is off.

*What happens if a Split uses an attribute whose value in code is not of correct type?*

For instance, take this Split definition:

if user.plan_type is in list ["basic"] then split 100%:on
else
if user is in segment all then split 100%:off

And say the value provided for plan_type is an int instead of a String:

In this scenario, the matcher in the first condition user.plan_type is in list ["basic"] will not match. The user will get the evaluation of the second condition: if user is in segment all then split 100%:off which is off.

*How can I use boolean types?*

At this point, the SDK does not provide matchers specifically for booleans. You can either translate booleans into a numeric attribute 1 or 0 and use is = 1 matcher, or a String attribute true or false and use is in list ["true"] matcher.

*How does the SDK perform date equality?*

If we have the following Split:

if user.registration_date is on 2016/01/01 then split 100%:on

Then there are 8.64 x 10^7 milliseconds that can fall on that date. The SDK ensures that if you provide any of those milliseconds, it will be considered equal to 2016/01/01.
    

### Advanced Configuration of the SDK 

The SDK can configured for performance. Each configuration has a default, however, you can provide an override value while instantiating the SDK.


```cs    
    var configurations = new ConfigurationOptions();
    configurations.FeaturesRefreshRate = 30;
    configurations.SegmentsRefreshRate = 30;
    configurations.ImpressionsRefreshRate = 30;
    configurations.MetricsRefreshRate = 30;
    configurations.ReadTimeout = 15000;
    configurations.ConnectionTimeOut = 15000;
    
    var factory = new SplitFactory("API_KEY", configurations);
    var sdk = factory.Client();
```

###  Blocking the SDK Until It Is Ready 

When the SDK is instantiated, it kicks off background tasks to update an in-memory cache with small amounts of data fetched from Split servers. This process can take up to a few hundred milliseconds, depending on the size of data. While the SDK is in this intermediate state, if it is asked to evaluate which treatment to show to a customer for a specific feature, it may not have data necessary to run the evaluation. In this circumstance, the SDK does not fail, rather it returns The Control Treatment.

If you would rather wait to send traffic till the SDK is ready, you can do that by blocking until the SDK is ready. This is best done as part of the startup sequence of your application. Here is an example:

```cs
        ISplitClient client = null;

        try
        {            
            var configurations = new ConfigurationOptions();
            configurations.Ready = 1000;
            var factory = new SplitFactory("API_KEY", configurations);
            client = factory.Client();
        }
        catch (TimeoutException t)
        {
            // SDK was not ready in 1000 miliseconds
        }
```

###  Running the SDK in 'off-the-grid' Mode 

Features start their life on one developer's machine. A developer should be able to put a feature behind Split on their development machine without the SDK requiring network connectivity. To achieve this, Split SDK can be started in 'localhost' (aka off-the-grid mode). In this mode, the SDK neither polls nor updates Split servers, rather it uses an in-memory data structure to determine what treatments to show to the logged in customer for each of the features. Here is how you can start the SDK in 'localhost' mode:

```cs
    var factory = new SplitFactory("localhost", configurations);
    var client = factory.Client();
```

In this mode, the SDK loads a mapping of feature name to treatment from a file at $HOME/.split. For a given feature, the specified treatment will be returned for every customer. In Split terms, the roll-out plan for that feature becomes:

```
if user is in segment all then split 100%:treatment
```

Any feature that is not mentioned in the file is assumed to not exist. The SDK returns The Control Treatment for every customer of that feature.

The format of this file is two columns separated by whitespace. The left column is the feature name, the right column is the treatment name. Here is a sample feature file:

```
# this is a comment

reporting_v2 on # sdk.getTreatment(*, reporting_v2) will return 'on'

double_writes_to_cassandra off

new-navigation v3
```

###  Split Manager 

In order to obtain a list of Split features available in the in-memory dataset used by Split client to evaluate treatments, use the Split Manager.

```cs
    var factory = new SplitFactory("API_KEY", configurations);
    var splitManager = factory.Manager();
```

Currently, SplitManager exposes the following interface:

```cs
    List<SplitView> Splits();
    SplitView Split(String featureName);
```

calling splitManager.Split(String featureName) will return the following structure:

```cs
    public class SplitView
    {
        public string name { get; set; }
        public string trafficType { get; set; }
        public bool killed { get; set; }
        public List<string> treatments { get; set; }
        public long changeNumber { get; set; }
    }
```

###  Randomization of Polling Periods 

The SDK polls Split servers for feature split and segment changes at regular periods. The configuration parameters FeaturesRefreshRate and SegmentsRefreshRate control these periods. Say the value set for FeaturesRefreshRate is 60 seconds. Then, instead of polling at exactly p seconds interval, each SDK instance polls at a randomly chosen time period in the range (0.5 * p, p). This randomization is done to avoid letting SDKs deployed across multiple machines to poll at the same time which can lead to bad performance.

###  Impression Listener

In order to capture every single impression in your app SDK provides an option called Impression Listener. It works pretty straightforward: you define a class that implements IImpressionListener interface, which must have instance method called Log, which must receive an argument of type KeyImpression. 

This is an example on how to write your custom impression listener:

```cs
	public class CustomImpressionListener: IImpressionListener
	{
	   ...
	   
	   public void Log(KeyImpression impression)
	   {
		 //Implement your custom code
	   }
	}
```

And then, you can configure the SDK to use it in ConfigurationOptions object:

```cs
	...
	configurations.ImpressionListener = new CustomImpressionListener();
	...

	var factory = new SplitFactory("API_KEY", configurations);
	var sdk = factory.Client();
```

###  Logging in the SDK 

The .NET SDK uses Common.Logging for logging. You can write your own adapter by implementing ILoggerFactoryAdapter interface. More details [here](http://netcommon.sourceforge.net/docs/2.1.0/reference/html/ch01.html)
Splitio SDK doesn't log by default, you need to configure an adapter.

This is an example on how to write an NLog adapter:

```cs
	public class NLogAdapter : ILog
	{
	  //Implement ILog interface
	}
```

```cs
    public class CommonLoggingNLogAdapter : ILoggerFactoryAdapter
    {
        public ILog GetLogger(Type type)
        {
            return new NLogAdapter(type);
        }

        public ILog GetLogger(string key)
        {
            return new NLogAdapter(key);
        }
    }
```

This is an example on how to configure NLog and its adapter:

```cs
	var config = new LoggingConfiguration();
	var fileTarget = new FileTarget();
	config.AddTarget("file", fileTarget);
	fileTarget.FileName = @"ANY FILE NAME";
	fileTarget.ArchiveFileName = "ANY FILE NAME";
	fileTarget.LineEnding = LineEndingMode.CRLF;
	fileTarget.Layout = "${longdate} ${level: uppercase = true} ${logger} - ${message} - ${exception:format=tostring}";
	fileTarget.ConcurrentWrites = true;
	fileTarget.CreateDirs = true;
	fileTarget.ArchiveNumbering = ArchiveNumberingMode.Date;
	var rule = new LoggingRule("*", LogLevel.Debug, fileTarget);
	config.LoggingRules.Add(rule);
	LogManager.Configuration = config;     

    Common.Logging.LogManager.Adapter = new CommonLoggingNLogAdapter();
	
	...
	
    var factory = new SplitFactory("API_KEY", configurations);
```	
