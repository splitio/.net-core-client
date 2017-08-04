Since joining Split, it has been clear that testing is the fundamental building block of our software development process. We deploy unit tests to validate discrete parts of business logic, end-to-end tests to monitor our APIs, automated user interaction tests to maintain our user experience, and entire network of bots ensuring the quality of service of our SDKs.

Our tests drive development, allowing engineers to think through all possible use cases and edge conditions. Tests provide documentation, showing new members of the team how a block of code is expected to work in a variety of situations. And tests drive stability, constantly seeking out regressions as more and more features are added.

Testing, with control.

In today's modern software development lifecycle, high quality testing is as important as ever, but it has become increasingly more challenging to ensure your coverage is complete. At the same time that feature flag technology has allowed for releases to be more stable than ever, poor implementations of that technology have brought about cultures who test purely in production - reliant on the ease of ramping and rollback to protect users from widespread catastrophe. Using a controlled rollout platform that is both feature rich and highly testable is a challenge, but here at Split we believe it is essential for successful releases to be able to have the utmost confidence in the features released to clients.

When most organizations start with working with controlled rollout, they default all feature flag checks to the "off" or "control" state and then rely on unit testing the methods specific to the new feature to ensure it behaves as expected. However, as more and more features are enabled in production there can be significant drift between the behavior you test and the behavior customers actually see.

To prevent that drift both during testing and for engineers running their product's locally, we have integrated into the Split Software Development Kit the recently-announced Off-the-Grid mode, which uses a configuration file to set the state of each of your features. Our customers often maintain such a configuration to run their tests against that mirrors the state of an environment. It is even possible to implement your deployment architecture to run tests with multiple Off-The-Grid configurations, allowing for a variety of states to be tested before deploying to users.

Introducing Split's new programmatic testing mode.

Often, though, we will want to test the multiple states of a feature directly inside the unit test suite. Rather than pass in multiple configuration files, we prefer a method that allows a single file to test a variety of feature options for consistency, and also to directly test for the expected changes of behavior when a feature is enabled. To that end, we have released a new testing mode for our .Net Core client. This testing mode allows you to programatically change the treatments returned by the Split Client; allowing you to start a test by setting the specific features you wish to test regardless of their state in the Split console.

Examples: how we test at Split.

This is best described in context of a recent feature release here at Split. At Split we know our success is dependent on ensuring our product is built for teams, and to that end we are releasing functionality that allows users to construct their own groups of users and use those teams as a short hand throughout the product. Part of this project involves transitioning our existing model of handling Split Administrators to utilize this same groups infrastructure. However, making a change to such a critical part of our architecture needs to be done with robust testing and deliberate roll out.

A best practice we have established for these types of transitions is to utilize the techniques of dual writes and dark reads. Effectively, you start your roll out by writing the relevant data to both the original collection and the new data structure, while continuing to read from the original collection. Then, to validate your new logic, you start reading from both the original source and the new structure and compare the results. You still return the data coming from the original source (this second read happens "in the dark") but are able to log any occasions that the results differ. Once you confirm that the data is always consistent, you can turn the reads from the new structure "on", and only once those reads are fully ramp do you transition your writes from "dual" to "off" and deprecate the old code. This process ensures a steady, consistent, and controlled roll out with minimal risk of exposure to customers.

Testing this logic in the old world would be very risky. Typically requiring careful monitoring of logs and dashboards, ramping and de-ramping of the relevant features, and additional deployments to fix bugs found along the way. Using the testing mode of our .Net Core client, we can add extensive unit tests to confirm all permutations of these features behave as expected.

```cs
public class Test 
{
	private SplitClientForTest splitClient = new SplitClientForTest();

	[Fact]
	public void TestOriginalDatabase() 
	{
		Controller controller = new Controller(splitClient);

		// Create in Both Databases via Dual Writes
		var expected = controller.Create();

		// Read returns original logic
		var result = controller.Get(expected.Id);

		Assert.Equal(expected, result); 
	}

	[Fact]
	public void TestDarkReads() 
	{
		// Turn on Writes
		splitClient.SetTreatment("feature_reads", "dark");

		Controller controller = new Controller(splitClient);

		// Create in Both Databases via Dual Writes
		var expected = controller.Create();

		// Read hits both, but returns original logic
		var result = controller.Get(expected.Id);

		Assert.Equal(expected, result); 
	}

	[Fact]
	public void TestNewDatabase() 
	{
		// Turn on Writes
		splitClient.SetTreatment("feature_reads", "on");

		Controller controller = new Controller(splitClient);

		// Create in Both Databases via Dual Writes
		var expected = controller.Create();

		// Read returns from new logic
		var result = controller.Get(expected.Id);

		Assert.Equal(expected, result); 
	}
}
```

