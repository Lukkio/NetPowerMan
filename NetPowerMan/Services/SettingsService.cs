using NetPowerMan.Interfaces;
using NetPowerMan.Models;
using NetPowerMan.Properties;
using NetPowerMan.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace NetPowerMan.Services
{
    internal class SettingsService
    {
        private const string fileName = "Settings.xml";
        private readonly ILogger _logger;
        private readonly IShowMessage _showMessage;
        public SettingsService(ILogger logger, IShowMessage showMessage) 
        {
            _logger = logger;
            _showMessage = showMessage;
        }
        public int ReadSettingsAttributes(string _xmlPath, out Dictionary<string,string> _listsettings) 
        {
            _listsettings = new Dictionary<string,string>();

            try
            {
                XmlTextReader Reader = new XmlTextReader(fileName);

                while (Reader.Read())
                {
                    if (Reader.NodeType == XmlNodeType.Element)
                    {
                        if (Reader.Name == _xmlPath)
                        {
                            Reader.ReadAttributeValue();
                            for (int i = 0; i < Reader.AttributeCount; i++)
                            {
                                Reader.MoveToNextAttribute();
                                _listsettings.Add(Reader.Name, Reader.Value);
                            }
                        }
                    }
                }
                Reader.Close();
            }
            catch (Exception ex)
            {
                _showMessage.ShowMessageError(ex.Message, "Error");
                _logger.Error(ex);
                _listsettings = null;
                return ex.HResult;
            }
            return 0;                   
        }
        public int ReadAllDevicesAttributes(string _xmlPath,out List<DeviceModel>  _listdevices)
        {
            _listdevices = new List<DeviceModel>();

            Dictionary<string, string> _singledevice;

            try
            {
                XmlTextReader Reader = new XmlTextReader(fileName);

                while (Reader.Read())
                {
                    if (Reader.NodeType == XmlNodeType.Element)
                    {
                        if (Reader.Name == _xmlPath)
                        {
                            Reader.ReadAttributeValue();
                            _singledevice = new Dictionary<string, string>();
                            for (int i = 0; i < Reader.AttributeCount; i++)
                            {
                                Reader.MoveToNextAttribute();
                                _singledevice.Add(Reader.Name, Reader.Value);
                            }
                            _listdevices.Add( new DeviceModel(_singledevice, _logger, _showMessage));
                        }
                    }
                }
                Reader.Close();
            }
            catch (Exception ex)
            {
                _showMessage.ShowMessageError(ex.Message, "Error");
                _logger.Error(ex);
                _listdevices = null;
                return ex.HResult;
            }
            return 0;
        }
        public void SettingsWrite(ObservableCollection<DeviceModel> DevicesUpdated)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);

                XmlNodeList aNodes = xmlDocument.SelectNodes("/Root/Device");

                foreach (XmlNode aNode in aNodes)
                {
                    aNode.RemoveAll();
                }

                for (int i = aNodes.Count - 1; i >= 0; i--)
                {
                    aNodes[i].ParentNode.RemoveChild(aNodes[i]);
                }

                XmlNode root = xmlDocument.SelectSingleNode("Root", null);

                foreach (var devicesModels in DevicesUpdated)
                {
                    XmlElement profElement = xmlDocument.CreateElement("Device");
                    profElement.SetAttribute("ID", devicesModels.ID);
                    profElement.SetAttribute("Name", devicesModels.Name);
                    profElement.SetAttribute("IP", devicesModels.IP);
                    profElement.SetAttribute("MAC", devicesModels.MAC);
                    profElement.SetAttribute("QueryMode", devicesModels.QueryMode.ToString());
                    profElement.SetAttribute("Enabled", devicesModels.Enabled.ToString());
                    profElement.SetAttribute("DefaultShutDown", devicesModels.DefaultShutDown.ToString());
                    profElement.SetAttribute("DefaultReboot", devicesModels.DefaultReboot.ToString());
                    profElement.SetAttribute("User", devicesModels.User);
                    profElement.SetAttribute("Password", devicesModels.Password);
                    profElement.SetAttribute("Message", devicesModels.Message);
                    profElement.SetAttribute("MessageTimeout", devicesModels.MessageTimeout.ToString());
                    root.AppendChild(profElement);
                }
                xmlDocument.Save(fileName);
            }
            catch (XmlException ex)
            {
                _logger.Error("SettingsWrite:" + ex.Message);
                _showMessage.ShowMessageError(ex.Message, "Error!");
            }
        }
        public void SettingsWriteGlobalSettings(string AttributeName, string AttributeValue)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);

                XmlNode aNode = xmlDocument.SelectSingleNode("/Root/Setting");

                XmlAttribute xmlAttribute = xmlDocument.CreateAttribute(AttributeName);
                xmlAttribute.Value = AttributeValue;

                SetOrAddAttribute(aNode, xmlAttribute);

                xmlDocument.Save(fileName);
            }
            catch (XmlException ex)
            {
                _logger.Error(ex);
                _showMessage.ShowMessageError(ex.Message, "Error");
            }
        }
        private void SetOrAddAttribute(XmlNode node, params XmlAttribute[] attrList)
        {
            foreach (var attr in attrList)
            {
                if (node.Attributes[attr.Name] != null)
                {
                    node.Attributes[attr.Name].Value = attr.Value;
                }
                else
                {
                    node.Attributes.Append(attr);
                }
            }
        }
    }
}
