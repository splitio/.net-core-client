using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splitio.Services.Parsing.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Splitio_tests.Unit_Tests.Matchers
{
    [TestClass]
    public class MatchesStringMatcherTests
    {
        [TestMethod]
        public void MatchShouldReturnTrueOnMatchingKey()
        {
            //Arrange
            var matcher = new MatchesStringMatcher("^a");

            //Act
            var result = matcher.Match("arrive");

            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseOnNonMatchingKey()
        {
            //Arrange
            var matcher = new MatchesStringMatcher("^a");

            //Act
            var result = matcher.Match("split");

            //Assert
            Assert.IsFalse(result);
        }


        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingLong()
        {
            //Arrange
            var matcher = new MatchesStringMatcher("^a");

            //Act
            var result = matcher.Match(123);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingDate()
        {
            //Arrange
            var matcher = new MatchesStringMatcher("^a");

            //Act
            var result = matcher.Match(DateTime.UtcNow);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingBoolean()
        {
            //Arrange
            var matcher = new MatchesStringMatcher("^a");

            //Act
            var result = matcher.Match(true);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MatchShouldReturnFalseIfMatchingSet()
        {
            //Arrange
            var matcher = new MatchesStringMatcher("^a");

            //Act
            var keys = new List<string>();
            keys.Add("test1");
            keys.Add("test3");

            var result = matcher.Match(keys);

            //Assert
            Assert.IsFalse(result);
        }

        [DeploymentItem(@"Resources\regex.txt")]
        [TestMethod]
        public void VerifyRegexMatcher()
        {
            VerifyTestFile(@"Resources\regex.txt", new string[] { "\r\n" });
        }


        private void VerifyTestFile(string file, string[] sepparator)
        {
            //Arrange
            var fileContent = File.ReadAllText(file);
            var contents = fileContent.Split(sepparator, StringSplitOptions.None);
            var csv = from line in contents
                      select line.Split('#').ToArray();

            var results = new List<string>();
            //Act
            foreach (string[] item in csv)
            {
                if (item.Length == 3)
                {
                    //Arrange
                    var matcher = new MatchesStringMatcher(item[0]);

                    //Act
                    var result = matcher.Match(item[1]);

                    //Assert
                    Assert.AreEqual(Convert.ToBoolean(item[2]), result, item[0] + "-" + item[1]);
                }

            }
        }
    }
}
