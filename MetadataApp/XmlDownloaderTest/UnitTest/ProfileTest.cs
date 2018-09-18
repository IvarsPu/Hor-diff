using Microsoft.VisualStudio.TestTools.UnitTesting;
using Metadataload.Controllers;
using System.Web.Mvc;
using System.IO;
using System.Web;
using Moq;

namespace UnitTest
{
    [TestClass]
    public class ProfileTest
    {
        private static ProfileController pc;

        static ProfileTest()
        {
            ProfileController.path = "test.xml";
        }

        [TestInitialize]
        public void Initialize()
        {
            SetSession("1");
            pc.CreateProfile("test", "");
        }

        #region View
        [TestMethod]
        public void GetLogInView()
        {
            Assert.IsNotNull(pc.LogIn());
        }

        [TestMethod]
        public void GetCreateView()
        {
            Assert.IsNotNull(pc.Create());
        }

        [TestMethod]
        public void GetUpdateView()
        {
            Assert.IsNotNull(pc.Update());
        }
        #endregion

        #region CreateProfile
        [TestMethod]
        public void CreateProfileSuccess()
        {
            Assert.AreNotEqual(0, pc.CreateProfile("tester", ""));
        }

        [TestMethod]
        public void CreateProfileUsedName()
        {
            Assert.AreEqual(0, pc.CreateProfile("test", ""));
        }
        #endregion

        #region GetProfile
        [TestMethod]
        public void GetProfileSuccess()
        {
            Assert.AreNotEqual(0, pc.GetProfile("test", ""));
        }

        [TestMethod]
        public void GetProfileWrongPassword()
        {
            Assert.AreEqual(0, pc.GetProfile("test", "45634534"));
        }

        [TestMethod]
        public void GetProfileWrongUrl()
        {
            Assert.AreEqual(0, pc.GetProfile("tester", ""));
        }
        #endregion

        #region DeleteProfile
        [TestMethod]
        public void DeleteProfileSuccess()
        {
            Assert.AreEqual(true, pc.DeleteProfile());
        }

        [TestMethod]
        public void DeleteProfileDoesntExist()
        {
            SetSession("0");
            Assert.AreEqual(false, pc.DeleteProfile());
        }
        #endregion

        #region UpdateProfile
        [TestMethod]
        public void UpdateProfileSuccess()
        {
            Assert.AreEqual(true, pc.UpdateProfile("tester", ""));
        }

        [TestMethod]
        public void UpdateProfileNotFound()
        {
            SetSession("0");
            Assert.AreEqual(false, pc.UpdateProfile("tester", ""));
        }

        [TestMethod]
        public void UpdateProfileAlreadyExistsWithThisURL()
        {
            pc.CreateProfile("tester", "");
            Assert.AreEqual(false, pc.UpdateProfile("tester", ""));
        }
        #endregion

        [TestCleanup]
        public void Cleanup()
        {
            File.Delete(ProfileController.path);
        }
        
        private static void SetSession(string value)
        {
            var context = new Mock<HttpContextBase>();
            var session = new Mock<HttpSessionStateBase>();

            session.Setup(s => s[ProfileController.profileId]).Returns(value);
            context.Setup(c => c.Session).Returns(session.Object);

            pc = new ProfileController();

            ControllerContext ctx = new ControllerContext();
            ctx.HttpContext = context.Object;
            pc.ControllerContext = ctx;
        }
    }
}
