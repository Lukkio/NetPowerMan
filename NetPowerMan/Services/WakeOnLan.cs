using System.Collections.Generic;
using System.Linq;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Linq.Expressions;
using System.Windows;
using NLog;

namespace NetPowerMan.Services
{
    public static class Network
    {
        public static void WakeOnLan(string macAddress)
        {
            byte[] magicPacket = BuildMagicPacket(macAddress);
            if (magicPacket == null) return;
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces().Where((n) =>
                n.NetworkInterfaceType != NetworkInterfaceType.Loopback && n.OperationalStatus == OperationalStatus.Up))
            {
                IPInterfaceProperties iPInterfaceProperties = networkInterface.GetIPProperties();
                foreach (MulticastIPAddressInformation multicastIPAddressInformation in iPInterfaceProperties.MulticastAddresses)
                {
                    IPAddress multicastIpAddress = multicastIPAddressInformation.Address;
                    if (multicastIpAddress.ToString().StartsWith("ff02::1%", StringComparison.OrdinalIgnoreCase)) // Ipv6: All hosts on LAN (with zone index)
                    {
                        UnicastIPAddressInformation unicastIPAddressInformation = iPInterfaceProperties.UnicastAddresses.Where((u) =>
                            u.Address.AddressFamily == AddressFamily.InterNetworkV6 && !u.Address.IsIPv6LinkLocal).FirstOrDefault();
                        if (unicastIPAddressInformation != null)
                        {
                            SendWakeOnLan(unicastIPAddressInformation.Address, multicastIpAddress, magicPacket);
                        }
                    }
                    else if (multicastIpAddress.ToString().Equals("224.0.0.1")) // Ipv4: All hosts on LAN
                    {
                        UnicastIPAddressInformation unicastIPAddressInformation = iPInterfaceProperties.UnicastAddresses.Where((u) =>
                            u.Address.AddressFamily == AddressFamily.InterNetwork && !iPInterfaceProperties.GetIPv4Properties().IsAutomaticPrivateAddressingActive).FirstOrDefault();
                        if (unicastIPAddressInformation != null)
                        {
                            SendWakeOnLan(unicastIPAddressInformation.Address, multicastIpAddress, magicPacket);
                        }
                    }
                }
            }
        }

        static byte[] BuildMagicPacket(string macAddress) // MacAddress in any standard HEX format
        {
            //macAddress = Regex.Replace(macAddress, "[: -]", "");
            byte[] macBytes;
            try 
            { 
                macAddress = Regex.Replace(macAddress, "[:]", "-"); 
                macBytes = PhysicalAddress.Parse(macAddress).GetAddressBytes();
            }
            catch (SystemException ex) 
            {
                MessageBox.Show(ex.Message + ": " + macAddress);
                return null;
            }

            //byte[] macBytes = Convert.FromHexString(macAddress);
            

            IEnumerable<byte> header = Enumerable.Repeat((byte)0xff, 6); //First 6 times 0xff
            IEnumerable<byte> data = Enumerable.Repeat(macBytes, 16).SelectMany(m => m); // then 16 times MacAddress
            return header.Concat(data).ToArray();
        }

        static void SendWakeOnLan(IPAddress localIpAddress, IPAddress multicastIpAddress, byte[] magicPacket)
        {
            UdpClient client = new UdpClient(new IPEndPoint(localIpAddress, 0));
            client.Send(magicPacket, magicPacket.Length, new IPEndPoint(multicastIpAddress, 9));
        }
    }
}
