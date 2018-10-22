using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BusinessLogic;
using Models;
using System.Linq;

namespace TestProject
{
    [TestClass]
    public class CompareTest
    {
        private static readonly string noChangeStatus = "not changed", addedStatus = "added", editStatus = "eddited", removeStatus = "removed";
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
            Assert.IsNull(compareFiles.Compare("515/13", "515/13", true, false, false).Find(r => !r.Status.Equals(noChangeStatus) && r.ResourceList.Exists(s =>  !s.Status.Equals(noChangeStatus))));
        }

        [TestMethod]
        public void CompareSameFilesFalseTo_IgnorNamespace()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/13", true, false, false).Find(r => !r.Status.Equals(noChangeStatus) && r.ResourceList.Exists(s => !s.Status.Equals(noChangeStatus))));
        }

        [TestMethod]
        public void CompareSameFilesFalseTo_Added()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/13", true, false, false).Find(r => !r.Status.Equals(noChangeStatus) && r.ResourceList.Exists(s => !s.Status.Equals(noChangeStatus))));
        }
        #endregion Same Files

        #region Different files
        [TestMethod]
        public void CompareDifferentFilesFalseTo_NoChange_Added_IgnorNamespace()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", false, false, false).Find(r => !r.Status.Equals(editStatus) && r.ResourceList.Exists(s => !s.Status.Equals(editStatus))));
        }

        [TestMethod]
        public void CompareDifferentFilesFalseTo_NoChange_Added()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", false, false, true).Find(r => !r.Status.Equals(editStatus) && r.ResourceList.Exists(s => !s.Status.Equals(editStatus))));
        }

        [TestMethod]
        public void CompareDifferentFilesFalseTo_NoChange_IgnorNamespace()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", false, true, false).Find(r => !r.Status.Equals(editStatus) && !r.Status.Equals(addedStatus) && r.ResourceList.Exists(s => !s.Status.Equals(editStatus) && !s.Status.Equals(addedStatus))));
        }

        [TestMethod]
        public void CompareDifferentFilesFalseTo_NoChange()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", false, true, true).Find(r => !r.Status.Equals(editStatus) && !r.Status.Equals(addedStatus) && r.ResourceList.Exists(s => !s.Status.Equals(editStatus) && !s.Status.Equals(addedStatus))));
        }

        [TestMethod]
        public void CompareDifferentFilesFalseTo_Added_IgnorNamespace()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", true, false, false).Find(r => !r.Status.Equals(editStatus) && !r.Status.Equals(noChangeStatus) && !r.Status.Equals(removeStatus) && r.ResourceList.Exists(s => !s.Status.Equals(editStatus) && !s.Status.Equals(noChangeStatus) && !s.Status.Equals(removeStatus))));
        }

        [TestMethod]
        public void CompareDifferentFilesFalseTo_IgnorNamespace()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", true, true, false).Find(r => !r.Status.Equals(editStatus) && !r.Status.Equals(noChangeStatus) && !r.Status.Equals(addedStatus) && !r.Status.Equals(removeStatus) && r.ResourceList.Exists(s => !s.Status.Equals(editStatus) && !s.Status.Equals(noChangeStatus) && !s.Status.Equals(addedStatus) && !s.Status.Equals(removeStatus))));
        }

        [TestMethod]
        public void CompareDifferentFilesFalseTo_Added()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", true, false, true).Find(r => !r.Status.Equals(editStatus) && !r.Status.Equals(noChangeStatus) && !r.Status.Equals(removeStatus) && r.ResourceList.Exists(s => !s.Status.Equals(editStatus) && !s.Status.Equals(noChangeStatus) && !s.Status.Equals(removeStatus))));
        }
        #endregion

        [TestMethod]
        public void CheckStatusText()
        {
            Assert.IsNull(compareFiles.Compare("515/13", "515/21", true, true, true).Find(x =>
            !x.ResourceList.All(o => o.Status.Equals(x.ResourceList[0].Status)) && !x.Status.Equals(editStatus) ||
            x.ResourceList.All(o => o.Status.Equals(x.ResourceList[0].Status)) && !x.Status.Equals(x.ResourceList[0].Status)));
        }
    }
}
