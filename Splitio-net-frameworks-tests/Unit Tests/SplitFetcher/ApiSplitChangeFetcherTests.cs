using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Splitio.Domain;
using Splitio.Services.SplitFetcher.Classes;
using Splitio.Services.SplitFetcher.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Splitio_net_frameworks_tests.Unit_Tests.SplitFetcher
{
    [TestClass]
    public class ApiSplitChangeFetcherTests
    {
        [TestMethod]
        [Description("Test a Json that changes its structure and is deserialized without exception. Contains: a field renamed, a field removed and a field added.")]
        public async Task ExecuteJsonDeserializeSuccessfulWithChangeInJsonFormat()
        {
            //Arrange
            Mock<ISplitSdkApiClient> apiMock = new Mock<ISplitSdkApiClient>();
            apiMock
                .Setup(x => x.FetchSplitChanges(-1))
                .Returns(TaskEx.FromResult("{\"splits\": [ { \"trafficType\": \"user\", \"name\": \"Reset_Seed_UI\", \"seed\": 1552577712, \"status\": \"ACTIVE\", \"defaultTreatment\": \"off\", \"changeNumber\": 1469827821322, \"conditions\": [ { \"matcherGroup\": { \"combiner\": \"AND\", \"matchers\": [ { \"keySelector\": { \"trafficType\": \"user\", \"attribute\": null }, \"matcherType\": \"ALL_KEYS\", \"negate\": false, \"userDefinedSegmentMatcherData\": null, \"whitelistMatcherData\": null, \"unaryNumericMatcherData\": null, \"betweenMatcherData\": null } ] }, \"partitions\": [ { \"treatment\": \"on\", \"size\": 100 }, { \"treatment\": \"off\", \"size\": 0, \"addedField\": \"test\"  } ] } ] } ], \"since\": 1469817846929, \"till\": 1469827821322 }\r\n"));

            ApiSplitChangeFetcher apiSplitChangeFetcher = new ApiSplitChangeFetcher(apiMock.Object);

            //Act
            var result = await apiSplitChangeFetcher.Fetch(-1);

            //Assert
            Assert.IsTrue(result != null);
            Assert.IsTrue(result.splits.Count > 0);
        }

        [TestMethod]
        public async Task FetchSplitChangesSuccessfull()
        {
            //Arrange
            var apiClient = new Mock<ISplitSdkApiClient>();
            apiClient
            .Setup(x => x.FetchSplitChanges(It.IsAny<long>()))
            .Returns(TaskEx.FromResult(@"{
                          'splits': [
                            {
                              'trafficTypeName': 'user',
                              'name': 'Test_1',
                              'seed': 673896442,
                              'status': 'ACTIVE',
                              'killed': false,
                              'defaultTreatment': 'off',
                              'changeNumber': 1470855828956,
                              'conditions': [
                                {
                                  'matcherGroup': {
                                    'combiner': 'AND',
                                    'matchers': [
                                      {
                                        'keySelector': {
                                          'trafficType': 'user',
                                          'attribute': null
                                        },
                                        'matcherType': 'ALL_KEYS',
                                        'negate': false,
                                        'userDefinedSegmentMatcherData': null,
                                        'whitelistMatcherData': null,
                                        'unaryNumericMatcherData': null,
                                        'betweenMatcherData': null
                                      }
                                    ]
                                  },
                                  'partitions': [
                                    {
                                      'treatment': 'on',
                                      'size': 0
                                    },
                                    {
                                      'treatment': 'off',
                                      'size': 100
                                    }
                                  ]
                                }
                              ]
                            }   
                          ],
                          'since': -1,
                          'till': 1470855828956
                        }"));
            var apiFetcher = new ApiSplitChangeFetcher(apiClient.Object);

            //Act
            var result = await apiFetcher.Fetch(-1);

            //Assert
            Assert.IsNotNull(result);
            var split = result.splits.First();
            Assert.AreEqual("Test_1", split.name);
            Assert.AreEqual(false, split.killed);
            Assert.AreEqual("ACTIVE", split.status);
            Assert.AreEqual("user", split.trafficTypeName);
            Assert.AreEqual("off", split.defaultTreatment);
            Assert.IsNotNull(split.conditions);
            Assert.AreEqual(-1, result.since);
            Assert.AreEqual(1470855828956, result.till);
            Assert.AreEqual(null, split.algo);
        }

        [TestMethod]
        public async Task FetchSplitChangesSuccessfullVerifyAlgorithmIsLegacy()
        {
            //Arrange
            var apiClient = new Mock<ISplitSdkApiClient>();
            apiClient
            .Setup(x => x.FetchSplitChanges(It.IsAny<long>()))
            .Returns(TaskEx.FromResult(@"{
                          'splits': [
                            {
                              'trafficTypeName': 'user',
                              'name': 'Test_1',
                              'seed': 673896442,
                              'status': 'ACTIVE',
                              'killed': false,
                              'algo': 1,
                              'defaultTreatment': 'off',
                              'changeNumber': 1470855828956,
                              'conditions': [
                                {
                                  'matcherGroup': {
                                    'combiner': 'AND',
                                    'matchers': [
                                      {
                                        'keySelector': {
                                          'trafficType': 'user',
                                          'attribute': null
                                        },
                                        'matcherType': 'ALL_KEYS',
                                        'negate': false,
                                        'userDefinedSegmentMatcherData': null,
                                        'whitelistMatcherData': null,
                                        'unaryNumericMatcherData': null,
                                        'betweenMatcherData': null
                                      }
                                    ]
                                  },
                                  'partitions': [
                                    {
                                      'treatment': 'on',
                                      'size': 0
                                    },
                                    {
                                      'treatment': 'off',
                                      'size': 100
                                    }
                                  ]
                                }
                              ]
                            }   
                          ],
                          'since': -1,
                          'till': 1470855828956
                        }"));
            var apiFetcher = new ApiSplitChangeFetcher(apiClient.Object);

            //Act
            var result = await apiFetcher.Fetch(-1);

            //Assert
            Assert.IsNotNull(result);
            var split = result.splits.First();
            Assert.AreEqual(AlgorithmEnum.LegacyHash, (AlgorithmEnum)split.algo);
        }

        [TestMethod]
        public async Task FetchSplitChangesSuccessfullVerifyAlgorithmIsMurmur()
        {
            //Arrange
            var apiClient = new Mock<ISplitSdkApiClient>();
            apiClient
            .Setup(x => x.FetchSplitChanges(It.IsAny<long>()))
            .Returns(TaskEx.FromResult(@"{
                          'splits': [
                            {
                              'trafficTypeName': 'user',
                              'name': 'Test_1',
                              'seed': 673896442,
                              'status': 'ACTIVE',
                              'killed': false,
                              'algo': 2,
                              'defaultTreatment': 'off',
                              'changeNumber': 1470855828956,
                              'conditions': [
                                {
                                  'matcherGroup': {
                                    'combiner': 'AND',
                                    'matchers': [
                                      {
                                        'keySelector': {
                                          'trafficType': 'user',
                                          'attribute': null
                                        },
                                        'matcherType': 'ALL_KEYS',
                                        'negate': false,
                                        'userDefinedSegmentMatcherData': null,
                                        'whitelistMatcherData': null,
                                        'unaryNumericMatcherData': null,
                                        'betweenMatcherData': null
                                      }
                                    ]
                                  },
                                  'partitions': [
                                    {
                                      'treatment': 'on',
                                      'size': 0
                                    },
                                    {
                                      'treatment': 'off',
                                      'size': 100
                                    }
                                  ]
                                }
                              ]
                            }   
                          ],
                          'since': -1,
                          'till': 1470855828956
                        }"));
            var apiFetcher = new ApiSplitChangeFetcher(apiClient.Object);

            //Act
            var result = await apiFetcher.Fetch(-1);

            //Assert
            Assert.IsNotNull(result);
            var split = result.splits.First();
            Assert.AreEqual(AlgorithmEnum.Murmur, (AlgorithmEnum)split.algo);
        }

        [TestMethod]
        public async Task FetchSplitChangesWithExcepionSouldReturnNull()
        {
            var apiClient = new Mock<ISplitSdkApiClient>();
            apiClient
            .Setup(x => x.FetchSplitChanges(It.IsAny<long>()))
            .Throws(new Exception());
            var apiFetcher = new ApiSplitChangeFetcher(apiClient.Object);

            //Act
            var result = await apiFetcher.Fetch(-1);

            //Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task FetchSplitChangesSuccessfull_WhenConfigurationsIsNotNull()
        {
            //Arrange
            var apiClient = new Mock<ISplitSdkApiClient>();
            apiClient
            .Setup(x => x.FetchSplitChanges(It.IsAny<long>()))
            .Returns(TaskEx.FromResult(@"{
                          'splits': [
                            {
                              'trafficTypeName': 'user',
                              'name': 'Test_1',
                              'seed': 673896442,
                              'status': 'ACTIVE',
                              'killed': false,
                              'algo': 2,
                              'defaultTreatment': 'off',
                              'changeNumber': 1470855828956,
                              'configurations': { 
                                'on': '{ size: 15 }',
                                'off':'{ size: 10 }'
                              },
                              'conditions': [
                                {
                                  'matcherGroup': {
                                    'combiner': 'AND',
                                    'matchers': [
                                      {
                                        'keySelector': {
                                          'trafficType': 'user',
                                          'attribute': null
                                        },
                                        'matcherType': 'ALL_KEYS',
                                        'negate': false,
                                        'userDefinedSegmentMatcherData': null,
                                        'whitelistMatcherData': null,
                                        'unaryNumericMatcherData': null,
                                        'betweenMatcherData': null
                                      }
                                    ]
                                  },
                                  'partitions': [
                                    {
                                      'treatment': 'on',
                                      'size': 0
                                    },
                                    {
                                      'treatment': 'off',
                                      'size': 100
                                    }
                                  ]
                                }
                              ]
                            }   
                          ],
                          'since': -1,
                          'till': 1470855828956
                        }"));

            var apiFetcher = new ApiSplitChangeFetcher(apiClient.Object);

            //Act
            var result = await apiFetcher.Fetch(-1);

            //Assert
            Assert.IsNotNull(result);
            var split = result.splits.First();
            Assert.AreEqual(AlgorithmEnum.Murmur, (AlgorithmEnum)split.algo);
            Assert.IsNotNull(split.configurations);
        }
    }
}