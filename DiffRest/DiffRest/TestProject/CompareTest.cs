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
        //compareFiles.Compare("515/13", "515/21", false, false, false).Find(r => !r.Status.Equals("eddited") || r.ResourceList.Find(s => !s.Status.Equals("eddited"))
        private static CompareFiles compareFiles;

        [TestInitialize]
        public void Initialize()
        {
            AppInfo.path = "../../test_place/HorizonRestMetadataService.xml";
            AppInfo.MetadataRootFolder = "../../test_place/MetadataLocalFolder/";
            AppInfo.FolderLocation = "../../test_place/Projects/";
            compareFiles = new CompareFiles();
        }
        
        [TestMethod]
        public void CompareSuccess()
        {
            Assert.IsNotNull(compareFiles.Compare("515/13", "515/21", true, true, true));
        }

        [TestMethod]
        public void CompareWrongVersion()
        {
            Assert.IsNull(compareFiles.Compare("515/1", "515/21", false, false, false));
        }

        #region Same files
        [TestMethod]
        public void CompareSameFilesDontShow_NoChange_Added_IgnorNamespace()
        {
            Assert.AreEqual(0, compareFiles.Compare("515/13", "515/13", false, false, false).Count);
        }

        [TestMethod]
        public void CompareSameFilesDontShow_NoChange_Added()
        {
            Assert.AreEqual(0, compareFiles.Compare("515/13", "515/13", false, false, true).Count);
        }

        [TestMethod]
        public void CompareSameFilesDontShow_NoChange_IgnorNamespace()
        {
            Assert.AreEqual(0, compareFiles.Compare("515/13", "515/13", false, true, false).Count);
        }

        [TestMethod]
        public void CompareSameFilesDontShow_NoChange()
        {
            Assert.AreEqual(0, compareFiles.Compare("515/13", "515/13", false, true, true).Count);
        }

        [TestMethod]
        public void CompareSameFilesDontShow_Added_IgnorNamespace()
        {
            Assert.AreNotEqual(0, compareFiles.Compare("515/13", "515/13", true, false, false).Count);
        }

        [TestMethod]
        public void CompareSameFilesDontShow_IgnorNamespace()
        {
            Assert.AreEqual(compareFiles.Compare("515/13", "515/13", true, false, false).Count, compareFiles.Compare("515/13", "515/13", true, true, false).Count);
        }

        [TestMethod]
        public void CompareSameFilesDontShow_Added()
        {
            Assert.AreEqual(compareFiles.Compare("515/13", "515/13", true, false, false).Count, compareFiles.Compare("515/13", "515/13", true, false, true).Count);
        }
        #endregion Same Files

        #region Different files
        [TestMethod]
        public void CompareDifferentFilesDontShow_NoChange_Added_IgnorNamespace()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", false, false, false).Find(r => !r.Status.Equals("eddited")));
        }

        [TestMethod]
        public void CompareDifferentFilesDontShow_NoChange_Added()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", false, false, true).Find(r => !r.Status.Equals("eddited")));
        }

        [TestMethod]
        public void CompareDifferentFilesDontShow_NoChange_IgnorNamespace()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", false, true, false).Find(r => !r.Status.Equals("eddited") && !r.Status.Equals("added")));
        }

        [TestMethod]
        public void CompareDifferentFilesDontShow_NoChange()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", false, true, true).Find(r => !r.Status.Equals("eddited") && !r.Status.Equals("added")));
        }

        [TestMethod]
        public void CompareDifferentFilesDontShow_Added_IgnorNamespace()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", true, false, false).Find(r => !r.Status.Equals("eddited") && !r.Status.Equals("no change") && !r.Status.Equals("removed")));
        }

        [TestMethod]
        public void CompareDifferentFilesDontShow_IgnorNamespace()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", true, true, false).Find(r => !r.Status.Equals("eddited") && !r.Status.Equals("no change") && !r.Status.Equals("added") && !r.Status.Equals("removed")));
        }

        [TestMethod]
        public void CompareDifferentFilesDontShow_Added()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", true, false, true).Find(r => !r.Status.Equals("eddited") && !r.Status.Equals("no change") && !r.Status.Equals("removed")));
        }
        #endregion
    }
}
