using System;
using System.IO;
using System.Net;
using System.Net.Http;
using BusinessLogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

namespace TestProject
{
    [TestClass]
    public class ChangeTest
    {
        private static ChangeController changeController;

        [TestInitialize]
        public void Initialize()
        {
            AppInfo.path = "../../test_place/HorizonRestMetadataService.xml";
            AppInfo.MetadataRootFolder = "../../test_place/MetadataLocalFolder/";
            AppInfo.FolderLocation = "../../test_place/Projects/";
            changeController = new ChangeController();
        }

        #region GetHorizonVersions
        [TestMethod]
        public void GetHorizonVersionsSuccess()
        {
            Assert.IsNotNull(changeController.GetHorizonVersions());
        }

        [TestMethod]
        public void GetHorizonVersionsWrongPath()
        {
            AppInfo.MetadataRootFolder = "../test_place/MetadataLocalFolder/";
            Assert.IsNull(changeController.GetHorizonVersions());
        }
        #endregion

        #region LoadFile
        [TestMethod]
        public void LoadFileSuccess()
        {
            Assert.AreNotEqual(new HttpResponseMessage(HttpStatusCode.InternalServerError).StatusCode, changeController.LoadFile("515/13", "515/21").StatusCode);
        }

        [TestMethod]
        public void LoadFileCompareSame()
        {
            Assert.AreEqual(new HttpResponseMessage(HttpStatusCode.InternalServerError).StatusCode, changeController.LoadFile("515/13", "515/13").StatusCode);
        }

        [TestMethod]
        public void LoadFileWrongPath()
        {
            AppInfo.MetadataRootFolder = "test_place/MetadataLocalFolder/"; //file folder
            AppInfo.FolderLocation = "test_place/Projects/"; //folder where result should be saved
            Assert.AreEqual(new HttpResponseMessage(HttpStatusCode.InternalServerError).StatusCode, changeController.LoadFile("515/13", "515/21").StatusCode);
        }

        [TestMethod]
        public void LoadFileWrongVersionOrRelease()
        {
            Assert.AreEqual(new HttpResponseMessage(HttpStatusCode.InternalServerError).StatusCode, changeController.LoadFile("515/13", "515/2").StatusCode);
        }
        #endregion

        #region DiffColor
        [TestMethod]
        public void DiffColorSuccess()
        {
            Assert.AreNotEqual("",changeController.DiffColor("515/13/Virsgrāmata/TdmKonApgrSar/query.xml", "515/21/Virsgrāmata/TdmKonApgrSar/query.xml"));
        }

        [TestMethod]
        public void DiffColorCompareDifferentFiles()
        {
            Assert.AreEqual(true, changeController.DiffColor("515/13/Virsgrāmata/TdmKonApgrSar/query.xml", "515/21/Virsgrāmata/TdmKonApgrSar/query.xml").Contains("<span class='deleted'>"));
        }

        [TestMethod]
        public void DiffColorCompareSameFiles()
        {
            Assert.AreEqual(false, changeController.DiffColor("515/13/Virsgrāmata/TdmKonApgrSar/query.xml", "515/13/Virsgrāmata/TdmKonApgrSar/query.xml").Contains("<span class='deleted'>"));
        }

        [TestMethod]
        public void DiffColorWrongPath()
        {
            Assert.AreNotEqual("", changeController.DiffColor("515/13/Virsgrāmata/TdmKonApgrSar", "515/21/Virsgrāmata/TdmKonApgrSar/query.xml"));
        }

        [TestMethod]
        public void DiffColorWrongPaths()
        {
            Assert.AreNotEqual("", changeController.DiffColor("515/13/Virsgrāmata/TdmKonApgrSar", "515/21/Virsgrāmata/TdmKonApgrSar/query.xml"));
        }
        #endregion

        #region GetFile
        [TestMethod]
        public void GetFileSuccess()
        {
            Assert.IsNotNull(changeController.GetFile("515/13/metadata.xml"));
        }

        [TestMethod]
        public void GetFileWrongPath()
        {
            Assert.IsNull(changeController.GetFile("515/13"));
        }
        #endregion
    }
}
