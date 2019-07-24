﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Domain;
using Splitio.Services.EngineEvaluator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Splitio_net_frameworks_tests.Unit_Tests.Splitters
{
    [TestClass]
    public class SplitterTests
    {
        [DeploymentItem(@"Resources\legacy-sample-data.csv")]
        [TestMethod]
        public void VerifyHashAndBucketSampleData()
        {
            VerifyTestFile("legacy-sample-data.csv", new string[] { "\r\n" });
        }

        [DeploymentItem(@"Resources\legacy-sample-data-non-alpha-numeric.csv")]
        [TestMethod]
        public void VerifyHashAndBucketSampleDataNonAlphanumeric()
        {
            VerifyTestFile("legacy-sample-data-non-alpha-numeric.csv", new string[] { "\n" });
        }


        [DeploymentItem(@"Resources\murmur3-sample-data-v2.csv")]
        [TestMethod]
        public void VerifyMurmur3HashAndBucketSampleData()
        {
            VerifyTestFile("murmur3-sample-data-v2.csv", new string[] { "\r\n" }, false);
        }


        [DeploymentItem(@"Resources\murmur3-sample-data-non-alpha-numeric-v2.csv")]
        [TestMethod]
        public void VerifyMurmur3HashAndBucketSampleDataNonAlphanumeric()
        {
            VerifyTestFile("murmur3-sample-data-non-alpha-numeric-v2.csv", new string[] { "\n" }, false);
        }


        private void VerifyTestFile(string file, string[] sepparator, bool legacy = true)
        {
            //Arrange
            var fileContent = File.ReadAllText(file);
            var contents = fileContent.Split(sepparator, StringSplitOptions.None);
            var csv = from line in contents
                      select line.Split(',').ToArray();

            var splitter = new Splitter();
            bool first = true;

            var results = new List<string>();
            //Act
            foreach (string[] item in csv)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    if (item.Length == 4)
                    {
                        var hash = legacy ? splitter.LegacyHash(item[1], int.Parse(item[0])) : splitter.Hash(item[1], int.Parse(item[0]));
                        var bucket = legacy ? splitter.LegacyBucket(item[1], int.Parse(item[0])) : splitter.Bucket(item[1], int.Parse(item[0]));

                        //Assert
                        Assert.AreEqual(hash, long.Parse(item[2]));
                        Assert.AreEqual(bucket, int.Parse(item[3]));
                    }
                }
            }
        }



        [TestMethod]
        public void VerifyCallMurmurOrLegacyDependingOnSplit()
        {
            //Arrange
            var splitter = new Splitter();
            var partitions = new List<PartitionDefinition>();
            partitions.Add(new PartitionDefinition(){ size = 10, treatment = "on"});
            partitions.Add(new PartitionDefinition(){ size = 90, treatment = "off"});

            //Act
            var result1 = splitter.GetTreatment("aUfEsdPN1twuEjff9Sl", 467569525, partitions, AlgorithmEnum.LegacyHash);
            var result2 = splitter.GetTreatment("Sx1JzS1TDc", 467569525, partitions, AlgorithmEnum.LegacyHash);
            var result3 = splitter.GetTreatment("Sx1JzS1TDc", 467569525, partitions, AlgorithmEnum.Murmur);
            var result4 = splitter.GetTreatment("aUfEsdPN1twuEjff9Sl", 467569525, partitions, AlgorithmEnum.Murmur);

            //Assert
            Assert.AreEqual("off", result1);
            Assert.AreEqual("on", result2);
            Assert.AreEqual("off", result3);
            Assert.AreEqual("off", result4);
        }
    }
}
