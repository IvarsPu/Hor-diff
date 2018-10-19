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
        public void CompareSameFilesFalseTo_NoChange_Added_IgnorNamespace()
        {
            Assert.AreEqual(0, compareFiles.Compare("515/13", "515/13", false, false, false).Count);
        }

        [TestMethod]
        public void CompareSameFilesFalseTo_NoChange_Added()
        {
            Assert.AreEqual(0, compareFiles.Compare("515/13", "515/13", false, false, true).Count);
        }

        [TestMethod]
        public void CompareSameFilesFalseTo_NoChange_IgnorNamespace()
        {
            Assert.AreEqual(0, compareFiles.Compare("515/13", "515/13", false, true, false).Count);
        }

        [TestMethod]
        public void CompareSameFilesFalseTo_NoChange()
        {
            Assert.AreEqual(0, compareFiles.Compare("515/13", "515/13", false, true, true).Count);
        }

        [TestMethod]
        public void CompareSameFilesFalseTo_Added_IgnorNamespace()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/13", true, false, false).Find(r => !r.Status.Equals("not changed") && r.ResourceList.Exists(s =>  !s.Status.Equals("not changed"))));
        }

        [TestMethod]
        public void CompareSameFilesFalseTo_IgnorNamespace()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/13", true, false, false).Find(r => !r.Status.Equals("not changed") && r.ResourceList.Exists(s => !s.Status.Equals("not changed"))));
        }

        [TestMethod]
        public void CompareSameFilesFalseTo_Added()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/13", true, false, false).Find(r => !r.Status.Equals("not changed") && r.ResourceList.Exists(s => !s.Status.Equals("not changed"))));
        }
        #endregion Same Files

        #region Different files
        [TestMethod]
        public void CompareDifferentFilesFalseTo_NoChange_Added_IgnorNamespace()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", false, false, false).Find(r => !r.Status.Equals("eddited") && r.ResourceList.Exists(s => !s.Status.Equals("eddited"))));
        }

        [TestMethod]
        public void CompareDifferentFilesFalseTo_NoChange_Added()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", false, false, true).Find(r => !r.Status.Equals("eddited") && r.ResourceList.Exists(s => !s.Status.Equals("eddited"))));
        }

        [TestMethod]
        public void CompareDifferentFilesFalseTo_NoChange_IgnorNamespace()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", false, true, false).Find(r => !r.Status.Equals("eddited") && !r.Status.Equals("added") && r.ResourceList.Exists(s => !s.Status.Equals("eddited") && !s.Status.Equals("added"))));
        }

        [TestMethod]
        public void CompareDifferentFilesFalseTo_NoChange()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", false, true, true).Find(r => !r.Status.Equals("eddited") && !r.Status.Equals("added") && r.ResourceList.Exists(s => !s.Status.Equals("eddited") && !s.Status.Equals("added"))));
        }

        [TestMethod]
        public void CompareDifferentFilesFalseTo_Added_IgnorNamespace()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", true, false, false).Find(r => !r.Status.Equals("eddited") && !r.Status.Equals("not changed") && !r.Status.Equals("removed") && r.ResourceList.Exists(s => !s.Status.Equals("eddited") && !s.Status.Equals("not changed") && !s.Status.Equals("removed"))));
        }

        [TestMethod]
        public void CompareDifferentFilesFalseTo_IgnorNamespace()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", true, true, false).Find(r => !r.Status.Equals("eddited") && !r.Status.Equals("not changed") && !r.Status.Equals("added") && !r.Status.Equals("removed") && r.ResourceList.Exists(s => !s.Status.Equals("eddited") && !s.Status.Equals("not changed") && !s.Status.Equals("added") && !s.Status.Equals("removed"))));
        }

        [TestMethod]
        public void CompareDifferentFilesFalseTo_Added()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", true, false, true).Find(r => !r.Status.Equals("eddited") && !r.Status.Equals("not changed") && !r.Status.Equals("removed") && r.ResourceList.Exists(s => !s.Status.Equals("eddited") && !s.Status.Equals("not changed") && !s.Status.Equals("removed"))));
        }
        #endregion
    }
}
