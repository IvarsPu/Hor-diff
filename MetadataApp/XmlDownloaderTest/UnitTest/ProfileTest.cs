using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Metadataload.Controllers;
using System.Web.Mvc;
using System.IO;
using System.Web;
using System.Collections.Generic;
using Moq;
using System.Web.Routing;

namespace UnitTest
{
    [TestClass]
    public class ProfileTest
    {
        static ProfileTest()
        {
            ProfileController.path = "C:/Users/ralfs.zangis/Desktop/test.xml";
            File.Delete(ProfileController.path);
        }

        [TestMethod]
        public void Test()
        {
            var context = new Mock<HttpContextBase>();
            var session = new Mock<HttpSessionStateBase>();

            session.Setup(s => s[ProfileController.profileId]).Returns("");
            context.Setup(c => c.Session).Returns(session.Object);

            ProfileController pc = new ProfileController();

            ControllerContext ctx = new ControllerContext();
            ctx.HttpContext = context.Object;
            pc.ControllerContext = ctx;

            ViewResult v = pc.LogIn() as ViewResult;
            Assert.IsNotNull(v);
        }

        [TestMethod]
        public void CreateProfileSuccess()
        {
            int result = new ProfileController().CreateProfile("test", "");
            Assert.AreNotEqual(0, result);
        }

        [TestMethod]
        public void CreateProfileUsedName()
        {
            int result = new ProfileController().CreateProfile("test", "");
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void GetProfileSuccess()
        {
            var controller = new ProfileController();
            Assert.AreNotEqual(0, controller.GetProfile("test", ""));
        }

        [TestMethod]
        public void GetProfileWrongPassword()
        {
            var controller = new ProfileController();
            Assert.AreEqual(0, controller.GetProfile("test", "45634534"));
        }

        [TestMethod]
        public void GetProfileWrongUrl()
        {
            var controller = new ProfileController();
            Assert.AreEqual(0, controller.GetProfile("tester", ""));
        }

        [TestMethod]
        public void DeleteProfileSuccess()
        {
            var controller = new ProfileController();
            Assert.AreEqual(true, controller.DeleteProfile());
        }
    }

    public class MockHttpSession: HttpSessionStateBase
    {
        Dictionary<string, object> _sessionDictionary = new Dictionary<string, object>();

        public override object this[string name]
        {
            get { return _sessionDictionary[name]; }
            set { _sessionDictionary[name] = value; }
        }
    }
}
