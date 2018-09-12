using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Metadataload.Controllers;
using System.Web.Mvc;

namespace UnitTest
{
    [TestClass]
    public class ProfileTest
    {
        [TestMethod]
        public void CreateNewProfileSuccess()
        {
            var controller = new ProfileController();
            int result = controller.CreateProfile("test", "");
            Assert.AreNotEqual(0, result);
        }

        [TestMethod]
        public void LogInSuccess()
        {
            var controller = new ProfileController();
            Assert.AreNotEqual(0, controller.GetProfile("test", ""));
        }

        [TestMethod]
        public void LogInWrongPassword()
        {
            var controller = new ProfileController();
            Assert.AreEqual(0, controller.GetProfile("test", "45634534"));
        }

        [TestMethod]
        public void LogInWrongUrl()
        {
            var controller = new ProfileController();
            Assert.AreEqual(0, controller.GetProfile("tester", ""));
        }
    }
}