This system allows you to validate multiple feature release states in a single unit test file, both setting treatments at the class level in the setup function and at the test level. However, this model does result in a lot of repeated code. Certainly you could separate out the business logic of the test to a separate function, but here at Split we have a better way.

Alongside the SplitClientForTest, we are also today releasing a suite of XUnit annotations to streamline testing of your application across a wide variety of configurations. By using this annotations in your tests and tagging your SplitClient in the file, you can run them with the features and treatments you wish and Split will do the rest.

```cs
public class Test 
{
	private SplitClientForTest splitClient;

	[Theory]
	[SplitTest(test: @"{ feature:'feature_reads', treatments:['on', 'dark', 'off'] }")]
	public void TestReading(string feature, string treatment) 
	{
	    splitClient = new SplitClientForTest();
        splitClient.RegisterTreatment(feature, treatment);
		Controller controller = new Controller(splitClient);
		var expected = controller.Create();
		var result = controller.Get(expected.Id);

		Assert.Equal(expected, result); 
	}
}
```

XUnit runner executes each SplitTest once for each treatment value, updating the SplitClient with the correct treatments before each run. Any unset features will return the "control" treatment, and anything treatments manually set elsewhere will be preserved unless specifically overwritten by the SplitTest annotation.

In most cases, one is not limited to testing a single feature's roll out, but ideally you wish to test how a behavior might interact with a variety of other feature states. Therefore we have built the SplitScenario, which allows the developer to define a variety of feature states and the test will run across all possible permutations. In the case at hand, that would be testing both the read and write treatments simultaneously!

```cs
public class Test 
{
	private SplitClientForTest splitClient;

	[Theory]
	[SplitScenario(features:
	 @"[{ feature:'feature_writes', treatments:['on', 'dual', 'off'] },
		{ feature:'feature_reads', treatments:['on', 'dark', 'off'] }]")]
	public void TestReading(string feature, string treatment) 
	{
		splitClient = new SplitClientForTest();
        splitClient.RegisterTreatment(feature, treatment);
		Controller controller = new Controller(splitClient);
		var expected = controller.Create();
		var result = controller.Get(expected.Id);

		Assert.Equal(expected, result); 
	}
}
```


Now in less code than before we are actually running the test 9 times! Our goal is to make it easier to test a wide variety of complex interactions than it would be to simply test two different feature states under other frameworks.

In running this test for the first time, one may see certain failing scenarios. With writes disabled to the feature, total reads to that feature would likely start failing, and so would that run of the test. Now, the best practice I recommend is to identify these failure scenarios and defensively code against them - falling back to a sane default. These permutations are an excellent way to identify such cases. However, there could be scenarios where a bad combination of features may be inevitable, and we do support limiting your tests to only supported feature combinations with SplitSuites.

```cs
public class Test 
{
	private SplitClientForTest splitClient;

	[Theory]	        
    [SplitSuite(scenarios:
         @"[{
               features:
               [{ feature:'feature_writes', treatments:['on'] },
                { feature:'feature_reads', treatments:['on', 'dark'] }]
            },
			{
               features:
               [{ feature:'feature_writes', treatments:['off'] },
                { feature:'feature_reads', treatments:['dark', 'off'] }]
            },
            {  features:
               [{ feature:'feature_writes', treatments:['dual'] },
                { feature:'feature_reads', treatments:['on', 'dark', 'off'] }]
            }]")]
	public void TestReading(feature, treatment) 
	{
		splitClient = new SplitClientForTest();
        splitClient.RegisterTreatment(feature, treatment);
		Controller controller = new Controller(splitClient);

		// Create in Both Databases via Dual Writes
		var expected = controller.Create();

		// Read returns original logic
		var result = controller.Get(expected.Id);

		Assert.Equal(expected, result); 
	}
}
```

In this way you can specifically test for different behavior with different combinations of features.

Testing with high numbers of permutations can be done as part of integration or release testing so as to reduce the runtime of local tests. The .Net Core testing client and annotations suite is available now, and further documentation is available by reviewing the sample tests available in that repository.

