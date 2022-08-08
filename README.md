> :warning: This repository has been archived since we unified the source code for both .NET and .NET-Core clients a while back. The last release done here was 6.2.3 and this repo won't receive any further maintenance as the source code for the client current versions is not kept here anymore.
For current and future releases, reporting issues or submitting any PRs, please see our unified and final [dotnet-client repository](https://github.com/splitio/dotnet-client).

# Split SDK for .NET

![CI Status](https://ci.appveyor.com/api/projects/status/github/splitio/.net-core-client?branch=master&svg=true&passingText=master%20-%20ok&pendingText=master%20-%20running&failingText=master%20-%20failing)

## Overview
This SDK is designed to work with Split, the platform for controlled rollouts, which serves features to your users via a Split feature flag to manage your complete customer experience.
 
[![Twitter Follow](https://img.shields.io/twitter/follow/splitsoftware.svg?style=social&label=Follow&maxAge=1529000)](https://twitter.com/intent/follow?screen_name=splitsoftware)
 
## Compatibility
This SDK is compatible with NET Framework 4.0 and above and NET Core 1.x, 2.x and 3.0.

## Getting started
Below is a simple example that describes the instantiation and most basic usage of our SDK:

```c#
var config = new ConfigurationOptions();
var factory = new SplitFactory("SDK_API_KEY", config);
var sdk = factory.Client();

try
{
    sdk.BlockUntilReady(10000);
}
catch (Exception ex)
{
    // log & handle 
}

var treatment = sdk.GetTreatment("CUSTOMER_ID","SPLIT_NAME");

if (treatment == "on") 
{
    // insert code here to show on treatment
} 
else if (treatment == "off") 
{
    // insert code here to show off treatment 
} 
else 
{
    // insert your control treatment code here
}
```

Please refer to [our official docs](https://help.split.io/hc/en-us/articles/360020240172--NET-SDK) to learn about all the functionality provided by our SDK and the configuration options available for tailoring it to your current application setup.

## Submitting issues
 
The Split team monitors all issues submitted to this [issue tracker](https://github.com/splitio/.net-core-client/issues). We encourage you to use this issue tracker to submit any bug reports, feedback, and feature enhancements. We'll do our best to respond in a timely manner.
 
## Contributing
Please see [Contributors Guide](CONTRIBUTORS-GUIDE.md) to find all you need to submit a Pull Request (PR).

## License
Licensed under the Apache License, Version 2.0. See: [Apache License](http://www.apache.org/licenses/).

## About Split
 
Split is the leading Feature Delivery Platform for engineering teams that want to confidently deploy features as fast as they can develop them. Split’s fine-grained management, real-time monitoring, and data-driven experimentation ensure that new features will improve the customer experience without breaking or degrading performance. Companies like Twilio, Salesforce, GoDaddy and WePay trust Split to power their feature delivery.
 
To learn more about Split, contact hello@split.io, or get started with feature flags for free at https://www.split.io/signup.
 
Split has built and maintains SDKs for:
 
* Java [Github](https://github.com/splitio/java-client) [Docs](https://help.split.io/hc/en-us/articles/360020405151-Java-SDK)
* Javascript [Github](https://github.com/splitio/javascript-client) [Docs](https://help.split.io/hc/en-us/articles/360020448791-JavaScript-SDK)
* Node [Github](https://github.com/splitio/javascript-client) [Docs](https://help.split.io/hc/en-us/articles/360020564931-Node-js-SDK)
* .NET [Github](https://github.com/splitio/dotnet-client) [Docs](https://help.split.io/hc/en-us/articles/360020240172--NET-SDK)
* Ruby [Github](https://github.com/splitio/ruby-client) [Docs](https://help.split.io/hc/en-us/articles/360020673251-Ruby-SDK)
* PHP [Github](https://github.com/splitio/php-client) [Docs](https://help.split.io/hc/en-us/articles/360020350372-PHP-SDK)
* Python [Github](https://github.com/splitio/python-client) [Docs](https://help.split.io/hc/en-us/articles/360020359652-Python-SDK)
* GO [Github](https://github.com/splitio/go-client) [Docs](https://help.split.io/hc/en-us/articles/360020093652-Go-SDK)
* Android [Github](https://github.com/splitio/android-client) [Docs](https://help.split.io/hc/en-us/articles/360020343291-Android-SDK)
* iOS [Github](https://github.com/splitio/ios-client) [Docs](https://help.split.io/hc/en-us/articles/360020401491-iOS-SDK)
 
For a comprehensive list of open source projects visit our [Github page](https://github.com/splitio?utf8=%E2%9C%93&query=%20only%3Apublic%20).
 
**Learn more about Split:**
 
Visit [split.io/product](https://www.split.io/product) for an overview of Split, or visit our documentation at [help.split.io](http://help.split.io) for more detailed information.