using System.Web;
using System.Web.Mvc;
using Metadataload.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace UnitTest
{
    [TestClass]
    public class ProcessTest
    {
        private static ProcessController pc;

        [TestInitialize]
        public void Initialize()
        {
            SetSession("1");
        }

        #region View
        [TestMethod]
        public void GetIndexView()
        {
            Assert.IsNotNull(pc.Index());
        }

        [TestMethod]
        public void GetInfoView()
        {
            Assert.IsNotNull(pc.Info(0));
        }

        [TestMethod]
        public void GetInfoViewBagSuccess()
        {
            pc.StartMetadataLoad("");
            var view = pc.Info(1) as ViewResult;
            Assert.AreNotEqual(null, view.ViewData["Process"]);
        }

        [TestMethod]
        public void GetInfoViewBagFail()
        {
            var view = pc.Info(1) as ViewResult;
            Assert.AreEqual(null, view.ViewData["Process"]);
        }
        #endregion

        #region StartMetadataLoad
        [TestMethod]
        public void StartMetadataLoadSuccess()
        {
            Assert.AreNotEqual(0, pc.StartMetadataLoad(""));
        }

        [TestMethod]
        public void StartMetadataLoadProfileDoesntExist()
        {
            SetSession("");
            Assert.AreEqual(0, pc.StartMetadataLoad(""));
        }

        [TestMethod]
        public void StartMetadataLoadProfileProcessingAlready()
        {
            pc.StartMetadataLoad("");
            Assert.AreEqual(0, pc.StartMetadataLoad(""));
        }
        #endregion

        #region StopProcess
        [TestMethod]
        public void StopProcessSucess()
        {
            Assert.AreEqual("", pc.StopProcess(pc.StartMetadataLoad("")));
        }

        [TestMethod]
        public void StopProcessNoAccess()
        {
            int id = pc.StartMetadataLoad("");
            SetSession("");
            Assert.AreEqual("No access", pc.StopProcess(id));
        }

        [TestMethod]
        public void StopProcessElementNotFound()
        {
            Assert.AreEqual("Element doesnt exist", pc.StopProcess(0));
        }
        #endregion

        #region GetProcessList
        [TestMethod]
        public void GetProcessListSuccess()
        {
            MetadataController controller = new MetadataController();
            Assert.IsNotNull(controller.GetProcessList());
        }

        [TestMethod]
        public void GetProcessListLast()
        {
            pc.StartMetadataLoad("");
            SetSession("2");
            pc.StartMetadataLoad("");
            MetadataController controller = new MetadataController();
            Assert.AreEqual(2,controller.GetProcessList(1)[0].Id);
        }

        [TestMethod]
        public void GetProcessListCount()
        {
            pc.StartMetadataLoad("");
            SetSession("2");
            pc.StartMetadataLoad("");
            MetadataController controller = new MetadataController();
            Assert.AreEqual(2, controller.GetProcessList().Count);
        }
        #endregion

        [TestCleanup]
        public void Cleanup()
        {
            ProcessController.Processes = new System.Collections.Generic.SortedDictionary<int, Metadataload.Models.Process>();
        }

        private static void SetSession(string value)
        {
            var context = new Mock<HttpContextBase>();
            var session = new Mock<HttpSessionStateBase>();

            session.Setup(s => s[ProfileController.profileId]).Returns(value);
            context.Setup(c => c.Session).Returns(session.Object);

            pc = new ProcessController();

            ControllerContext ctx = new ControllerContext();
            ctx.HttpContext = context.Object;
            pc.ControllerContext = ctx;
        }
    }
}
