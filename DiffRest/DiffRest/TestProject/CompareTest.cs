using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BusinessLogic;
using Models;

namespace TestProject
{
    [TestClass]
    public class CompareTest
    {
        private static CompareFiles compareFiles;

        [TestInitialize]
        public void Initialize()
        {
            AppInfo.path = "../../test_place/HorizonRestMetadataService.xml";
            AppInfo.MetadataRootFolder = "../../test_place/MetadataLocalFolder/";
            AppInfo.FolderLocation = "../../test_place/Projects/";
            compareFiles = new CompareFiles();
        }

        #region Compare
        [TestMethod]
        public void CompareSuccess()
        {
            Assert.IsNotNull(compareFiles.Compare("515/13", "515/21", true, true, true));
        }

        [TestMethod]
        public void CompareSameFilesDontShow_NoChange()
        {
            //has added!
            Assert.AreEqual(0, compareFiles.Compare("515/13", "515/13", false, false, false).Count);
        }

        [TestMethod]
        public void CompareWrongVersion()
        {
            Assert.IsNull(compareFiles.Compare("515/1", "515/21", false, false, false));
        }
        #endregion
    }
}
