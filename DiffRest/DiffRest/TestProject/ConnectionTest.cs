using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BusinessLogic;
using Models;

namespace TestProject
{
    [TestClass]
    public class ConnectionTest
    {
        private static Connection connection;
        private static RestConnection restConnection;

        [TestInitialize]
        public void Initialize()
        {
            AppInfo.path = "../../test_place/HorizonRestMetadataService.xml";
            AppInfo.MetadataRootFolder = "../../test_place/MetadataLocalFolder/";
            AppInfo.FolderLocation = "../../test_place/Projects/";
            connection = new Connection();

            restConnection = new RestConnection();
            restConnection.Id = 3;
            restConnection.Name = "test";
            restConnection.Password = "123";
            restConnection.Url = "www.tvnet.lv";
            restConnection.Username = "tester";
        }

        #region GetServerConn
        [TestMethod]
        public void GetServerConnSuccess()
        {
            Assert.IsNotNull(connection.GetServerConn(1));
        }

        [TestMethod]
        public void GetServerConnWrongId()
        {
            Assert.IsNull(connection.GetServerConn(0));
        }

        [TestMethod]
        public void GetServerConnWrongPath()
        {
            AppInfo.path = "../test_place/HorizonRestMetadataService.xml";
            Assert.IsNull(connection.GetServerConn(1));
        }
        #endregion

        #region CreateServerConn
        [TestMethod]
        public void CreateServerConnSuccess()
        {
            Assert.AreEqual(true,connection.CreateServerConn(restConnection));
            connection.DeleteServerConn(restConnection.Id);
        }

        [TestMethod]
        public void CreateServerConnAlreadyExists()
        {
            connection.CreateServerConn(restConnection);
            Assert.AreEqual(false, connection.CreateServerConn(restConnection));
            connection.DeleteServerConn(restConnection.Id);
        }
        #endregion

        #region DeleteServerConn
        [TestMethod]
        public void DeleteServerConnSuccess()
        {
            connection.CreateServerConn(restConnection);
            Assert.AreEqual(true, connection.DeleteServerConn(restConnection.Id));
        }

        [TestMethod]
        public void DeleteServerConnDoesntExist()
        {
            Assert.AreEqual(false, connection.DeleteServerConn(restConnection.Id));
        }

        [TestMethod]
        public void DeleteServerConnWrongPath()
        {
            AppInfo.path = "../test_place/HorizonRestMetadataService.xml";
            Assert.AreEqual(false, connection.DeleteServerConn(restConnection.Id));
        }
        #endregion

        #region EditServerConn
        [TestMethod]
        public void EditServerConnSuccess()
        {
            connection.CreateServerConn(restConnection);
            Assert.AreEqual(true, connection.EditServerConn(restConnection));
            connection.DeleteServerConn(restConnection.Id);
        }

        [TestMethod]
        public void EditServerConnAlreadyExistss()
        {
            connection.CreateServerConn(restConnection);
            restConnection.Id = restConnection.Id + 1;
            restConnection.Url = "www.tvnet.com";
            connection.CreateServerConn(restConnection);
            restConnection.Url = "www.tvnet.lv";
            Assert.AreEqual(false, connection.EditServerConn(restConnection));
            connection.DeleteServerConn(restConnection.Id);
            connection.DeleteServerConn(restConnection.Id - 1);
        }

        [TestMethod]
        public void EditServerConnWrongPath()
        {
            AppInfo.path = "../test_place/HorizonRestMetadataService.xml";
            Assert.AreEqual(false, connection.EditServerConn(restConnection));
        }
        #endregion

        #region GetConnections
        [TestMethod]
        public void GetConnectionsSuccess()
        {
            Assert.AreNotEqual(0, connection.GetConnections().Count);
        }

        [TestMethod]
        public void GetConnectionsWrongPath()
        {
            AppInfo.path = "../test_place/HorizonRestMetadataService.xml";
            Assert.AreEqual(null, connection.GetConnections());
        }
        #endregion
    }
}
